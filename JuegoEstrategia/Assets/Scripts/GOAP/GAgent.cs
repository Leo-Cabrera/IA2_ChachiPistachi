
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// NUEVO: representa un objetivo que el agente puede perseguir.
public class SubGoal
{
    public Dictionary<string, int> sgoals;
    public bool remove;

    public SubGoal(string s, int i, bool r)
    {
        sgoals = new Dictionary<string, int>();
        sgoals.Add(s, i);
        remove = r;
    }
}

public class GAgent : MonoBehaviour
{
    public List<GAction> actions = new List<GAction>();
    public Dictionary<SubGoal, int> goals = new Dictionary<SubGoal, int>();

    GPlanner planner;
    Queue<GAction> actionQueue;

    [HideInInspector] public GAction currentAction;
    [HideInInspector] public GAction lastAction;

    SubGoal currentGoal;

    public event System.Action<GAgent> OnTurnFinished;

    private bool turnFinished = true;
    private bool planningAttempted = false;
    private bool invoke = false;

    [HideInInspector] public UnidadHex unidad;

    protected virtual void Start()
    {
        unidad = GetComponent<UnidadHex>();

        GAction[] acts = GetComponents<GAction>();

        foreach (GAction a in acts)
        {
            if (!actions.Contains(a))
                actions.Add(a);
        }
    }

    private void CompleteAction()
    {
        if (currentAction == null)
            return;

        bool success = currentAction.PostPerform();

        lastAction = currentAction;

        currentAction.running = false;
        currentAction = null;

        invoke = false;

        if (!success)
        {
            actionQueue = null;
            FinishTurn();
        }
    }

    private void LateUpdate()
    {
        if (turnFinished)
            return;

        if (unidad == null)
        {
            FinishTurn();
            return;
        }

        // Esperamos a que termine la acción que se está ejecutando.
        if (currentAction != null && currentAction.running)
        {
            if (currentAction.finished && !invoke)
            {
                Invoke(nameof(CompleteAction), currentAction.duration);
                invoke = true;
            }

            return;
        }

        if (unidad.puntosMovimientoRestantes <= 0)
        {
            FinishTurn();
            return;
        }

        // Si ya no quedan acciones en el plan, comprobamos el resultado.
        if (actionQueue != null && actionQueue.Count == 0)
        {
            if (currentGoal != null &&
                currentGoal.remove &&
                lastAction != null &&
                lastAction.goalSatisfied)
            {
                goals.Remove(currentGoal);
            }

            FinishTurn();
            return;
        }

        // Ejecutamos la siguiente acción.
        if (actionQueue != null && actionQueue.Count > 0)
        {
            currentAction = actionQueue.Dequeue();

            if (currentAction.PrePerform())
            {
                currentAction.running = true;
                currentAction.finished = false;
                currentAction.IniciateAction();
            }
            else
            {
                // NUEVO: si una acción no puede empezar, se termina el turno.
                // Evita entrar en ciclos de planificación.
                currentAction = null;
                actionQueue = null;
                FinishTurn();
            }
        }
    }

    public void StartTurn()
    {
        if (!turnFinished)
            return;

        if (unidad == null)
            unidad = GetComponent<UnidadHex>();

        if (unidad == null)
        {
            Debug.LogError("El agente no tiene una UnidadHex.");
            return;
        }

        Debug.Log("START TURN: " + unidad.datosTropa.nombreTropa);

        turnFinished = false;
        planningAttempted = false;

        planner = null;
        actionQueue = null;
        currentAction = null;
        lastAction = null;
        currentGoal = null;

        CancelInvoke();

        PlanTurn();
    }

    private void PlanTurn()
    {
        if (turnFinished || planningAttempted)
            return;

        planningAttempted = true;

        planner = new GPlanner();

        // NUEVO: mezclamos el orden de los objetivos.
        // Así el NPC no elige siempre la misma alternativa.
        List<SubGoal> availableGoals = goals.Keys.ToList();

        for (int i = availableGoals.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            SubGoal temp = availableGoals[i];
            availableGoals[i] = availableGoals[j];
            availableGoals[j] = temp;
        }

        foreach (SubGoal sg in availableGoals)
        {
            actionQueue = planner.plan(actions, sg.sgoals, null);

            if (actionQueue != null && actionQueue.Count > 0)
            {
                currentGoal = sg;
                Debug.Log("Objetivo elegido por " +
                          unidad.datosTropa.nombreTropa);
                break;
            }
        }

        if (actionQueue == null || actionQueue.Count == 0)
            FinishTurn();
    }

    private void FinishTurn()
    {
        if (turnFinished)
            return;

        turnFinished = true;

        CancelInvoke();

        OnTurnFinished?.Invoke(this);
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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

    [HideInInspector] public int unitIndex;

    [HideInInspector] public WorldStates beliefs;


    protected virtual void Start()
    {
        unidad = GetComponent<UnidadHex>();

        GAction[] acts = GetComponents<GAction>();

        foreach (GAction a in acts)
        {
            if (!actions.Contains(a))
                actions.Add(a);
        }

        unitIndex = GWorld.Instance.AddUnitAndGetIndex(gameObject);

        beliefs = new WorldStates();
        beliefs.SetState("turnoIntacto", 1);
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

        // Esperamos a que termine la acción actual.
        if (currentAction != null && currentAction.running)
        {
            if (currentAction.finished && !invoke)
            {
                Invoke(nameof(CompleteAction), currentAction.duration);
                invoke = true;
            }

            return;
        }

        // Si se han agotado los puntos, termina el turno.
        if (unidad.puntosMovimientoRestantes <= 0)
        {
            FinishTurn();
            return;
        }

        // Si no quedan acciones en el plan, comprobamos el objetivo.
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
                // REGISTRO CENTRALIZADO DE ACCIONES.
                // No depende de las condiciones del Inspector.
                bool esStayStill = currentAction is StayStill;

                unidad.RegistrarAccionGOAP(esStayStill);

                currentAction.running = true;
                currentAction.finished = false;

                currentAction.IniciateAction();
            }
            else
            {
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

        // Reiniciamos el registro de acciones para este turno.
        unidad.ReiniciarRegistroAccionesGOAP();

        beliefs.SetState("turnoIntacto", 1);

        RefreshBeliefs();

        PlanTurn();
    }


    private void PlanTurn()
    {
        if (turnFinished || planningAttempted)
            return;

        planningAttempted = true;

        planner = new GPlanner();

        // Conservamos las prioridades de cada tropa.
        // Solo aleatorizamos los objetivos con igual prioridad.
        List<SubGoal> availableGoals = goals.Keys
            .OrderByDescending(g => goals[g])
            .ThenBy(g => Random.value)
            .ToList();

        foreach (SubGoal sg in availableGoals)
        {
            actionQueue = planner.plan(actions, sg.sgoals, beliefs);

            if (actionQueue != null && actionQueue.Count > 0)
            {
                currentGoal = sg;
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


    public void RefreshBeliefs()
    {
        int pm = unidad.puntosMovimientoRestantes;

        beliefs.SetState("puntosMovimiento", pm);

        if (pm > 0)
            beliefs.SetState("puedeMoverse", 1);
        else
            beliefs.RemoveState("puedeMoverse");
    }
}
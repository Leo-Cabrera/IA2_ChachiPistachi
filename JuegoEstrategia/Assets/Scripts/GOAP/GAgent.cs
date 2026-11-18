using UnityEngine;
using System.Collections.Generic;
using System.Collections;
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
    private bool plannigAtempted = false;
    [HideInInspector] public UnidadHex unidad;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        unidad = GetComponent<UnidadHex>();
        GAction[] acts = this.GetComponents<GAction>();
        foreach (GAction a in acts)
        {
            actions.Add(a);
        }
    }

    bool invoke = false;

    void CompleteAction()
    {
        if (currentAction == null) return;

        bool success = currentAction.PostPerform();
        lastAction = currentAction;
        currentAction.running = false;
        invoke = false;

        if (!success)
        {
            actionQueue = null;
        }

        currentAction = null;
    }

    void LateUpdate()
    {
        if (turnFinished) return;

        //metodo para revisar si la accion en curso ha terminado
        if(currentAction != null && currentAction.running)
        {
            if (currentAction.finished)//si tiene un camino y ha llegado al sitio, o sea ha terminado
            {
                if (!invoke)
                {
                    Invoke("CompleteAction", currentAction.duration);
                    invoke= true;
                }
            }
            return;
        }

        if(unidad.puntosMovimientoRestantes <= 0)
        {
            FinishTurn();
            return;
        }

        //revisar si se han terminado las acciones del plan para eliminar el plan
        if (actionQueue != null && actionQueue.Count == 0)
        {
            if (currentGoal.remove && GoalSatisfied(currentAction))
            {
                
                goals.Remove(currentGoal);
            }
            
            FinishTurn();
            return;
        }

        //activar siguiente accion del plan
        if (actionQueue != null && actionQueue.Count > 0)
        {
            currentAction = actionQueue.Dequeue();
            if (currentAction.PrePerform())
            {
                if (!currentAction.running)
                {
                    currentAction.running = true;
                    currentAction.IniciateAction();
                }
            }

            else
            {
                currentAction = null;
                actionQueue = null;
                currentGoal = null;

                plannigAtempted = false;
                PlanTurn();
                return;
            }
        }
    }

    public void StartTurn()
    {
        Debug.Log("START TURN: " + unidad.datosTropa.nombreTropa);
        Debug.Log("Objetivos disponibles: " + goals.Count);

        if(!turnFinished) return;

        turnFinished = false;
        plannigAtempted = false;

        planner = null;
        actionQueue = null;
        currentAction = null;

        PlanTurn();
    }
    private void PlanTurn()
    {
        if(turnFinished || plannigAtempted) return;
        plannigAtempted = true;

        planner = new GPlanner();

        var sortedGoals = from entry in goals orderby entry.Value descending select entry;

        foreach (KeyValuePair<SubGoal, int> sg in sortedGoals)
        {
            actionQueue = planner.plan(actions, sg.Key.sgoals, null);
            if (actionQueue != null)
            {
                currentGoal = sg.Key;
                break;
            }
        }

        if(actionQueue == null || actionQueue.Count == 0)
        {
            FinishTurn();
        }
    }

    private void FinishTurn()
    {
        if(turnFinished) return;
        turnFinished = true;
        OnTurnFinished?.Invoke(this);
    }

    private bool GoalSatisfied(GAction goal)
    {
        if (goal == null) return false;
        return goal.goalSatisfied;
        
    }
}

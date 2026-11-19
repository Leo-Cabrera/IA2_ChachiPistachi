
using UnityEngine;
using System.Collections.Generic;

public class Node
{
    public Node parent;
    public float cost;
    public Dictionary<string, int> state;
    public GAction action;

    public Node(Node parent, float cost, Dictionary<string, int> allStates, GAction action)
    {
        this.parent = parent;
        this.cost = cost;
        this.state = new Dictionary<string, int>(allStates);
        this.action = action;
    }
}

public class GPlanner
{
    public Queue<GAction> plan(
        List<GAction> actions,
        Dictionary<string, int> goal,
        WorldStates states)
    {
        List<GAction> usableActions = new List<GAction>();

        foreach (GAction a in actions)
        {
            if (a.IsAchievable())
                usableActions.Add(a);
        }

        List<Node> leaves = new List<Node>();

        // Se parte de una copia de los estados globales.
        Node start = new Node(
            null,
            0,
            GWorld.Instance.GetWorld().GetStates(),
            null
        );

        bool success = BuildGraph(start, leaves, usableActions, goal);

        if (!success)
        {
            Debug.Log("NO PLAN");
            return null;
        }

        Node cheapest = null;

        foreach (Node leaf in leaves)
        {
            if (cheapest == null || leaf.cost < cheapest.cost)
                cheapest = leaf;
        }

        List<GAction> result = new List<GAction>();

        Node n = cheapest;

        while (n != null)
        {
            if (n.action != null)
                result.Insert(0, n.action);

            n = n.parent;
        }

        Queue<GAction> queue = new Queue<GAction>();

        foreach (GAction a in result)
            queue.Enqueue(a);

        Debug.Log("Plan GOAP:");

        foreach (GAction a in queue)
            Debug.Log("Acción: " + a.actionName);

        return queue;
    }

    private bool BuildGraph(
        Node parent,
        List<Node> leaves,
        List<GAction> usableActions,
        Dictionary<string, int> goal)
    {
        bool foundPath = false;

        foreach (GAction action in usableActions)
        {
            if (!action.IsAchievableGiven(parent.state))
                continue;

            Dictionary<string, int> currentState =
                new Dictionary<string, int>(parent.state);

            // MODIFICADO: ahora los efectos actualizan también las claves
            // que ya existían en el estado.
            foreach (KeyValuePair<string, int> eff in action.effects)
            {
                currentState[eff.Key] = eff.Value;
            }

            Node node = new Node(
                parent,
                parent.cost + action.cost,
                currentState,
                action
            );

            if (GoalAchieved(goal, currentState))
            {
                leaves.Add(node);
                foundPath = true;
            }
            else
            {
                List<GAction> subset = ActionSubset(usableActions, action);

                if (BuildGraph(node, leaves, subset, goal))
                    foundPath = true;
            }
        }

        return foundPath;
    }

    private bool GoalAchieved(
        Dictionary<string, int> goal,
        Dictionary<string, int> state)
    {
        // CONSERVADO: los objetivos se consideran alcanzados cuando
        // existen sus claves. No se comparan sus valores todavía.
        foreach (KeyValuePair<string, int> g in goal)
        {
            if (!state.ContainsKey(g.Key))
                return false;
        }

        return true;
    }

    private List<GAction> ActionSubset(List<GAction> actions, GAction removeMe)
    {
        List<GAction> subset = new List<GAction>();

        foreach (GAction a in actions)
        {
            if (a != removeMe)
                subset.Add(a);
        }

        return subset;
    }
}

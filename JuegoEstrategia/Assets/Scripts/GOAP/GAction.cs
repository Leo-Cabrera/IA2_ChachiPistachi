
using UnityEngine;
using System.Collections.Generic;

public abstract class GAction : MonoBehaviour
{
    public string actionName = "Action";
    public float cost = 1.0f;
    public float duration = 0f;

    public WorldState[] preConditions;
    public WorldState[] afterEffects;

    public Dictionary<string, int> preconditions;
    public Dictionary<string, int> effects;

    

    [HideInInspector] public bool running = false;
    [HideInInspector] public bool finished = false;
    [HideInInspector] public bool goalSatisfied = false;
    [HideInInspector] public UnidadHex unidad;
    [HideInInspector] public GAgent agent;

    public GAction()
    {
        preconditions = new Dictionary<string, int>();
        effects = new Dictionary<string, int>();
    }

    public void Awake()
    {
        // MODIFICADO: limpiamos los diccionarios para evitar claves duplicadas
        // si la acción vuelve a inicializarse. 
        preconditions.Clear();
        effects.Clear();

        if (preConditions != null)
        {
            foreach (WorldState w in preConditions)
            {
                if (w != null && !preconditions.ContainsKey(w.key))
                    preconditions.Add(w.key, w.value);
            }
        }

        if (afterEffects != null)
        {
            foreach (WorldState w in afterEffects)
            {
                if (w != null && !effects.ContainsKey(w.key))
                    effects.Add(w.key, w.value);
            }
        }

        unidad = GetComponent<UnidadHex>();
        agent = GetComponent<GAgent>();
    }

    public virtual bool IsAchievable()
    {
        return true;
    }

    public bool IsAchievableGiven(Dictionary<string, int> conditions)
    {
        // CONSERVADO: se comprueba la existencia de la clave.
        
        foreach (KeyValuePair<string, int> p in preconditions)
        {
            if (!conditions.TryGetValue(p.Key, out int v)) return false;
            if (v != p.Value) return false;
        }

        return true;
    }

    public abstract bool PrePerform();
    public abstract bool PostPerform();
    public abstract void IniciateAction();
}

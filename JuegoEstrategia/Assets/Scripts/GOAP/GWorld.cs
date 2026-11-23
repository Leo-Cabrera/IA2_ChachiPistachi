using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public sealed class GWorld
{
    private static readonly GWorld instance = new GWorld();
    private static WorldStates world;

    public static List<GameObject> EnemiesUnits;
    public static List<GameObject> PlayerUnits;
    public static List<GameObject> EnemiesTargets;

    static GWorld()
    {
        world = new WorldStates();
        EnemiesUnits = new List<GameObject>();
        PlayerUnits = new List<GameObject>();
        EnemiesTargets = new List<GameObject>();
    }

    private GWorld() { }

    public static GWorld Instance
    {
        get { return instance; }

    }

    public WorldStates GetWorld()
    {
        return world;
    }


    public int AddUnitAndGetIndex(GameObject unit)
    {
        if (EnemiesUnits == null) EnemiesUnits = new List<GameObject>();
        EnemiesUnits.Add(unit);
        EnemiesTargets.Add(null);

        return EnemiesUnits.Count - 1;
    }

    public void AddAllyUnit(GameObject unit)
    {
        if (PlayerUnits == null) PlayerUnits = new List<GameObject>();
        PlayerUnits.Add(unit);
    }

    

    
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Plantilla : GAction
{
    public override bool PrePerform()
    {
        base.goalSatisfied = false;
        return true;
    }

    public override bool PostPerform()
    {
        agent.RefreshBeliefs();
        base.goalSatisfied = true;
        return true;
    }

    public override void IniciateAction()
    {
        finished = false;
        Debug.Log(base.unidad.datosTropa.nombreTropa + " ");




        finished = true;
    }
}

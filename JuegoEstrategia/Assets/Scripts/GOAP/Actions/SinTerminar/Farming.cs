
using UnityEngine;
using System.Collections;

public class Farming : GAction
{
    public override bool PrePerform()
    {

        base.goalSatisfied = false;

        return base.unidad != null &&
            agent.beliefs.HasState("turnoIntacto");
       

    }

    public override bool PostPerform()
    {
        base.unidad.puntosMovimientoRestantes = 0;
        agent.RefreshBeliefs();
        base.goalSatisfied = true;
        return true;
    }

    public override void IniciateAction()
    {
        finished = false;
        Debug.Log(
            base.unidad.datosTropa.nombreTropa +
            " ha recogido recursos."
        );
        

        StartCoroutine(FarmingCoroutine());
    }

    private IEnumerator FarmingCoroutine()
    {
        if (base.unidad == null)
        {
            finished = true;
            yield break;
        }

        base.unidad.puntosMovimientoRestantes = 0;

        yield return new WaitForSeconds(base.duration);

        finished = true;
    }
}

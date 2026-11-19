
using UnityEngine;
using System.Collections;

public class Farming : GAction
{
    public override bool PrePerform()
    {
        base.goalSatisfied = false;

        return base.unidad != null &&
               base.unidad.puntosMovimientoRestantes ==
               base.unidad.puntosMovimientoMaximos;
    }

    public override bool PostPerform()
    {
        base.goalSatisfied = true;
        return true;
    }

    public override void IniciateAction()
    {
        finished = false;

        Debug.Log("Ha recogido recursos.");

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

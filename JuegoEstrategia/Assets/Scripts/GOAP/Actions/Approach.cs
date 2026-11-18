using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Approach : GAction
{
    
    public override bool PrePerform()
    {
        base.goalSatisfied = false;
        return base.unidad != null && base.unidad.puntosMovimientoRestantes > 0;
       
    }

    public override bool PostPerform()
    {
        return true;
    }

    public override void IniciateAction()
    {
        finished = false;
        StartCoroutine(ApproachCoroutine());

        
    }

    private IEnumerator ApproachCoroutine()
    {
        if (base.unidad == null)
        {
            finished = true;
            yield break;
        }

        if (base.unidad.puntosMovimientoRestantes <= 0)
        {
            Debug.Log(base.unidad.datosTropa.nombreTropa + " no tiene movimiento.");
            base.goalSatisfied = true;
            finished = true;
            yield break;
        }

        List<HexTile> ruta = ControlUnidades.instance.evaluador.ElegirRuta(
                base.unidad,
                ControlUnidades.instance.tropasJugador
            );

        if (ruta.Count <= 1)
        {
            Debug.Log(base.unidad.datosTropa.nombreTropa + " se queda quieto.");
            finished = true;
            yield break;
        }

        for (int i = 1; i < ruta.Count; i++)
        {
            ControlUnidades.instance.movimientoEnCurso = true;
            yield return StartCoroutine(ControlUnidades.instance.MoverUnidad(base.unidad, ruta[i]));

        }

        ///////base.unidad.puntosMovimientoRestantes -= ruta.Count;
        base.unidad.yaSeHaMovido = true;

        Debug.Log("accion de movimiento");


        finished = true;
    }


}

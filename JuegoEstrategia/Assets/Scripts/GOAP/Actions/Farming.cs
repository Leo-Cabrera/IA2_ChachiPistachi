using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Farming : GAction
{
    
    public override bool PrePerform()
    {
        base.goalSatisfied = false;
        return base.unidad.puntosMovimientoRestantes == base.unidad.puntosMovimientoMaximos;
       

    }

    public override bool PostPerform()
    {
        base.goalSatisfied = true;
        return true;
    }

    public override void IniciateAction()
    {
        finished = false;

        Debug.Log("ha farmeado");

        //aqui pones el codigo que se ejecuta al farmear. Lo de abajo es una corutina que usa el A*
        //para moverse, si no la necesitas y lo haces todo el una sola funcion la borras y te aseguras
        //de poner finished = true al final y ya
        StartCoroutine(FarmingCoroutine());

        
    }

    private IEnumerator FarmingCoroutine()
    {
        
        base.unidad.puntosMovimientoRestantes = 0; 
        yield return new WaitForSeconds(base.duration); // Simula el tiempo que tarda en farmear
        finished = true;
    }


}

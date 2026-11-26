using UnityEngine;

public class StayStill : GAction
{
    public override bool PrePerform()
    {
        base.goalSatisfied = false;

        return base.unidad != null &&
               agent.beliefs.HasState("turnoIntacto");
    }


    public override bool PostPerform()
    {
        if (base.unidad == null)
            return false;

        base.unidad.puntosMovimientoRestantes = 0;

        agent.RefreshBeliefs();

        base.goalSatisfied = true;

        return true;
    }


    public override void IniciateAction()
    {
        finished = false;

        if (base.unidad == null)
        {
            finished = true;
            return;
        }

        // Consume todo el movimiento.
        base.unidad.puntosMovimientoRestantes = 0;

        // La unidad ha terminado su turno.
        base.unidad.yaSeHaMovido = true;

        // No modificamos seHaDesplazadoEsteTurno.
        // No se ha movido realmente.

        base.goalSatisfied = true;

        agent.RefreshBeliefs();

        Debug.Log(
            base.unidad.datosTropa.nombreTropa +
            " ha pasado su turno con StayStill."
        );

        finished = true;
    }
}
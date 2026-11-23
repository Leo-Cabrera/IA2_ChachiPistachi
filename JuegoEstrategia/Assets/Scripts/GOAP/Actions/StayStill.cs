
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
        base.unidad.puntosMovimientoRestantes = 0;
        agent.RefreshBeliefs();
        return true;
    }

    public override void IniciateAction()
    {
        finished = false;

        // NUEVO: consume el movimiento restante porque el NPC
        // ha decidido quedarse quieto durante este turno.
        base.unidad.puntosMovimientoRestantes = 0;

        // NUEVO: marca la unidad como terminada.
        base.unidad.yaSeHaMovido = true;

        // NUEVO: indica que la acción de quedarse quieto
        // se ha completado correctamente.
        base.goalSatisfied = true;

        Debug.Log(
            base.unidad.datosTropa.nombreTropa +
            " ha decidido quedarse quieto."
        );

        finished = true;
    }
}

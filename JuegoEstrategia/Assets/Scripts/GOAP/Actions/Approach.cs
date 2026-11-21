
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Approach : GAction
{
    public override bool PrePerform()
    {
        base.goalSatisfied = false;

        return base.unidad != null &&
               !agent.beliefs.HasState("puedeMoverse");
    }

    public override bool PostPerform()
    {
        agent.RefreshBeliefs();
        agent.beliefs.RemoveState("turnoIntacto"); //turno usado
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
            agent.RefreshBeliefs();
            yield break;
        }

        // NUEVO: si ya está junto a un enemigo, no necesita avanzar.
        if (EstaJuntoAEnemigo())
        {
            finished = true;
            yield break;
        }

        if (base.unidad.puntosMovimientoRestantes <= 0)
        {


            Debug.Log(base.unidad.datosTropa.nombreTropa + " se queda quieto.");
            finished = true;
            agent.RefreshBeliefs();
            yield break;
        }


        ControlUnidades control = ControlUnidades.instance;

        if (control == null || control.evaluador == null)
        {
            Debug.LogError("No se ha encontrado ControlUnidades o EvaluadorTactico.");
            finished = true;
            agent.RefreshBeliefs();
            yield break;
        }

        // El evaluador elige el mejor destino alcanzable.
        List<HexTile> ruta = control.evaluador.ElegirRuta(
            base.unidad,
            control.tropasJugador
        );

        if (ruta == null || ruta.Count <= 1)
        {
            Debug.Log(base.unidad.datosTropa.nombreTropa +
                      " no puede avanzar más este turno.");

            // NUEVO: si no puede avanzar, termina su turno.
            base.unidad.puntosMovimientoRestantes = 0;
            base.unidad.yaSeHaMovido = true;

            ActualizarPreparadoAtacar();

            finished = true;
            yield break;
        }

        // NUEVO: recorremos la ruta hasta agotar los puntos
        // o alcanzar una posición vecina al enemigo.
        for (int i = 1; i < ruta.Count; i++)
        {
            if (base.unidad.puntosMovimientoRestantes <= 0)
                break;

            control.movimientoEnCurso = true;

            yield return StartCoroutine(
                control.MoverUnidad(base.unidad, ruta[i])
            );

            // Si ya hemos llegado junto al jugador, no seguimos avanzando.
            if (ActualizarPreparadoAtacar())
                break;
        }


        //base.unidad.puntosMovimientoRestantes -= ruta.Count; //por ahora resta puntos por casilla, cambiar segun terreno
        base.unidad.yaSeHaMovido = true;

        // NUEVO: solo consideramos alcanzado el objetivo
        // cuando realmente estamos junto a un jugador.
        ActualizarPreparadoAtacar();

        Debug.Log(
            base.unidad.datosTropa.nombreTropa +
            " ha terminado su aproximación."
        );

        finished = true;
    }

    // NUEVO: comprueba si existe algún jugador en una casilla vecina.
    private bool EstaJuntoAEnemigo()
    {
        ControlUnidades control = ControlUnidades.instance;

        if (control == null || control.grid == null ||
            control.evaluador == null)
            return false;

        HexTile casillaActual = control.grid.ObtenerCasilla(
            base.unidad.coordenadaX,
            base.unidad.coordenadaY
        );

        if (casillaActual == null)
            return false;

        foreach (UnidadHex jugador in control.tropasJugador)
        {
            if (jugador == null)
                continue;

            int distancia = control.evaluador.DistanciaHex(
                casillaActual,
                jugador
            );

            if (distancia == 1)
                return true;
        }

        return false;
    }
    private bool ActualizarPreparadoAtacar()
    {
        bool estaJuntoAEnemigo = EstaJuntoAEnemigo();
        if (estaJuntoAEnemigo) {
            agent.beliefs.SetState("preparadoAtacar", 1);
            base.goalSatisfied = true;
            agent.RefreshBeliefs();
            return true;
        }
            
        else
        {
            agent.beliefs.RemoveState("preparadoAtacar");
            base.goalSatisfied = false;
            agent.RefreshBeliefs();
            return false;
        }
    }
}

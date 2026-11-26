using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Farming : GAction
{
    private bool monedaRecogida = false;


    // El planificador solo podrá utilizar Farming
    // cuando exista una moneda con una ruta válida.
    public override bool IsAchievable()
    {
        if (base.unidad == null)
            return false;

        ControlUnidades control = ControlUnidades.instance;

        if (control == null ||
            control.evaluador == null)
        {
            return false;
        }

        HexTile monedaObjetivo;

        List<HexTile> ruta =
            control.evaluador.ElegirRutaHaciaMoneda(
                base.unidad,
                out monedaObjetivo
            );

        return monedaObjetivo != null &&
               ruta != null &&
               ruta.Count > 0;
    }


    public override bool PrePerform()
    {
        base.goalSatisfied = false;

        monedaRecogida = false;

        return base.unidad != null &&
               agent.beliefs.HasState("turnoIntacto");
    }


    public override bool PostPerform()
    {
        if (base.unidad == null)
            return false;

        base.unidad.puntosMovimientoRestantes = 0;

        agent.RefreshBeliefs();

        // Si no ha recogido la moneda, el objetivo no se elimina.
        base.goalSatisfied = monedaRecogida;

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

        Debug.Log(
            base.unidad.datosTropa.nombreTropa +
            " comienza a buscar monedas."
        );

        StartCoroutine(FarmingCoroutine());
    }


    private IEnumerator FarmingCoroutine()
    {
        UnidadHex unidad = base.unidad;

        if (unidad == null)
        {
            finished = true;
            yield break;
        }

        ControlUnidades control = ControlUnidades.instance;

        if (control == null ||
            control.evaluador == null ||
            control.grid == null ||
            control.gestorMonedas == null)
        {
            Debug.LogError(
                "[Farming] Faltan referencias en ControlUnidades."
            );

            unidad.puntosMovimientoRestantes = 0;
            finished = true;
            yield break;
        }

        HexTile casillaActual = control.grid.ObtenerCasilla(
            unidad.coordenadaX,
            unidad.coordenadaY
        );

        // Comprobamos si ya estamos sobre una moneda.
        if (casillaActual != null &&
            casillaActual.monedaEnCasilla != null)
        {
            control.gestorMonedas.RecogerMoneda(
                casillaActual,
                unidad
            );

            monedaRecogida = true;

            unidad.puntosMovimientoRestantes = 0;
            unidad.yaSeHaMovido = true;

            agent.RefreshBeliefs();

            finished = true;
            yield break;
        }

        HexTile monedaObjetivo;

        List<HexTile> ruta =
            control.evaluador.ElegirRutaHaciaMoneda(
                unidad,
                out monedaObjetivo
            );

        if (ruta == null ||
            ruta.Count <= 1 ||
            monedaObjetivo == null)
        {
            Debug.Log(
                "[Farming] No encuentra una ruta válida."
            );

            unidad.puntosMovimientoRestantes = 0;
            unidad.yaSeHaMovido = true;

            agent.RefreshBeliefs();

            finished = true;
            yield break;
        }

        Debug.Log(
            "[Farming] " +
            unidad.datosTropa.nombreTropa +
            " se dirige a la moneda de (" +
            monedaObjetivo.coordenadaX + "," +
            monedaObjetivo.coordenadaY + ")."
        );

        for (int i = 1; i < ruta.Count; i++)
        {
            if (unidad.puntosMovimientoRestantes <= 0)
                break;

            HexTile siguiente = ruta[i];

            if (siguiente == null)
                break;

            int coste = control.ObtenerCosteMovimiento(
                unidad,
                siguiente
            );

            if (coste == int.MaxValue ||
                coste > unidad.puntosMovimientoRestantes)
            {
                break;
            }

            control.movimientoEnCurso = true;

            yield return StartCoroutine(
                control.MoverUnidad(unidad, siguiente)
            );

            if (unidad.coordenadaX != siguiente.coordenadaX ||
                unidad.coordenadaY != siguiente.coordenadaY)
            {
                break;
            }

            // MoverUnidad ya recoge la moneda automáticamente.
            if (siguiente == monedaObjetivo &&
                siguiente.monedaEnCasilla == null)
            {
                monedaRecogida = true;

                Debug.Log(
                    "[Farming] " +
                    unidad.datosTropa.nombreTropa +
                    " ha recogido una moneda."
                );

                break;
            }
        }

        unidad.puntosMovimientoRestantes = 0;
        unidad.yaSeHaMovido = true;

        agent.RefreshBeliefs();

        if (!monedaRecogida)
        {
            Debug.Log(
                "[Farming] Continuará buscando la moneda " +
                "en el siguiente turno."
            );
        }

        yield return new WaitForSeconds(base.duration);

        finished = true;
    }
}
using UnityEngine;
using System.Collections.Generic;

public class EvaluadorTactico : MonoBehaviour
{
    public HexGrid grid;
    public MovimientoHex movimiento;

    [Header("Pesos")]
    public float pesoDistancia = 1f;
    public float pesoCoste = 0.1f;
    public float pesoPeligro = 1f;
    public float pesoVentajaTerreno = 1f;
    public float pesoTerrenoObjetivo = 0.5f;

    [Header("Debug")]
    public bool debugCostes = false;


    // =========================================================
    // APROXIMACIÓN A ENEMIGOS
    // =========================================================

    public List<HexTile> ElegirRuta(
        UnidadHex enemigo,
        List<UnidadHex> objetivos)
    {
        if (enemigo == null ||
            objetivos == null ||
            objetivos.Count == 0)
        {
            return new List<HexTile>();
        }

        HexTile casillaEnemigo = grid.ObtenerCasilla(
            enemigo.coordenadaX,
            enemigo.coordenadaY
        );

        if (casillaEnemigo == null)
            return new List<HexTile>();

        Dictionary<HexTile, float> alcanzables =
            movimiento.CalcularCasillasAlcanzables(
                enemigo,
                enemigo.puntosMovimientoRestantes
            );

        if (alcanzables.Count == 0)
            return new List<HexTile>();

        HexTile mejorCasilla = null;
        UnidadHex mejorObjetivo = null;

        float mejorPuntuacion = float.MinValue;
        float puntuacionActual = float.MinValue;

        foreach (UnidadHex objetivo in objetivos)
        {
            if (objetivo == null)
                continue;

            float puntuacion = PuntuarCasilla(
                casillaEnemigo,
                0f,
                enemigo,
                objetivo
            );

            if (puntuacion > puntuacionActual)
                puntuacionActual = puntuacion;
        }

        foreach (var par in alcanzables)
        {
            HexTile casilla = par.Key;

            if (casilla == casillaEnemigo)
                continue;

            float coste = par.Value;

            foreach (UnidadHex objetivo in objetivos)
            {
                if (objetivo == null)
                    continue;

                float puntuacion = PuntuarCasilla(
                    casilla,
                    coste,
                    enemigo,
                    objetivo
                );

                if (EsMejor(
                    casilla,
                    objetivo,
                    puntuacion,
                    mejorCasilla,
                    mejorObjetivo,
                    mejorPuntuacion))
                {
                    mejorPuntuacion = puntuacion;
                    mejorCasilla = casilla;
                    mejorObjetivo = objetivo;
                }
            }
        }

        if (mejorCasilla == null)
            return new List<HexTile>();

        if (mejorPuntuacion <= puntuacionActual + 0.05f)
        {
            Debug.Log(
                "[IA] Se moverá igualmente para probar la ruta."
            );
        }

        List<HexTile> ruta = movimiento.CalcularRuta(
            casillaEnemigo,
            mejorCasilla,
            enemigo
        );

        return ruta;
    }


    // =========================================================
    // NUEVO: BÚSQUEDA DE MONEDAS
    // =========================================================

    public List<HexTile> ElegirRutaHaciaMoneda(
        UnidadHex unidad,
        out HexTile monedaObjetivo)
    {
        monedaObjetivo = null;

        if (unidad == null ||
            grid == null ||
            movimiento == null)
        {
            return new List<HexTile>();
        }

        HexTile casillaInicial = grid.ObtenerCasilla(
            unidad.coordenadaX,
            unidad.coordenadaY
        );

        if (casillaInicial == null)
            return new List<HexTile>();

        HexTile[,] todasLasCasillas =
            grid.ObtenerTodasLasCasillas();

        if (todasLasCasillas == null)
            return new List<HexTile>();

        List<HexTile> mejorRuta = new List<HexTile>();

        float menorCoste = float.MaxValue;

        foreach (HexTile casilla in todasLasCasillas)
        {
            if (casilla == null ||
                casilla.monedaEnCasilla == null ||
                !casilla.esTransitable)
            {
                continue;
            }

            // Si la unidad ya está en la casilla de la moneda.
            if (casilla == casillaInicial)
            {
                monedaObjetivo = casilla;
                return new List<HexTile> { casillaInicial };
            }

            // Calculamos la ruta A* hasta la moneda.
            List<HexTile> ruta = movimiento.CalcularRuta(
                casillaInicial,
                casilla,
                unidad
            );

            if (ruta == null || ruta.Count <= 1)
                continue;

            float costeRuta = 0f;

            // Calculamos el coste completo de la ruta.
            for (int i = 1; i < ruta.Count; i++)
            {
                int coste = movimiento.pesosTerreno.ObtenerCoste(
                    unidad.datosTropa,
                    ruta[i].tipoTerreno
                );

                if (coste <= 0)
                {
                    costeRuta = float.MaxValue;
                    break;
                }

                costeRuta += coste;
            }

            if (costeRuta < menorCoste)
            {
                menorCoste = costeRuta;
                mejorRuta = ruta;
                monedaObjetivo = casilla;
            }
        }

        if (monedaObjetivo == null)
            return new List<HexTile>();

        if (debugCostes)
        {
            Debug.Log(
                "[Farming] Moneda elegida en (" +
                monedaObjetivo.coordenadaX + "," +
                monedaObjetivo.coordenadaY +
                "). Coste total: " + menorCoste
            );
        }

        return mejorRuta;
    }


    // =========================================================
    // PUNTUACIÓN TÁCTICA
    // =========================================================

    public float PuntuarCasilla(
        HexTile casilla,
        float coste,
        UnidadHex enemigo,
        UnidadHex objetivo)
    {
        if (casilla == null || objetivo == null)
            return float.MinValue;

        float puntuacion = 0f;

        puntuacion -= DistanciaHex(casilla, objetivo) *
                      pesoDistancia;

        puntuacion -= coste * pesoCoste;

        puntuacion -= CalcularPeligro(casilla, enemigo) *
                      pesoPeligro;

        int afinidadCasilla =
            movimiento.pesosTerreno.ObtenerAfinidad(
                enemigo.datosTropa,
                casilla.tipoTerreno
            );

        float ventajaCasilla = afinidadCasilla - 1;

        puntuacion += ventajaCasilla * pesoVentajaTerreno;

        HexTile casillaObjetivo = grid.ObtenerCasilla(
            objetivo.coordenadaX,
            objetivo.coordenadaY
        );

        if (casillaObjetivo != null)
        {
            int afinidadObjetivo =
                movimiento.pesosTerreno.ObtenerAfinidad(
                    enemigo.datosTropa,
                    casillaObjetivo.tipoTerreno
                );

            float ventajaObjetivo = afinidadObjetivo - 1;

            puntuacion += ventajaObjetivo *
                          pesoTerrenoObjetivo;
        }

        return puntuacion;
    }


    private bool EsMejor(
        HexTile candidata,
        UnidadHex objetivoCandidato,
        float puntuacion,
        HexTile actual,
        UnidadHex objetivoActual,
        float mejorPuntuacion)
    {
        if (actual == null)
            return true;

        const float epsilon = 0.001f;

        if (puntuacion > mejorPuntuacion + epsilon)
            return true;

        if (puntuacion < mejorPuntuacion - epsilon)
            return false;

        int distanciaCandidata =
            DistanciaHex(candidata, objetivoCandidato);

        int distanciaActual =
            DistanciaHex(actual, objetivoActual);

        if (distanciaCandidata != distanciaActual)
            return distanciaCandidata < distanciaActual;

        if (candidata.coordenadaX != actual.coordenadaX)
            return candidata.coordenadaX < actual.coordenadaX;

        if (candidata.coordenadaY != actual.coordenadaY)
            return candidata.coordenadaY < actual.coordenadaY;

        if (objetivoCandidato.coordenadaX !=
            objetivoActual.coordenadaX)
        {
            return objetivoCandidato.coordenadaX <
                   objetivoActual.coordenadaX;
        }

        return objetivoCandidato.coordenadaY <
               objetivoActual.coordenadaY;
    }


    public float CalcularPeligro(
        HexTile casilla,
        UnidadHex unidad)
    {
        List<HexTile> vecinas =
            movimiento.ObtenerVecinas(casilla);

        if (vecinas.Count == 0)
            return 1f;

        float peligro = 0f;

        foreach (HexTile v in vecinas)
        {
            if (!v.esTransitable ||
                movimiento.EstaOcupada(v, unidad))
            {
                peligro += 1f;
                continue;
            }

            int coste =
                movimiento.pesosTerreno.ObtenerCoste(
                    unidad.datosTropa,
                    v.tipoTerreno
                );

            if (coste <= 0)
            {
                peligro += 1f;
                continue;
            }

            peligro += Mathf.Clamp01((coste - 1) / 2f);
        }

        return peligro / vecinas.Count;
    }


    public int DistanciaHex(HexTile a, UnidadHex b)
    {
        if (a == null || b == null)
            return 999;

        HexTile c = grid.ObtenerCasilla(
            b.coordenadaX,
            b.coordenadaY
        );

        if (c == null)
            return 999;

        return DistanciaHexEntreCasillas(a, c);
    }


    private int DistanciaHexEntreCasillas(
        HexTile a,
        HexTile b)
    {
        int aq = a.coordenadaX;
        int ar = a.coordenadaY -
            (a.coordenadaX - (a.coordenadaX & 1)) / 2;

        int bq = b.coordenadaX;
        int br = b.coordenadaY -
            (b.coordenadaX - (b.coordenadaX & 1)) / 2;

        int as_ = -aq - ar;
        int bs = -bq - br;

        return (
            Mathf.Abs(aq - bq) +
            Mathf.Abs(ar - br) +
            Mathf.Abs(as_ - bs)
        ) / 2;
    }
}

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

    // Compara todos los objetivos posibles.
    public List<HexTile> ElegirRuta(
        UnidadHex enemigo,
        List<UnidadHex> objetivos)
    {
        if (enemigo == null || objetivos == null || objetivos.Count == 0) {
            return new List<HexTile>();
        }

        HexTile casillaEnemigo =
            grid.ObtenerCasilla(enemigo.coordenadaX, enemigo.coordenadaY);

        if (casillaEnemigo == null) {
            return new List<HexTile>();
        }

        Dictionary<HexTile, float> alcanzables =
            movimiento.CalcularCasillasAlcanzables(
                enemigo,
                enemigo.puntosMovimientoRestantes
            );

        if (alcanzables.Count == 0)
        {
            return new List<HexTile>();
        }

        HexTile mejorCasilla = null;
        UnidadHex mejorObjetivo = null;

        float mejorPuntuacion = float.MinValue;

        // Evaluamos también la posición actual.
        // Así podemos saber si realmente merece la pena moverse.

        float puntuacionActual = float.MinValue;

        foreach (UnidadHex objetivo in objetivos)
        {
            if (objetivo == null) continue;

            HexTile casillaObjetivo =
                grid.ObtenerCasilla(objetivo.coordenadaX, objetivo.coordenadaY);

            if (casillaObjetivo == null) continue;

            float puntuacion = PuntuarCasilla(
                casillaEnemigo,
                0f,
                enemigo,
                objetivo
            );

            if (puntuacion > puntuacionActual)
                puntuacionActual = puntuacion;
        }

        // Evaluamos todas las casillas alcanzables
        // frente a todos los objetivos.

        foreach (var par in alcanzables)
        {
            HexTile casilla = par.Key;

            if (casilla == casillaEnemigo) continue;

            float coste = par.Value;

            foreach (UnidadHex objetivo in objetivos)
            {
                if (objetivo == null) continue;

                HexTile casillaObjetivo =
                    grid.ObtenerCasilla(
                        objetivo.coordenadaX,
                        objetivo.coordenadaY
                    );

                if (casillaObjetivo == null) continue;

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
        {
            return new List<HexTile>();
        }

        // Evitamos movernos si la posición actual es mejor.
        if (mejorPuntuacion <= puntuacionActual + 0.05f)
        {
            Debug.Log("[IA] Se moverá igualmente para probar la ruta."); //ARREGLO TEMPORAL
        }

        // if (mejorPuntuacion <= puntuacionActual + 0.05f)
        // {
        //     if (debugCostes)
        //     {
        //         Debug.Log(
        //             "[IA] Se queda quieto. " +
        //             "Actual: " + puntuacionActual.ToString("F2") +
        //             " Mejor: " + mejorPuntuacion.ToString("F2") +
        //             " Casilla: " +
        //             (mejorCasilla != null
        //                 ? mejorCasilla.coordenadaX + "," + mejorCasilla.coordenadaY
        //                 : "NULL")
        //         );
        //     }

        //     Debug.LogError("[PRUEBA 6] Salgo porque mejorPuntuacion <= puntuacionActual + 0.05f");

        //     return new List<HexTile>();
        // }

        HexTile casillaInicial = grid.ObtenerCasilla(
            enemigo.coordenadaX,
            enemigo.coordenadaY
        );

        List<HexTile> ruta = movimiento.CalcularRuta(
            casillaInicial,
            mejorCasilla,
            enemigo
        );

        return ruta;

        if (debugCostes)
        {
            string r = "";

            for (int i = 1; i < ruta.Count; i++)
            {
                r += " -> (" +
                    ruta[i].coordenadaX + "," +
                    ruta[i].coordenadaY + ")" +
                    ruta[i].tipoTerreno +
                    "[" +
                    movimiento.pesosTerreno.ObtenerCoste(
                        enemigo.datosTropa,
                        ruta[i].tipoTerreno
                    ) +
                    "]";
            }
        }

        return ruta;
    }

    // Puntuación de una posición respecto a un objetivo.

    public float PuntuarCasilla(
        HexTile casilla,
        float coste,
        UnidadHex enemigo,
        UnidadHex objetivo)
    {
        if (casilla == null || objetivo == null)
            return float.MinValue;

        float puntuacion = 0f;

        // 1. Distancia al objetivo.

        puntuacion -= DistanciaHex(casilla, objetivo) * pesoDistancia;

        // 2. Coste de movimiento necesario para llegar.

        puntuacion -= coste * pesoCoste;

        // 3. Peligro de la posición.

        puntuacion -= CalcularPeligro(casilla, enemigo) * pesoPeligro;

        // 4. Afinidad del terreno donde termina el movimiento.
        // Favorito = +1
        // Neutral = 0

        int afinidadCasilla =
            movimiento.pesosTerreno.ObtenerAfinidad(
                enemigo.datosTropa,
                casilla.tipoTerreno
            );

        float ventajaCasilla = afinidadCasilla - 1;

        puntuacion += ventajaCasilla * pesoVentajaTerreno;

        // 5. Afinidad del terreno donde se encuentra el objetivo.
        // Esto ayuda a elegir entre dos jugadores equidistantes.

        HexTile casillaObjetivo =
            grid.ObtenerCasilla(
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

            puntuacion += ventajaObjetivo * pesoTerrenoObjetivo;
        }

        return puntuacion;
    }

    // Desempate estable.

    private bool EsMejor(
        HexTile candidata,
        UnidadHex objetivoCandidato,
        float puntuacion,
        HexTile actual,
        UnidadHex objetivoActual,
        float mejorPuntuacion)
    {
        if (actual == null) return true;

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

        // Último desempate por coordenadas del objetivo.

        if (objetivoCandidato.coordenadaX != objetivoActual.coordenadaX)
            return objetivoCandidato.coordenadaX < objetivoActual.coordenadaX;

        return objetivoCandidato.coordenadaY < objetivoActual.coordenadaY;
    }

    // Calcula el peligro de una casilla.

    public float CalcularPeligro(HexTile casilla, UnidadHex unidad)
    {
        List<HexTile> vecinas = movimiento.ObtenerVecinas(casilla);

        if (vecinas.Count == 0) return 1f;

        float peligro = 0f;

        foreach (HexTile v in vecinas)
        {
            if (!v.esTransitable || movimiento.EstaOcupada(v, unidad))
            {
                peligro += 1f;
                continue;
            }

            int coste = movimiento.pesosTerreno.ObtenerCoste(
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

    // Distancia hexagonal.

    public int DistanciaHex(HexTile a, UnidadHex b)
    {
        if (a == null || b == null) return 999;

        HexTile c = grid.ObtenerCasilla(
            b.coordenadaX,
            b.coordenadaY
        );

        if (c == null) return 999;

        return DistanciaHexEntreCasillas(a, c);
    }

    private int DistanciaHexEntreCasillas(HexTile a, HexTile b)
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

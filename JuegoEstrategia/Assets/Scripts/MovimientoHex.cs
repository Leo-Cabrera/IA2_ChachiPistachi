
using UnityEngine;
using System.Collections.Generic;

public class MovimientoHex : MonoBehaviour
{
    public HexGrid grid;

    // Devuelve las casillas vecinas de una casilla
    public List<HexTile> ObtenerVecinas(HexTile casilla)
    {
        List<HexTile> vecinas = new List<HexTile>();

        int x = casilla.coordenadaX;
        int y = casilla.coordenadaY;

        // Desplazamientos para columnas pares
        Vector2Int[] desplazamientosPares ={new Vector2Int(1, 0),new Vector2Int(1, -1),new Vector2Int(0, -1),new Vector2Int(-1, -1),new Vector2Int(-1, 0),new Vector2Int(0, 1)};

        // Desplazamientos para columnas impares
        Vector2Int[] desplazamientosImpares ={new Vector2Int(1, 1),new Vector2Int(1, 0),new Vector2Int(0, -1), new Vector2Int(-1, 0),new Vector2Int(-1, 1), new Vector2Int(0, 1)};

        Vector2Int[] desplazamientos =(x % 2 == 0) ? desplazamientosPares : desplazamientosImpares;

        foreach (Vector2Int desplazamiento in desplazamientos)
        {
            int nuevoX = x + desplazamiento.x;
            int nuevoY = y + desplazamiento.y;

            HexTile vecina = grid.ObtenerCasilla(nuevoX, nuevoY);

            if (vecina != null)
            {
                vecinas.Add(vecina);
            }
        }

        return vecinas;
    }


    
    public Dictionary<HexTile, float> CalcularCasillasAlcanzables(UnidadHex unidad)
    {
        // Guarda cada casilla y el coste necesario para llegar a ella
        Dictionary<HexTile, float> costes = new Dictionary<HexTile, float>();

        // Lista de casillas que todavía debemos explorar
        List<HexTile> pendientes = new List<HexTile>();

        // Obtiene la casilla donde está actualmente la unidad
        HexTile inicio = grid.ObtenerCasilla(unidad.coordenadaX, unidad.coordenadaY);

        if (inicio == null)
        {
            return costes;
        }

        // La casilla inicial tiene coste cero
        costes[inicio] = 0f;
        pendientes.Add(inicio);

        while (pendientes.Count > 0)
        {
            // Busca la casilla pendiente con menor coste acumulado
            HexTile actual = pendientes[0];

            foreach (HexTile casilla in pendientes)
            {
                if (costes[casilla] < costes[actual])
                {
                    actual = casilla;
                }
            }

            pendientes.Remove(actual);

            // Comprueba las seis casillas vecinas
            foreach (HexTile vecina in ObtenerVecinas(actual))
            {
                // No permite atravesar terrenos no transitables
                if (!vecina.esTransitable)
                {
                    continue;
                }

                // No permite atravesar casillas ocupadas por otras unidades
                if (EstaOcupada(vecina))
                {
                    continue;
                }

                // Calcula el coste acumulado para llegar a la vecina
                float nuevoCoste = costes[actual] + vecina.costeMovimiento;

                // No permite superar los puntos de movimiento disponibles
                if (nuevoCoste > unidad.puntosMovimiento)
                {
                    continue;
                }

                // Si no habíamos llegado antes o encontramos un camino más barato
                if (!costes.ContainsKey(vecina) || nuevoCoste < costes[vecina])
                {
                    costes[vecina] = nuevoCoste;

                    if (!pendientes.Contains(vecina))
                    {
                        pendientes.Add(vecina);
                    }
                }
            }
        }

        return costes;
    }

    bool EstaOcupada(HexTile casilla)
    {
        UnidadHex[] unidades = FindObjectsOfType<UnidadHex>();

        foreach (UnidadHex unidad in unidades)
        {
            if (unidad.coordenadaX == casilla.coordenadaX &&
                unidad.coordenadaY == casilla.coordenadaY)
            {
                return true;
            }
        }

        return false;
    }

}


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
                if (nuevoCoste > unidad.puntosMovimientoRestantes)
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

    public bool EstaOcupada(HexTile casilla)
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

    public List<HexTile> CalcularRuta(HexTile inicio, HexTile destino, UnidadHex unidad)
    {
        List<HexTile> abierta = new List<HexTile>();
        HashSet<HexTile> cerrada = new HashSet<HexTile>();

        Dictionary<HexTile, float> g = new Dictionary<HexTile, float>();
        Dictionary<HexTile, HexTile> padre = new Dictionary<HexTile, HexTile>();

        // comprobación de seguridad: si inicio o destino son null, devuelve lista vacía
        if (inicio == null || destino == null) {
            return new List<HexTile>();
        }

        g[inicio] = 0f;
        abierta.Add(inicio);

        int exploradas = 0;

        while (abierta.Count > 0)
        {
            // 1. encontrar en abierta la casilla con menor f
            HexTile actual = abierta[0];
            float mejorF = CalcularF(actual, destino, g);

            foreach (HexTile casilla in abierta) {
                float f = CalcularF(casilla, destino, g);
                if (f < mejorF)
                {
                    mejorF = f;
                    actual = casilla;
                }
            }
            // 2. si es el destino -> return ReconstruirRuta(padre, destino)

            if (actual == destino) {
                Debug.Log("Casillas exploradas: " + exploradas);

                return ReconstruirRuta(padre, destino);
            }

            // 3. quitarla de abierta, añadirla a cerrada

            abierta.Remove(actual);
            exploradas++;
            cerrada.Add(actual);

            // 4. recorrer ObtenerVecinas(actual) y aplicar los filtros y la mejora

            List<HexTile> vecinas = ObtenerVecinas(actual);

            foreach (HexTile vecina in vecinas)
            {
                if (cerrada.Contains(vecina))
                    continue;

                if (!vecina.esTransitable)
                    continue;

                if (EstaOcupada(vecina) && vecina != destino)
                    continue;

                float costePaso = vecina.costeMovimiento;
                float gTentativo = g[actual] + costePaso;

                if (g.ContainsKey(vecina) && gTentativo >= g[vecina])
                    continue;

                padre[vecina] = actual;
                g[vecina] = gTentativo;

                if (!abierta.Contains(vecina))
                    abierta.Add(vecina);
            }

        }

         Debug.Log("Casillas exploradas: " + exploradas);

        return new List<HexTile>();  // sin ruta
    }

    private float CalcularF(HexTile casilla, HexTile destino, Dictionary<HexTile, float> g)
    {
        return g[casilla] + Heuristica(casilla, destino);
    }

    private float Heuristica(HexTile a, HexTile b)
    {
        int aq = a.coordenadaX;
        int ar = a.coordenadaY - (a.coordenadaX - (a.coordenadaX & 1)) / 2;

        int bq = b.coordenadaX;
        int br = b.coordenadaY - (b.coordenadaX - (b.coordenadaX & 1)) / 2;

        int as_ = -aq - ar;
        int bs = -bq - br;

        int distancia = (Mathf.Abs(aq - bq) + Mathf.Abs(ar - br) + Mathf.Abs(as_ - bs)) / 2;

        return distancia * 1f;  // coste mínimo por paso

        // return 0f;
    }


    private List<HexTile> ReconstruirRuta(Dictionary<HexTile, HexTile> padre, HexTile destino)
    {
        List<HexTile> ruta = new List<HexTile>();
        HexTile actual = destino;

        while (actual != null)
        {
            ruta.Add(actual);

            if (!padre.ContainsKey(actual))
            {
                break;
            }

            actual = padre[actual];
        }

        ruta.Reverse();
        return ruta;
    }




}

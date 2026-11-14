
using UnityEngine;
using System.Collections.Generic;

public class MovimientoHex : MonoBehaviour
{
    public HexGrid grid;

    public PesosTerreno pesosTerreno;

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
        Dictionary<HexTile, float> alcanzables = new Dictionary<HexTile, float>();
        Queue<HexTile> pendientes = new Queue<HexTile>();

        HexTile inicio = grid.ObtenerCasilla(unidad.coordenadaX, unidad.coordenadaY);

        if (inicio == null)
            return alcanzables;

        int alcance = pesosTerreno.ObtenerPeso(unidad.datosTropa, inicio.tipoTerreno);

        Debug.Log("Tropa: " + unidad.datosTropa.nombreTropa +" | Nivel: " + unidad.datosTropa.nivel +" | Terreno inicial: " + inicio.tipoTerreno +" | Influencia: " + alcance);

        if (alcance <= 0)
            return alcanzables;

        alcanzables[inicio] = 0;
        pendientes.Enqueue(inicio);

        while (pendientes.Count > 0)
        {
            HexTile actual = pendientes.Dequeue();
            int distancia = Mathf.CeilToInt(alcanzables[actual]);

            if (distancia >= alcance)
                continue;

            foreach (HexTile vecina in ObtenerVecinas(actual))
            {
                if (!vecina.esTransitable || EstaOcupada(vecina, unidad))
                    continue;

                int influenciaVecina = pesosTerreno.ObtenerPeso(unidad.datosTropa, vecina.tipoTerreno);

                if (influenciaVecina <= 0)
                    continue;

                int nuevaDistancia = distancia + 1;

                if (!alcanzables.ContainsKey(vecina) || nuevaDistancia < alcanzables[vecina])
                {
                    alcanzables[vecina] = nuevaDistancia;
                    pendientes.Enqueue(vecina);
                }
            }
        }

        return alcanzables;
    }

    public bool EstaOcupada(HexTile casilla, UnidadHex unidadActual)
    {
        UnidadHex[] unidades = FindObjectsOfType<UnidadHex>();

        foreach (UnidadHex unidad in unidades)
        {
            if (unidad == unidadActual)
                continue;

            if (unidad.coordenadaX == casilla.coordenadaX && unidad.coordenadaY == casilla.coordenadaY)
                return true;
        }

        return false;
    }

    public bool EstaOcupada(HexTile casilla)
    {
        UnidadHex[] unidades = FindObjectsOfType<UnidadHex>();

        foreach (UnidadHex unidad in unidades)
        {
            if (unidad.coordenadaX == casilla.coordenadaX && unidad.coordenadaY == casilla.coordenadaY)
                return true;
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

                if (EstaOcupada(vecina, unidad))
                    continue;

                int peso = pesosTerreno.ObtenerPeso(unidad.datosTropa, vecina.tipoTerreno);
                if (peso == 0) continue;
                float costePaso = peso;
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

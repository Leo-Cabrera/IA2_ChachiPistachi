using UnityEngine;
using System.Collections.Generic;

public class MovimientoHex : MonoBehaviour
{
    public HexGrid grid;
    public PesosTerreno pesosTerreno;

    public List<HexTile> ObtenerVecinas(HexTile casilla)
    {
        List<HexTile> vecinas = new List<HexTile>();

        int x = casilla.coordenadaX;
        int y = casilla.coordenadaY;

        Vector2Int[] desplazamientosPares = { new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(0, -1), new Vector2Int(-1, -1), new Vector2Int(-1, 0), new Vector2Int(0, 1) };
        Vector2Int[] desplazamientosImpares = { new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0, 1) };

        Vector2Int[] desplazamientos = (x % 2 == 0) ? desplazamientosPares : desplazamientosImpares;

        foreach (Vector2Int desplazamiento in desplazamientos)
        {
            HexTile vecina = grid.ObtenerCasilla(x + desplazamiento.x, y + desplazamiento.y);
            if (vecina != null) vecinas.Add(vecina);
        }

        return vecinas;
    }

    // El presupuesto se pasa explícitamente: no depende de puntosMovimientoRestantes
    public Dictionary<HexTile, float> CalcularCasillasAlcanzables(UnidadHex unidad, float presupuesto)
    {
        Dictionary<HexTile, float> alcanzables = new Dictionary<HexTile, float>();
        List<HexTile> pendientes = new List<HexTile>();

        HexTile inicio = grid.ObtenerCasilla(unidad.coordenadaX, unidad.coordenadaY);
        if (inicio == null) return alcanzables;
        if (presupuesto <= 0) return alcanzables;

        alcanzables[inicio] = 0f;
        pendientes.Add(inicio);

        while (pendientes.Count > 0)
        {
            HexTile actual = pendientes[0];
            foreach (HexTile c in pendientes)
                if (alcanzables[c] < alcanzables[actual]) actual = c;

            pendientes.Remove(actual);

            foreach (HexTile vecina in ObtenerVecinas(actual))
            {
                if (!vecina.esTransitable) continue;
                if (EstaOcupada(vecina, unidad)) continue;

                int coste = pesosTerreno.ObtenerCoste(unidad.datosTropa, vecina.tipoTerreno);
                if (coste <= 0) continue;

                float nuevoCoste = alcanzables[actual] + coste;
                if (nuevoCoste > presupuesto) continue;

                if (!alcanzables.ContainsKey(vecina) || nuevoCoste < alcanzables[vecina])
                {
                    alcanzables[vecina] = nuevoCoste;
                    if (!pendientes.Contains(vecina)) pendientes.Add(vecina);
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
            if (unidad == unidadActual) continue;
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

        if (inicio == null || destino == null) return new List<HexTile>();

        g[inicio] = 0f;
        abierta.Add(inicio);

        while (abierta.Count > 0)
        {
            HexTile actual = abierta[0];
            float mejorF = CalcularF(actual, destino, g);

            foreach (HexTile casilla in abierta)
            {
                float f = CalcularF(casilla, destino, g);
                if (f < mejorF) { mejorF = f; actual = casilla; }
            }

            if (actual == destino)
            {

                List<HexTile> resultado =
                    ReconstruirRuta(padre, inicio, destino);

                return resultado;
            }

            abierta.Remove(actual);
            cerrada.Add(actual);

            foreach (HexTile vecina in ObtenerVecinas(actual))
            {
                if (cerrada.Contains(vecina)) continue;
                if (!vecina.esTransitable) continue;
                if (EstaOcupada(vecina, unidad)) continue;

                int coste = pesosTerreno.ObtenerCoste(unidad.datosTropa, vecina.tipoTerreno);
                if (coste <= 0) continue;

                float gTentativo = g[actual] + coste;

                if (g.ContainsKey(vecina) && gTentativo >= g[vecina]) continue;

                padre[vecina] = actual;
                g[vecina] = gTentativo;

                if (!abierta.Contains(vecina)) abierta.Add(vecina);
            }
        }

        return new List<HexTile>(); 
    }

    private float CalcularF(HexTile casilla, HexTile destino, Dictionary<HexTile, float> g)
    {
        return g[casilla] + Heuristica(casilla, destino);
    }

    // public: EvaluadorTactico también la usa
    public float Heuristica(HexTile a, HexTile b)
    {
        int aq = a.coordenadaX;
        int ar = a.coordenadaY - (a.coordenadaX - (a.coordenadaX & 1)) / 2;

        int bq = b.coordenadaX;
        int br = b.coordenadaY - (b.coordenadaX - (b.coordenadaX & 1)) / 2;

        int as_ = -aq - ar;
        int bs = -bq - br;

        int distancia = (Mathf.Abs(aq - bq) + Mathf.Abs(ar - br) + Mathf.Abs(as_ - bs)) / 2;

        return distancia * 1f;
    }

    private List<HexTile> ReconstruirRuta(
    Dictionary<HexTile, HexTile> padre,
    HexTile inicio,
    HexTile destino)
    {
        List<HexTile> ruta = new List<HexTile>();
        HexTile actual = destino;

        while (actual != inicio)
        {
            ruta.Add(actual);

            if (!padre.ContainsKey(actual))
            {
                return new List<HexTile>();
            }

            actual = padre[actual];
        }

        // Añadimos el inicio, que no tiene padre.
        ruta.Add(inicio);

        ruta.Reverse();

        return ruta;
    }
}

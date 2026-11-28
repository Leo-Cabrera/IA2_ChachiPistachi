
using UnityEngine;
using System.Collections.Generic;

public class MapaInfluencia : MonoBehaviour
{
    [Header("Referencias")]
    public ControlUnidades controlUnidades;
    public MovimientoHex movimiento;

    [Header("Visualizacion")]
    public bool mostrarColores = true;
    public bool mostrarGizmos = true;
    public bool mostrarMapaTension = false;
    Mesh hexagonoGizmo;

    [Header("Parametros de influencia")]
    //decay = 0.1: la influencia se extiende mas lejos.
    // decay = 0.5: disminuye a una velocidad intermedia.
    // decay = 1.0: disminuye rapidamente.
    [Range(0.01f, 2f)]
    public float decay = 0.5f;

    // Influencia de nuestras tropas de cada casilla
    public Dictionary<HexTile, float> influenciaAliada = new Dictionary<HexTile, float>();
    // Influencia de tropas enemigas de cada casilla
    public Dictionary<HexTile, float> influenciaEnemiga = new Dictionary<HexTile, float>();
    // Influencia de total de cada casilla
    public Dictionary<HexTile, float> influenciaTotal = new Dictionary<HexTile, float>();

    public Dictionary<HexTile, float> mapaTension = new Dictionary<HexTile, float>();

    public void CalcularMapaInfluencia()
    {
        influenciaAliada.Clear();
        influenciaEnemiga.Clear();
        influenciaTotal.Clear();
        mapaTension.Clear();

        foreach (UnidadHex unidad in controlUnidades.tropasJugador)
            CalcularInfluenciaTropa(unidad, influenciaAliada);

        foreach (UnidadHex unidad in controlUnidades.tropasEnemigas)
            CalcularInfluenciaTropa(unidad, influenciaEnemiga);

        foreach (var elemento in influenciaAliada)
            influenciaTotal[elemento.Key] = elemento.Value;

        foreach (var elemento in influenciaEnemiga)
        {
            if (!influenciaTotal.ContainsKey(elemento.Key))
                influenciaTotal[elemento.Key] = 0;

            influenciaTotal[elemento.Key] -= elemento.Value;
        }

        foreach (var elemento in influenciaAliada)
            mapaTension[elemento.Key] = elemento.Value;

        foreach (var elemento in influenciaEnemiga)
        {
            if (!mapaTension.ContainsKey(elemento.Key))
                mapaTension[elemento.Key] = 0;

            mapaTension[elemento.Key] += elemento.Value;
        }

        Debug.Log("Mapa de control y mapa de tensión actualizados.");
        Debug.Log("Casillas con control: " + influenciaTotal.Count);
        Debug.Log("Casillas con tensión: " + mapaTension.Count);    
    }

    void CalcularInfluenciaTropa(UnidadHex unidad, Dictionary<HexTile, float> mapaDestino)
    {
        if (unidad == null || unidad.datosTropa == null) return;

        HexTile inicio = movimiento.grid.ObtenerCasilla(unidad.coordenadaX, unidad.coordenadaY);
        if (inicio == null) return;

        int presupuesto = movimiento.pesosTerreno.ObtenerPresupuesto(unidad.datosTropa);
        if (presupuesto <= 0) return;

        Dictionary<HexTile, float> alcanzables =
            movimiento.CalcularCasillasAlcanzables(unidad, presupuesto);

        foreach (var elemento in alcanzables)
        {
            HexTile casilla = elemento.Key;
            float coste = elemento.Value;

            float influencia = presupuesto * Mathf.Exp(-coste * decay);

            if (influencia <= 0.001f) continue;

            if (!mapaDestino.ContainsKey(casilla))
                mapaDestino[casilla] = 0;

            mapaDestino[casilla] += influencia;
        }
    }



    void OnDrawGizmos()
    {
        if (!mostrarGizmos || influenciaTotal == null || movimiento == null || movimiento.grid == null)
            return;

        var mapaActual = mostrarMapaTension ? mapaTension : influenciaTotal;

        foreach (var elemento in mapaActual)
        {
            HexTile casilla = elemento.Key;
            float influencia = elemento.Value;

            if (casilla == null) continue;

            if (mostrarColores)
            {
                if (hexagonoGizmo == null)
                    hexagonoGizmo = CrearHexagonoGizmo();

                float intensidad = Mathf.Clamp01(Mathf.Abs(influencia) / 3f);

                if (mostrarMapaTension)
                {
                    if (influencia <= 0) continue;

                    Gizmos.color = Color.Lerp(new Color(1f, 0.85f, 0.2f, 0.8f), new Color(1f, 0f, 0f, 0.95f), intensidad);
                }
                else
                {
                    if (influencia > 0)
                        Gizmos.color = Color.Lerp(new Color(0.4f, 0.65f, 1f, 0.8f), new Color(0f, 0f, 0.35f, 0.95f), intensidad);
                    else if (influencia < 0)
                        Gizmos.color = Color.Lerp(new Color(1f, 0.4f, 0.4f, 0.8f), new Color(0.5f, 0f, 0f, 0.95f), intensidad);
                    else
                        continue;
                }

                Vector3 posicion = casilla.transform.position;
                posicion.z -= 0.1f;

                Gizmos.DrawMesh(hexagonoGizmo, posicion, Quaternion.identity, Vector3.one * 0.5f);
            }

        }
    }


    Mesh CrearHexagonoGizmo()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[7];
        int[] triangulos = new int[18];

        vertices[0] = Vector3.zero;

        for (int i = 0; i < 6; i++)
        {
            float angulo = Mathf.Deg2Rad * (60 * i);
            vertices[i + 1] = new Vector3(Mathf.Cos(angulo), Mathf.Sin(angulo), 0);
            triangulos[i * 3] = 0;
            triangulos[i * 3 + 1] = (i + 1) % 6 + 1;
            triangulos[i * 3 + 2] = i + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangulos;
        mesh.RecalculateNormals();

        return mesh;
    }
}

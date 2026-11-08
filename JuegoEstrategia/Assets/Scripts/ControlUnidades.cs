
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ControlUnidades : MonoBehaviour
{
    [Header("Referencias")]
    public HexGrid grid;

    public UnidadHex tropaJugador;
    public UnidadHex tropaEnemiga;


    [Header("Indicadores de movimiento")]
    public GameObject prefabPuntoMovimiento;

    private List<GameObject> puntosMovimiento = new List<GameObject>();

    private Dictionary<HexTile, float> casillasAlcanzables;
    private UnidadHex unidadActual;

    private bool movimientoEnCurso = false;

    private bool unidadSeleccionada = false;
    private bool unidadesReiniciadas = false;

    [Header("Interfaz")]
    public TMPro.TextMeshProUGUI textoPuntosMovimiento;

    void Start()
    {
        StartCoroutine(EsperarMapa());
    }

    void Update()
    {

        if (movimientoEnCurso) return;
        if (!GameManager.instance.TurnoJugador())
        {
            //codigo para el turno del enemigo
            if (!unidadesReiniciadas) ReiniciarUnidades();
        }

        else
        {
            ActualizarTextoPuntos();
            //codigo para el turno del jugador
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 posicionMouse = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

                RaycastHit2D impacto = Physics2D.Raycast(posicionMouse, Vector2.zero);

                if (impacto.collider != null)
                {
                    // Comprobar si hemos pulsado una unidad
                    UnidadHex unidad = impacto.collider.GetComponent<UnidadHex>();

                    if (unidad != null && unidad.esJugador)
                    {
                        unidadSeleccionada = true;
                        MostrarCasillasMovimiento(unidad);
                        return;
                    }

                    // Comprobar si hemos pulsado una casilla
                    HexTile casilla = impacto.collider.GetComponent<HexTile>();

                    if (casilla != null && unidadSeleccionada)
                    {
                        if (casillasAlcanzables.ContainsKey(casilla))
                        {
                            movimientoEnCurso = true;
                            unidadActual.puntosMovimientoRestantes -= Mathf.CeilToInt(casillasAlcanzables[casilla]);
                            unidadesReiniciadas = false;
                            StartCoroutine(MoverUnidad(unidadActual, casilla));

                            unidadSeleccionada = false;
                            LimpiarPuntosMovimiento();
                        }
                    }
                }
            }
        }

        
    }

    IEnumerator EsperarMapa()
    {
        // Espera hasta que HexGrid termine de crear el mapa
        yield return new WaitUntil(() => grid != null && grid.mapaCreado);

        // Coloca las dos unidades
        ColocarUnidad(tropaJugador);
        ColocarUnidad(tropaEnemiga);

        ProbarMovimiento();
    }

    void ColocarUnidad(UnidadHex unidad)
    {
        if (unidad == null)
        {
            Debug.LogError("No se ha asignado una unidad.");
            return;
        }

        HexTile casilla = grid.ObtenerCasilla(unidad.coordenadaX,unidad.coordenadaY);

        if (casilla == null)
        {
            Debug.LogError("No se ha encontrado la casilla de la unidad.");
            return;
        }

        unidad.transform.position = casilla.transform.position;
    }

    void ProbarMovimiento()
    {
        MovimientoHex movimiento = GetComponent<MovimientoHex>();

        if (movimiento == null)
        {
            Debug.LogError("Falta el componente MovimientoHex.");
            return;
        }

        var casillas = movimiento.CalcularCasillasAlcanzables(tropaJugador);

        Debug.Log("Casillas alcanzables: " + (casillas.Count - 1));
    }

    void MostrarCasillasMovimiento(UnidadHex unidad)
    {
        // Elimina los puntos anteriores
        LimpiarPuntosMovimiento();

        MovimientoHex movimiento = GetComponent<MovimientoHex>();

        // Calcula las casillas alcanzables
        unidadActual = unidad;

        ActualizarTextoPuntos();

        casillasAlcanzables = movimiento.CalcularCasillasAlcanzables(unidad);

        var casillas = casillasAlcanzables;

        foreach (var elemento in casillas)
        {
            HexTile casilla = elemento.Key;

            // No muestra un punto sobre la propia unidad
            if (casilla.coordenadaX == unidad.coordenadaX && casilla.coordenadaY == unidad.coordenadaY)
            {
                continue;
            }

            // Crea el punto verde encima de la casilla
            GameObject punto = Instantiate( prefabPuntoMovimiento, casilla.transform.position, Quaternion.identity );

            puntosMovimiento.Add(punto);
        }
    }

    void LimpiarPuntosMovimiento()
    {
        foreach (GameObject punto in puntosMovimiento)
        {
            Destroy(punto);
        }

        puntosMovimiento.Clear();
    }

    IEnumerator MoverUnidad(UnidadHex unidad, HexTile destino)
    {
        Vector3 posicionInicial = unidad.transform.position;
        Vector3 posicionFinal = destino.transform.position;

        float duracion = 0.3f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / duracion;

            unidad.transform.position = Vector3.Lerp(posicionInicial, posicionFinal, progreso);

            yield return null;
        }

        unidad.transform.position = posicionFinal;

        unidad.coordenadaX = destino.coordenadaX;
        unidad.coordenadaY = destino.coordenadaY;

        ActualizarTextoPuntos();

        movimientoEnCurso = false;
    }

    void ActualizarTextoPuntos()
    {
        if (unidadActual != null)
        {
            textoPuntosMovimiento.text = "Puntos de movimiento: " + unidadActual.puntosMovimientoRestantes;
        }
    }

    private void ReiniciarUnidades()
    {
        tropaJugador.puntosMovimientoRestantes = tropaJugador.puntosMovimientoMaximos;
        unidadesReiniciadas = true;
    }
}

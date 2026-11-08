
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ControlUnidades : MonoBehaviour
{
    public static ControlUnidades instance;

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

    private void Awake()
    {
        instance = this;
    }


    void Start()
    {
        StartCoroutine(EsperarMapa());
    }

    void Update()
    {
        if (!GameManager.instance.TurnoJugador())
        {

            if (!unidadesReiniciadas) ReiniciarUnidades();
        }
        if (movimientoEnCurso) return;
        
        if (!esTurnoJugador) {
            
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
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

        List<HexTile> ruta = ElegirRutaHaciaJugador(tropaEnemiga, tropaJugador);

        ////////////////////////////////////////////////////////////////Debug.Log("Ruta hacia vecina del jugador: " + ruta.Count + " casillas");

        List<HexTile> recortada = RecortarPorPuntos(ruta, tropaEnemiga);

        ///////////////////////////////////////////////////////////////////////Debug.Log("Ruta recortada: " + recortada.Count + " casillas");
        //Debug.Log("Coste gastado: " + CalcularCosteRuta(recortada));

        foreach (HexTile casilla in ruta)
        {
            ///////////////////////////////////////////////////Debug.Log("(" + casilla.coordenadaX + ", " + casilla.coordenadaY + ")");
        }
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

    float CalcularCosteRuta(List<HexTile> ruta)
    {
        float coste = 0f;

        // Empieza en 1: la casilla 0 es donde ya está la unidad
        for (int i = 1; i < ruta.Count; i++)
        {
            coste += ruta[i].costeMovimiento;
        }

        return coste;
    }

    List<HexTile> ElegirRutaHaciaJugador(UnidadHex enemigo, UnidadHex jugador)
    {
        MovimientoHex movimiento = GetComponent<MovimientoHex>();

        HexTile casillaJugador = grid.ObtenerCasilla(jugador.coordenadaX, jugador.coordenadaY);
        HexTile casillaEnemigo = grid.ObtenerCasilla(enemigo.coordenadaX, enemigo.coordenadaY);

        if (casillaJugador == null || casillaEnemigo == null)
            return new List<HexTile>();

        List<HexTile> mejorRuta = new List<HexTile>();
        float mejorCoste = float.MaxValue;

        foreach (HexTile vecina in movimiento.ObtenerVecinas(casillaJugador))
        {
            // El enemigo no puede acabar sobre terreno intransitable
            if (!vecina.esTransitable) continue;

            // Ni sobre otra unidad (el jugador no está en su propia vecina, así que no se descarta a sí mismo)
            if (movimiento.EstaOcupada(vecina)) continue;

            List<HexTile> ruta = movimiento.CalcularRuta(casillaEnemigo, vecina, enemigo);

            // Sin ruta = inalcanzable, se descarta
            if (ruta.Count == 0) continue;

            float coste = CalcularCosteRuta(ruta);

            if (coste < mejorCoste)
            {
                mejorCoste = coste;
                mejorRuta = ruta;
            }
        }

        return mejorRuta;
    }

    List<HexTile> RecortarPorPuntos(List<HexTile> ruta, UnidadHex unidad)
    {
        List<HexTile> recortada = new List<HexTile>();

        if (ruta.Count == 0) return recortada;

        // La casilla 0 es donde ya está la unidad: siempre se incluye
        recortada.Add(ruta[0]);

        float acumulado = 0f;

        for (int i = 1; i < ruta.Count; i++)
        {
            acumulado += ruta[i].costeMovimiento;

            // Si este paso se pasa del presupuesto, se corta ANTES de añadirlo
            if (acumulado > unidad.puntosMovimientoRestantes) break;

            recortada.Add(ruta[i]);
        }

        return recortada;
    }

    private bool esTurnoJugador = true;
    private bool turnoEnemigoEnCurso = false;

    public void IniciarTurnoEnemigo()
    {
        if (turnoEnemigoEnCurso) return;

        esTurnoJugador = false;
        unidadSeleccionada = false;
        LimpiarPuntosMovimiento();

        StartCoroutine(TurnoEnemigo());
    }

    IEnumerator TurnoEnemigo()
    {
        turnoEnemigoEnCurso = true;

        List<HexTile> ruta = ElegirRutaHaciaJugador(tropaEnemiga, tropaJugador);
        ruta = RecortarPorPuntos(ruta, tropaEnemiga);

        if (ruta.Count <= 1)
        {
            Debug.Log("El enemigo no puede acercarse.");
        }
        else
        {
            float costeTotal = CalcularCosteRuta(ruta);

            for (int i = 1; i < ruta.Count; i++)
            {
                movimientoEnCurso = true;
                yield return StartCoroutine(MoverUnidad(tropaEnemiga, ruta[i]));
            }

            tropaEnemiga.puntosMovimientoRestantes -= Mathf.CeilToInt(costeTotal);
        }

        turnoEnemigoEnCurso = false;
        TerminarTurnoEnemigo();
    }

    void TerminarTurnoEnemigo()
    {
        esTurnoJugador = true;
        GameManager.instance.CambiarTurno();
    }


    private void ReiniciarUnidades()
    {
        tropaJugador.puntosMovimientoRestantes = tropaJugador.puntosMovimientoMaximos;
        unidadesReiniciadas = true;
        ActualizarTextoPuntos();
        
    }


}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ControlUnidades : MonoBehaviour
{
    public static ControlUnidades instance;

    public EvaluadorTactico evaluador;

    [Header("Referencias")]
    public HexGrid grid;

    public List<UnidadHex> tropasJugador = new List<UnidadHex>();
    public List<UnidadHex> tropasEnemigas = new List<UnidadHex>();

    [Header("Indicadores de movimiento")]
    public GameObject prefabPuntoMovimiento;

    private List<GameObject> puntosMovimiento = new List<GameObject>();

    private Dictionary<HexTile, float> casillasAlcanzables;
    private UnidadHex unidadActual;

    private bool movimientoEnCurso = false;
    private bool unidadSeleccionada = false;
    private bool unidadesAliadasReiniciadas = false;
    private bool unidadesEnemigasReiniciadas = false;

    [Header("Interfaz")]
    public TMPro.TextMeshProUGUI textoPuntosMovimiento;

    public GestorMonedas gestorMonedas;

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
            if (!unidadesAliadasReiniciadas) ReiniciarUnidadesAliadas();
        }
        else
        {
            if (!unidadesEnemigasReiniciadas) ReiniciarUnidadesEnemigas();
        }

        if (movimientoEnCurso) return;
        if (!esTurnoJugador) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ActualizarTextoPuntos();

            Vector2 posicionMouse = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            // Una unidad y la casilla pueden tener colliders superpuestos.
            // Revisa todos los impactos y prioriza seleccionar una unidad.
            Collider2D[] impactos = Physics2D.OverlapPointAll(posicionMouse);

            foreach (Collider2D impacto in impactos)
            {
                UnidadHex unidad = impacto.GetComponentInParent<UnidadHex>();

                if (unidad != null && unidad.esJugador && !unidad.yaSeHaMovido)
                {
                    unidadSeleccionada = true;
                    MostrarCasillasMovimiento(unidad);
                    return;
                }
            }

            // Si no se pulsó una tropa seleccionable, usa el impacto con una
            // casilla alcanzable como destino de movimiento.
            if (unidadSeleccionada && unidadActual != null)
            {
                foreach (Collider2D impacto in impactos)
                {
                    HexTile casilla = impacto.GetComponentInParent<HexTile>();

                    if (casilla != null && casillasAlcanzables != null &&
                        casillasAlcanzables.ContainsKey(casilla))
                    {
                        movimientoEnCurso = true;
                        unidadActual.yaSeHaMovido = true;
                        unidadActual.puntosMovimientoRestantes = 0;
                        unidadesAliadasReiniciadas = false;
                        StartCoroutine(MoverUnidad(unidadActual, casilla));

                        unidadSeleccionada = false;
                        LimpiarPuntosMovimiento();
                        return;
                    }
                }
            }
        }
    }

    IEnumerator EsperarMapa()
    {
        yield return new WaitUntil(() => grid != null && grid.mapaCreado);

        foreach (UnidadHex unidad in tropasJugador)
            ColocarUnidad(unidad);

        foreach (UnidadHex unidad in tropasEnemigas)
            ColocarUnidad(unidad);

        ReiniciarUnidadesAliadas();
        ReiniciarUnidadesEnemigas();

        GetComponent<MapaInfluencia>()?.CalcularMapaInfluencia();
    }

    void ColocarUnidad(UnidadHex unidad)
    {
        if (unidad == null)
        {
            Debug.LogError("No se ha asignado una unidad.");
            return;
        }

        HexTile casilla = grid.ObtenerCasilla(unidad.coordenadaX, unidad.coordenadaY);

        if (casilla == null)
        {
            Debug.LogError("No se ha encontrado la casilla de la unidad.");
            return;
        }

        unidad.transform.position = casilla.transform.position;
    }

    void MostrarCasillasMovimiento(UnidadHex unidad)
    {
        LimpiarPuntosMovimiento();

        MovimientoHex movimiento = GetComponent<MovimientoHex>();

        unidadActual = unidad;

        ActualizarTextoPuntos();

        casillasAlcanzables = movimiento.CalcularCasillasAlcanzables(unidad, unidad.puntosMovimientoRestantes);

        foreach (var elemento in casillasAlcanzables)
        {
            HexTile casilla = elemento.Key;

            if (casilla.coordenadaX == unidad.coordenadaX && casilla.coordenadaY == unidad.coordenadaY)
                continue;

            GameObject punto = Instantiate(prefabPuntoMovimiento, casilla.transform.position, Quaternion.identity);
            puntosMovimiento.Add(punto);
        }
    }

    void LimpiarPuntosMovimiento()
    {
        foreach (GameObject punto in puntosMovimiento)
            Destroy(punto);

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

        GetComponent<MapaInfluencia>()?.CalcularMapaInfluencia();

        if (gestorMonedas != null)
            gestorMonedas.RecogerMoneda(destino, unidad);

        ActualizarTextoPuntos();

        movimientoEnCurso = false;
    }

    void ActualizarTextoPuntos()
    {
        if (unidadActual != null && textoPuntosMovimiento != null)
            textoPuntosMovimiento.text = "Puntos de movimiento: " + unidadActual.puntosMovimientoRestantes;
    }

    float CalcularCosteRuta(List<HexTile> ruta, UnidadHex unidad)
    {
        float costeTotal = 0;
        MovimientoHex movimiento = GetComponent<MovimientoHex>();

        for (int i = 1; i < ruta.Count; i++)
            costeTotal += movimiento.pesosTerreno.ObtenerCoste(unidad.datosTropa, ruta[i].tipoTerreno);

        return costeTotal;
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

        // Reinicio explícito aquí: no dependemos del Update
        ReiniciarUnidadesEnemigas();

        if (tropasEnemigas.Count == 0 || tropasJugador.Count == 0)
        {
            turnoEnemigoEnCurso = false;
            TerminarTurnoEnemigo();
            yield break;
        }

        foreach (UnidadHex enemigo in tropasEnemigas)
        {
            if (enemigo == null) continue;

            if (enemigo.puntosMovimientoRestantes <= 0)
            {
                Debug.Log(enemigo.datosTropa.nombreTropa + " no tiene movimiento.");
                continue;
            }

            // El evaluador recibe TODOS los jugadores.
            // Él decidirá a cuál conviene acercarse.

            List<HexTile> ruta = evaluador.ElegirRuta(
                enemigo,
                tropasJugador
            );

            if (ruta.Count <= 1)
            {
                Debug.Log(enemigo.datosTropa.nombreTropa + " se queda quieto.");
                continue;
            }

            for (int i = 1; i < ruta.Count; i++)
            {
                movimientoEnCurso = true;
                yield return StartCoroutine(MoverUnidad(enemigo, ruta[i]));
            }

            enemigo.puntosMovimientoRestantes = 0;
            enemigo.yaSeHaMovido = true;
        }

        turnoEnemigoEnCurso = false;
        TerminarTurnoEnemigo();
    }

    // Elige el jugador más cercano que esté en una casilla válida
    UnidadHex ElegirObjetivo(UnidadHex enemigo)
    {
        HexTile casillaEnemigo = grid.ObtenerCasilla(enemigo.coordenadaX, enemigo.coordenadaY);
        if (casillaEnemigo == null) return null;

        UnidadHex mejor = null;
        int mejorDist = int.MaxValue;

        foreach (UnidadHex jugador in tropasJugador)
        {
            if (jugador == null) continue;
            if (grid.ObtenerCasilla(jugador.coordenadaX, jugador.coordenadaY) == null) continue;

            int d = evaluador.DistanciaHex(casillaEnemigo, jugador);

            if (d < mejorDist)
            {
                mejorDist = d;
                mejor = jugador;
            }
        }

        return mejor;
    }

    void TerminarTurnoEnemigo()
    {
        esTurnoJugador = true;
        GameManager.instance.CambiarTurno();
    }

    private void ReiniciarUnidadesAliadas()
    {
        MovimientoHex movimiento = GetComponent<MovimientoHex>();

        foreach (UnidadHex unidad in tropasJugador)
        {
            if (unidad == null) continue;

            int presupuesto = movimiento.pesosTerreno.ObtenerPresupuesto(unidad.datosTropa);

            unidad.puntosMovimientoMaximos = presupuesto;
            unidad.puntosMovimientoRestantes = presupuesto;
            unidad.yaSeHaMovido = false;
        }

        unidadesAliadasReiniciadas = true;
        ActualizarTextoPuntos();
    }

    private void ReiniciarUnidadesEnemigas()
    {
        MovimientoHex movimiento = GetComponent<MovimientoHex>();

        foreach (UnidadHex unidad in tropasEnemigas)
        {
            if (unidad == null) continue;

            int presupuesto = movimiento.pesosTerreno.ObtenerPresupuesto(unidad.datosTropa);

            unidad.puntosMovimientoMaximos = presupuesto;
            unidad.puntosMovimientoRestantes = presupuesto;
            unidad.yaSeHaMovido = false;
        }

        unidadesEnemigasReiniciadas = true;
    }
}

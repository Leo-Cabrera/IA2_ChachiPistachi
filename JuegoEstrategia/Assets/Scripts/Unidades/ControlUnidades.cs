
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

    [HideInInspector] public bool movimientoEnCurso = false;

    private bool unidadSeleccionada = false;
    private bool unidadesAliadasReiniciadas = false;
    private bool unidadesEnemigasReiniciadas = false;

    [Header("Interfaz")]
    public TMPro.TextMeshProUGUI textoPuntosMovimiento;

    public GestorMonedas gestorMonedas;

    private HashSet<GAgent> agentesPendientes = new HashSet<GAgent>();

    [HideInInspector] public bool esTurnoJugador = true;
    [HideInInspector] public bool turnoEnemigoEnCurso = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        StartCoroutine(EsperarMapa());
    }

    private void Update()
    {
        if (!GameManager.instance.TurnoJugador())
        {
            if (!unidadesAliadasReiniciadas)
                ReiniciarUnidadesAliadas();
        }
        else
        {
            if (!unidadesEnemigasReiniciadas)
                ReiniciarUnidadesEnemigas();
        }

        if (movimientoEnCurso)
            return;

        if (!esTurnoJugador)
            return;

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            ActualizarTextoPuntos();

            Vector2 posicionMouse = Camera.main.ScreenToWorldPoint(
                Mouse.current.position.ReadValue()
            );

            Collider2D[] impactos =
                Physics2D.OverlapPointAll(posicionMouse);

            // Primero comprobamos si se ha seleccionado una unidad.
            foreach (Collider2D impacto in impactos)
            {
                UnidadHex unidad =
                    impacto.GetComponentInParent<UnidadHex>();

                if (unidad != null &&
                    unidad.esJugador &&
                    !unidad.yaSeHaMovido)
                {
                    unidadSeleccionada = true;
                    MostrarCasillasMovimiento(unidad);
                    return;
                }
            }

            // Si no se ha seleccionado otra unidad,
            // comprobamos si se ha pulsado una casilla alcanzable.
            if (unidadSeleccionada && unidadActual != null)
            {
                foreach (Collider2D impacto in impactos)
                {
                    HexTile casilla =
                        impacto.GetComponentInParent<HexTile>();

                    if (casilla != null &&
                        casillasAlcanzables != null &&
                        casillasAlcanzables.ContainsKey(casilla))
                    {
                        movimientoEnCurso = true;

                        unidadActual.yaSeHaMovido = true;
                        unidadesAliadasReiniciadas = false;

                        unidadActual.GetComponent<GlowUnidad>()
                            ?.SetGlow(false);

                        StartCoroutine(
                            MoverUnidad(unidadActual, casilla)
                        );

                        unidadSeleccionada = false;
                        LimpiarPuntosMovimiento();

                        return;
                    }
                }
            }
        }
    }

    public void Evolucionar()
    {
        //solo podemos evolucionar una tropa propia seleccionada, que no se este moviendo durante nuestro turno, no durante el de la IA
        if (!GameManager.instance.TurnoJugador() || movimientoEnCurso ||
            !unidadSeleccionada || unidadActual == null || !unidadActual.esJugador)
        {
            return;
        }

        DatosTropa datos = unidadActual.datosTropa; //guardamos la referencia de los datos de la tropa

        //guardamos la referencia del componente CambiarSprites, la necesitaremos para poder cambiar el render
        CambiarSprites sprites = unidadActual.GetComponent<CambiarSprites>();

        //Si falta algún componente necesario o la unidad ya está en nivel 2, el método termina sin gastar monedas
        if (datos == null || sprites == null || gestorMonedas == null ||
            datos.nivel < 0 || datos.nivel >= 2)
        {
            return;
        }

        //vamos a comprobar que haya una siguiente forma, si el pokemon es de nivel 0 con su sprite inicial,
        //el siguiente que se vera sera sprite2, sino, no es nivel 0 pues el siguiente sera sprite3. 
        Sprite siguiente = datos.nivel == 0 ? sprites.sprite2 : sprites.sprite3;

        //para comprobar que siguiente apunta a un sprite asignado, si no hay sprite es null y no cobramos
        if (siguiente == null)
        {
            Debug.LogWarning("Falta el sprite de la siguiente evolución.");
            return;
        }

        //si no tenemos monedas suficientes para pagar la evolucion (5 monedas)
        if (!gestorMonedas.GastarMonedas(unidadActual.esJugador, 5))
        {
            Debug.Log("No hay monedas suficientes para evolucionar.");
            return;
        }

        datos.nivel++;  //sumamos de nivel a la tropa actual porque hemos evolucionado
        sprites.ActualizarSpriteSegunNivel();  //actualizamos el sprite segun el nivel al que hayamos evolucionado

        //la evolución consume la acción de esta tropa, asi que no gana monedas aunque este en el mismo lugar y tampoco se mueve
        unidadActual.yaSeHaMovido = true;
        unidadActual.puntosMovimientoRestantes = 0;

        unidadSeleccionada = false;  //ya no se podra seleccionar
        LimpiarPuntosMovimiento();  //quitamos los marcadores de las casillas a las que podiamos ir
        unidadActual.GetComponent<GlowUnidad>()?.SetGlow(false);  //apagamos el brillo de seleccion
        ActualizarColorUnidad(unidadActual);  //enseñamos la tropa en gris
        ActualizarTextoPuntos();  //actualizamos los puntos en los que podemos movernos que ahora seran 0

        //en el próximo turno, recuperará movimiento según su nuevo nivel
        unidadesAliadasReiniciadas = false;
    }

    private IEnumerator EsperarMapa()
    {
        yield return new WaitUntil(
            () => grid != null && grid.mapaCreado
        );

        foreach (UnidadHex unidad in tropasJugador)
            ColocarUnidad(unidad);

        foreach (UnidadHex unidad in tropasEnemigas)
            ColocarUnidad(unidad);

        ReiniciarUnidadesAliadas();
        ReiniciarUnidadesEnemigas();

        GetComponent<MapaInfluencia>()
            ?.CalcularMapaInfluencia();
    }

    private void ColocarUnidad(UnidadHex unidad)
    {
        if (unidad == null)
        {
            Debug.LogError("No se ha asignado una unidad.");
            return;
        }

        HexTile casilla = grid.ObtenerCasilla(
            unidad.coordenadaX,
            unidad.coordenadaY
        );

        if (casilla == null)
        {
            Debug.LogError(
                "No se ha encontrado la casilla de la unidad."
            );
            return;
        }

        unidad.transform.position = casilla.transform.position;
    }

    private void MostrarCasillasMovimiento(UnidadHex unidad)
    {
        LimpiarPuntosMovimiento();

        MovimientoHex movimiento = GetComponent<MovimientoHex>();

        if (unidadActual != null)
        {
            unidadActual.GetComponent<GlowUnidad>()
                ?.SetGlow(false);
        }

        unidadActual = unidad;

        unidadActual.GetComponent<GlowUnidad>()
            ?.SetGlow(true);

        ActualizarTextoPuntos();

        casillasAlcanzables =
            movimiento.CalcularCasillasAlcanzables(
                unidad,
                unidad.puntosMovimientoRestantes
            );

        foreach (var elemento in casillasAlcanzables)
        {
            HexTile casilla = elemento.Key;

            if (casilla.coordenadaX == unidad.coordenadaX &&
                casilla.coordenadaY == unidad.coordenadaY)
                continue;

            GameObject punto = Instantiate(
                prefabPuntoMovimiento,
                casilla.transform.position,
                Quaternion.identity
            );

            puntosMovimiento.Add(punto);
        }
    }

    private void LimpiarPuntosMovimiento()
    {
        foreach (GameObject punto in puntosMovimiento)
            Destroy(punto);

        puntosMovimiento.Clear();
    }

    // NUEVO: calcula el coste entero de entrar en una casilla.
    // Como UnidadHex utiliza puntos de movimiento enteros,
    // redondeamos hacia arriba los costes decimales.
    public int ObtenerCosteMovimiento(
        UnidadHex unidad,
        HexTile destino)
    {
        MovimientoHex movimiento = GetComponent<MovimientoHex>();

        if (movimiento == null ||
            movimiento.pesosTerreno == null ||
            unidad == null ||
            destino == null)
            return int.MaxValue;

        float coste = movimiento.pesosTerreno.ObtenerCoste(
            unidad.datosTropa,
            destino.tipoTerreno
        );

        if (coste <= 0)
            return int.MaxValue;

        return Mathf.Max(1, Mathf.CeilToInt(coste));
    }

    public IEnumerator MoverUnidad(
        UnidadHex unidad,
        HexTile destino)
    {
        if (unidad == null || destino == null)
        {
            movimientoEnCurso = false;
            yield break;
        }

        // NUEVO: calculamos el coste antes de mover.
        int costeMovimiento = ObtenerCosteMovimiento(
            unidad,
            destino
        );

        if (costeMovimiento == int.MaxValue ||
            costeMovimiento > unidad.puntosMovimientoRestantes)
        {
            movimientoEnCurso = false;
            yield break;
        }

        Vector3 posicionInicial = unidad.transform.position;
        Vector3 posicionFinal = destino.transform.position;

        float duracion = 0.3f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float progreso = tiempo / duracion;

            unidad.transform.position = Vector3.Lerp(
                posicionInicial,
                posicionFinal,
                progreso
            );

            yield return null;
        }

        unidad.transform.position = posicionFinal;

        //comparamos la casilla de origen con la de destino
        if (unidad.coordenadaX != destino.coordenadaX ||
            unidad.coordenadaY != destino.coordenadaY)
        {
            unidad.seHaDesplazadoEsteTurno = true;
        }

        unidad.coordenadaX = destino.coordenadaX;
        unidad.coordenadaY = destino.coordenadaY;

        // NUEVO: descontamos el coste real del terreno.
        unidad.puntosMovimientoRestantes -= costeMovimiento;

        unidad.puntosMovimientoRestantes = Mathf.Max(
            0,
            unidad.puntosMovimientoRestantes
        );

        if (unidad.puntosMovimientoRestantes <= 0)
            ActualizarColorUnidad(unidad);

        GetComponent<MapaInfluencia>()
            ?.CalcularMapaInfluencia();

        // Se conserva la recogida de monedas existente.
        if (gestorMonedas != null)
            gestorMonedas.RecogerMoneda(destino, unidad);

        ActualizarTextoPuntos();

        movimientoEnCurso = false;
    }

    private void ActualizarTextoPuntos()
    {
        if (unidadActual != null &&
            textoPuntosMovimiento != null)
        {
            textoPuntosMovimiento.text =
                "Puntos de movimiento: " +
                unidadActual.puntosMovimientoRestantes;
        }
    }

    public void IniciarTurnoEnemigo()
    {
        if (turnoEnemigoEnCurso)
            return;

        unidadSeleccionada = false;

        if (unidadActual != null)
        {
            unidadActual.GetComponent<GlowUnidad>()
                ?.SetGlow(false);
        }

        LimpiarPuntosMovimiento();

        turnoEnemigoEnCurso = true;

        ReiniciarUnidadesEnemigas();

        if (tropasEnemigas.Count == 0 ||
            tropasJugador.Count == 0)
        {
            TerminarTurnoEnemigo();
            return;
        }

        esTurnoJugador = false;

        // NUEVO: limpiamos posibles agentes pendientes de un turno anterior.
        agentesPendientes.Clear();

        foreach (UnidadHex enemigo in tropasEnemigas)
        {
            if (enemigo == null)
                continue;

            GAgent agente = enemigo.GetComponent<GAgent>();

            if (agente == null)
            {
                Debug.LogWarning(
                    enemigo.name + " no tiene un GAgent."
                );

                continue;
            }

            agentesPendientes.Add(agente);
            agente.OnTurnFinished += OnAgenteTurnFinished;
        }

        if (agentesPendientes.Count == 0)
        {
            TerminarTurnoEnemigo();
            return;
        }

        // Recorremos una copia para evitar errores si un agente
        // termina su turno y se elimina del HashSet original.
        foreach (GAgent agente in new List<GAgent>(agentesPendientes))
        {
            agente.StartTurn();
        }
    }

    private void OnAgenteTurnFinished(GAgent agente)
    {
        if (agente == null)
            return;

        agente.OnTurnFinished -= OnAgenteTurnFinished;

        agentesPendientes.Remove(agente);

        if (agentesPendientes.Count == 0)
            TerminarTurnoEnemigo();
    }

    private void TerminarTurnoEnemigo()
    {
        if (!turnoEnemigoEnCurso)
            return;

        turnoEnemigoEnCurso = false;
        esTurnoJugador = true;

        GameManager.instance.CambiarTurno();
    }

    private void ReiniciarUnidadesAliadas()
    {
        MovimientoHex movimiento = GetComponent<MovimientoHex>();

        foreach (UnidadHex unidad in tropasJugador)
        {
            if (unidad == null)
                continue;

            int presupuesto =
                movimiento.pesosTerreno.ObtenerPresupuesto(
                    unidad.datosTropa
                );

            unidad.puntosMovimientoMaximos = presupuesto;
            unidad.puntosMovimientoRestantes = presupuesto;
            unidad.yaSeHaMovido = false;
            unidad.seHaDesplazadoEsteTurno = false;

            ActualizarColorUnidad(unidad);
        }

        unidadesAliadasReiniciadas = true;

        ActualizarTextoPuntos();
    }

    private void ReiniciarUnidadesEnemigas()
    {
        MovimientoHex movimiento = GetComponent<MovimientoHex>();

        foreach (UnidadHex unidad in tropasEnemigas)
        {
            if (unidad == null)
                continue;

            int presupuesto =
                movimiento.pesosTerreno.ObtenerPresupuesto(
                    unidad.datosTropa
                );

            unidad.puntosMovimientoMaximos = presupuesto;
            unidad.puntosMovimientoRestantes = presupuesto;
            unidad.yaSeHaMovido = false;
            unidad.seHaDesplazadoEsteTurno = false;

            ActualizarColorUnidad(unidad);
        }

        unidadesEnemigasReiniciadas = true;
    }

    private void ActualizarColorUnidad(UnidadHex unidad)
    {
        SpriteRenderer sr =
            unidad.GetComponentInChildren<SpriteRenderer>();

        if (sr == null)
            return;

        sr.color = unidad.yaSeHaMovido
            ? Color.gray
            : Color.white;
    }
}

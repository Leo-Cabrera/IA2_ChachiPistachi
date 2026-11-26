
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private bool turnoJugador = true;

    public GameObject btnSigTurno;

    [SerializeField] private GameObject btnEvolucionar;

    public GameObject textoPensando;
    public GameObject textoMovRestantes;

    [Header("Contadores de monedas")]
    [SerializeField] private TextMeshProUGUI textoMonedas;
    [SerializeField] private TextMeshProUGUI textoMonedasJugador;
    [SerializeField] private TextMeshProUGUI textoMonedasIA;

    public Temporizador temporizador;

    [HideInInspector] public bool empezarAccionEnemiga = false;

    private GestorMonedas gestorMonedas;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        BuscarGestorMonedas();
        ActualizarInterfaz();
        ActualizarContadoresMonedas();
    }

    private void OnEnable()
    {
        if (gestorMonedas != null)
            gestorMonedas.OnMonedasActualizadas += ActualizarContadoresMonedas;
    }

    private void OnDisable()
    {
        if (gestorMonedas != null)
            gestorMonedas.OnMonedasActualizadas -= ActualizarContadoresMonedas;
    }

    private void BuscarGestorMonedas()
    {
        if (ControlUnidades.instance == null)
            return;

        gestorMonedas = ControlUnidades.instance.gestorMonedas;

        if (gestorMonedas != null)
        {
            gestorMonedas.OnMonedasActualizadas -= ActualizarContadoresMonedas;
            gestorMonedas.OnMonedasActualizadas += ActualizarContadoresMonedas;
        }
    }

    private void Update()
    {
        if (empezarAccionEnemiga)
        {
            empezarAccionEnemiga = false;

            if (ControlUnidades.instance != null)
                ControlUnidades.instance.IniciarTurnoEnemigo();
        }
    }

    private void LateUpdate()
    {
        if (gestorMonedas == null)
            BuscarGestorMonedas();

        ActualizarContadoresMonedas();
    }

    private void ActualizarContadoresMonedas()
    {
        if (gestorMonedas == null)
            return;

        int saldoJugador = gestorMonedas.ObtenerMonedas(true);
        int saldoIA = gestorMonedas.ObtenerMonedas(false);

        // Si hay dos textos asignados, mostramos ambos saldos.
        if (textoMonedasJugador != null)
            textoMonedasJugador.text = "Tus monedas: " + saldoJugador;

        if (textoMonedasIA != null)
            textoMonedasIA.text = "Monedas IA: " + saldoIA;

        // Compatibilidad con la interfaz antigua de un solo texto.
        if (textoMonedas != null)
        {
            if (textoMonedasJugador == null &&
                textoMonedasIA == null)
            {
                int saldoActual = turnoJugador
                    ? saldoJugador
                    : saldoIA;

                textoMonedas.text = turnoJugador
                    ? "Tus monedas: " + saldoActual
                    : "Monedas IA: " + saldoActual;
            }
        }
    }

    
    public void CambiarTurno()
    {
        ControlUnidades control = ControlUnidades.instance;

        if (turnoJugador &&
            control != null &&
            control.movimientoEnCurso)
        {
            return;
        }

        // El jugador termina su turno:
        // recompensamos únicamente a las tropas que no han realizado
        // ninguna otra acción durante este turno.
        if (turnoJugador &&
            control != null &&
            gestorMonedas != null)
        {
            gestorMonedas.RecompensarTropasQuietas(
                control.tropasJugador,
                true
            );

            control.PrepararSiguienteTurnoJugador();
        }

        // La IA acaba de terminar su turno.
        // Mantenemos exactamente su sistema de recompensa actual.
        if (!turnoJugador &&
            control != null &&
            gestorMonedas != null)
        {
            gestorMonedas.RecompensarTropasQuietas(
                control.tropasEnemigas,
                false
            );
        }

        turnoJugador = !turnoJugador;

        ActualizarInterfaz();
        ActualizarContadoresMonedas();

        if (!turnoJugador && temporizador != null)
            temporizador.IniciarTemporizador();

        if (btnEvolucionar != null)
            btnEvolucionar.SetActive(turnoJugador);
    }


    private void ActualizarInterfaz()
    {
        if (btnSigTurno != null)
            btnSigTurno.SetActive(turnoJugador);

        if (textoPensando != null)
            textoPensando.SetActive(!turnoJugador);

        if (textoMovRestantes != null)
            textoMovRestantes.SetActive(turnoJugador);
    }

    public bool TurnoJugador()
    {
        return turnoJugador;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private bool turnoJugador = true;
    public GameObject btnSigTurno;
    public GameObject textoPensando;
    public GameObject textoMovRestantes;
    public Temporizador temporizador;
    [HideInInspector] public bool empezarAccionEnemiga = false;


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
        ActualizarInterfaz();
    }

    private void Update()
    {
        if (empezarAccionEnemiga)
        {
            empezarAccionEnemiga = false;

            ControlUnidades.instance.IniciarTurnoEnemigo();
        }
    }

    public void CambiarTurno()
    {
        turnoJugador = !turnoJugador;

        ActualizarInterfaz();

        if (!turnoJugador)
        {
            temporizador.IniciarTemporizador();
        }
    }

    private void ActualizarInterfaz()
    {
        btnSigTurno.SetActive(turnoJugador);
        textoPensando.SetActive(!turnoJugador);
        textoMovRestantes.SetActive(turnoJugador);
    }

    public bool TurnoJugador()
    {
        return turnoJugador;
    }
}

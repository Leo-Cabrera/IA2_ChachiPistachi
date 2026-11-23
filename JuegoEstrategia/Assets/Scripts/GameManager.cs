using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private bool turnoJugador = true;
    public GameObject btnSigTurno;
    [SerializeField] private GameObject btnEvolucionar;
    public GameObject textoPensando;
    public GameObject textoMovRestantes;
    [SerializeField] private TextMeshProUGUI textoMonedas;
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

    private void LateUpdate() //para que se actualice en cada fotograma 
    {
        //guardamos en control el control de unidades de la partida actual 
        ControlUnidades control = ControlUnidades.instance;

        //si falta alguna referencia entonces salimos del metodo en ese fotograma para que no de problema
        if (textoMonedas == null || control == null || control.gestorMonedas == null)
            return;

        //consultamos el saldo de monedas del equipo que está jugando ahora. si es el jugador dara true al metodo obtenermonedas, sino false
        int saldo = control.gestorMonedas.ObtenerMonedas(turnoJugador);  //si es true en el metodo enseña jugador, si es falso la IA

        if (turnoJugador)  //si estamos en el turno del jugador
            textoMonedas.text = $"Tus monedas · {saldo}";  //enseñamos este texto
        else  //sino, es el de la IA
            textoMonedas.text = $"Monedas IA · {saldo}";  //enseñamos el texto con las monedas de la IA
    }

    public void CambiarTurno()
    {
        //gaurdamos la referencia de ControlUnidades donde estan las listas de las tropas y sus movimientos
        ControlUnidades control = ControlUnidades.instance;

        //si es el turno del jugador, control apunta al componente  bien y se puede seguir con un turno, entonces salimos y nos esperamos
        if (turnoJugador && control != null && control.movimientoEnCurso)
            return;

        //mientras esos componente de control no estén sin referenciar, vacios
        if (control != null && control.gestorMonedas != null)
        {
            if (turnoJugador)  //si es el turno del jugador llamamos a recompensar para que se haga el metodo
                control.gestorMonedas.RecompensarTropasQuietas(control.tropasJugador, true);
            else  //lo mismo cuando sea el turno de la IA, el false es porque no es el jugador (parametro del metodo)
                control.gestorMonedas.RecompensarTropasQuietas(control.tropasEnemigas, false);
        }

        turnoJugador = !turnoJugador;

        ActualizarInterfaz();

        if (!turnoJugador)
        {
            temporizador.IniciarTemporizador();
        }

        if (btnEvolucionar != null)  //mientras el boton este asignado
        {
            btnEvolucionar.SetActive(turnoJugador);  //que solo aparezca en el turno del jugador 
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

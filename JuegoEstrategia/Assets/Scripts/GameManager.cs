using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private bool turnoJugador = true;
    public GameObject btnSigTurno;
    public GameObject textoPensando;
    public GameObject textoPuntosMovimiento;
    public Temporizador temporizador;
    

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            if (ControlUnidades.instance == null)
            {
                Debug.LogError("No hay ControlUnidades en la escena.");
            }

        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (turnoJugador)
        {
            btnSigTurno.SetActive(true);
            textoPensando.SetActive(false);
            textoPuntosMovimiento.SetActive(true);

        }
        else
        {
            btnSigTurno.SetActive(false);
            textoPensando.SetActive(true);
            textoPuntosMovimiento.SetActive(false);
        }
    }
    public void CambiarTurno()
    {
        turnoJugador = !turnoJugador;
        if (!turnoJugador)
        {
            temporizador.IniciarTemporizador();
            ControlUnidades.instance.IniciarTurnoEnemigo();
        }
    }
    public bool TurnoJugador()
    {
        return turnoJugador;
    }
}

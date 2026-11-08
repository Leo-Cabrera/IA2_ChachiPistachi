using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private bool turnoJugador = true;
    public GameObject btnSigTurno;
    public GameObject textoPensando;
    public Temporizador temporizador;
    

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
        }
        else
        {
            btnSigTurno.SetActive(false);
            textoPensando.SetActive(true);
        }
    }
    public void CambiarTurno()
    {
        turnoJugador = !turnoJugador;
        if (!turnoJugador)
        {
            temporizador.IniciarTemporizador();
        }
    }
}

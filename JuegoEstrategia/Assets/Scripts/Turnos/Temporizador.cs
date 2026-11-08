using UnityEngine;



public class Temporizador : MonoBehaviour
{
    public float maxTemporizador = 5f;
    private float temporizador;
    private float tiempoEspera;
    [HideInInspector] public bool temporizadorActivo;
    private bool pensando;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        temporizadorActivo = false;
        pensando = false;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (temporizadorActivo)
        {
            temporizador -= Time.deltaTime;
            
            if(pensando)
            {
                tiempoEspera -= Time.deltaTime;
                if(tiempoEspera <= 0)
                {
                    tiempoEspera = 0;
                    pensando = false;
                    GameManager.instance.empezarAccionEnemiga = true;
                    
                }
            }
            
            if (temporizador <= 0)
            {
                ReiniciarTemporizador();

            }
        }
        

    }
    private void ReiniciarTemporizador()
    {
        temporizadorActivo = false;
        pensando = false;
        
    }
    public void IniciarTemporizador()
    {
        temporizador = maxTemporizador;
        tiempoEspera = maxTemporizador / 2;
        temporizadorActivo = true;
        pensando = true;
    }
}

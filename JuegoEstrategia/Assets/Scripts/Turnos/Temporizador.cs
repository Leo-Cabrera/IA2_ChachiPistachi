using UnityEngine;



public class Temporizador : MonoBehaviour
{
    public float maxTemporizador = 5f;
    private float temporizador;
    [HideInInspector] public bool temporizadorActivo;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        temporizador = maxTemporizador;
        temporizadorActivo = false;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (temporizadorActivo)
        {
            temporizador -= Time.deltaTime;
            if (temporizador <= 0)
            {
                ReiniciarTemporizador();

            }
        }
        

    }
    private void ReiniciarTemporizador()
    {
        temporizador = maxTemporizador;
        temporizadorActivo = false;
        GameManager.instance.CambiarTurno();
    }
    public void IniciarTemporizador()
    {
        temporizadorActivo = true;
    }
}

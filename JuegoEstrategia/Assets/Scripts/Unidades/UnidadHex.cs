
using UnityEngine;

public class UnidadHex : MonoBehaviour
{
    [Header("Equipo")]
    public bool esJugador;

    [Header("Posicion en el mapa")]
    public int coordenadaX;
    public int coordenadaY;

    [Header("Movimiento")]
    public int puntosMovimientoRestantes = 0;
    public int puntosMovimientoMaximos = 0;
    public bool yaSeHaMovido = false;

    public bool seHaDesplazadoEsteTurno = false;
    public bool haEvolucionado = false;

    [Header("Registro de acciones GOAP")]
    public bool haElegidoStayStillEsteTurno = false;
    public bool haEjecutadoOtraAccionEsteTurno = false;
    public bool haRecibidoRecompensaStayStillEsteTurno = false;

    [Header("Datos de la tropa")]
    public DatosTropa datosTropa;

    public void ReiniciarRegistroAccionesGOAP()
    {
        haElegidoStayStillEsteTurno = false;
        haEjecutadoOtraAccionEsteTurno = false;
        haRecibidoRecompensaStayStillEsteTurno = false;
    }

    public void RegistrarAccionGOAP(bool esStayStill)
    {
        if (esStayStill)
        {
            if (!haEjecutadoOtraAccionEsteTurno)
                haElegidoStayStillEsteTurno = true;
        }
        else
        {
            haEjecutadoOtraAccionEsteTurno = true;
            haElegidoStayStillEsteTurno = false;
        }
    }

    public bool PuedeRecibirRecompensaStayStill()
    {
        return haElegidoStayStillEsteTurno &&
               !haEjecutadoOtraAccionEsteTurno &&
               !haRecibidoRecompensaStayStillEsteTurno;
    }

    public bool PuedeRecibirRecompensaPorQuedarseQuietoJugador()
    {
        return !yaSeHaMovido &&
               !seHaDesplazadoEsteTurno &&
               !haEjecutadoOtraAccionEsteTurno &&
               !haRecibidoRecompensaStayStillEsteTurno;
    }
}

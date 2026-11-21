
using UnityEngine;

public class UnidadHex : MonoBehaviour
{
    [Header("Equipo")]
    //Indica si la unidad pertenece al jugador o al enemigo
    public bool esJugador;

    [Header("Posicion en el mapa")]
    //	Columna de la casilla en la que esta la unidad
    public int coordenadaX;
    // Fila de la casilla en la que esta la unidad
    public int coordenadaY;

    [Header("Movimiento")]
    //Cantidad de movimiento disponible para la unidad durante su turno
    public int puntosMovimientoRestantes = 0;
    public int puntosMovimientoMaximos = 0;
    public bool yaSeHaMovido = false;

    //indica si la unidad ha cambiado de casilla durante el turno, lo necesitamos para ver si damos monedas
    public bool seHaDesplazadoEsteTurno = false;

    [Header("Datos de la tropa")]
    public DatosTropa datosTropa;
}

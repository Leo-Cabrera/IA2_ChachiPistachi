using UnityEngine;

public class GestorMonedas: MonoBehaviour
{
    public HexGrid grid;  //la usaremos para guardar la referencia de nuestro mapa
    public MovimientoHex movimiento;  //para saber si sera una casilla a la que pueda moverse el personaje 
    public GameObject moneda;  //guardarmeos el modelo de la moneda

    public int columna = 13;
    public int fila = 7;

    public void ColocarMoneda()
    {
        //para comprobar que hemos asignaddo cosas en el inspector antes de continuar
        if (grid == null || movimiento == null || moneda == null)
        {
            Debug.LogError("Faltan referencias en GestorMonedas.");
            return;
        }

        //guardamos un objeto tile en casilla, nos guardamos la casilla en columna y fila que elijamos
        HexTile casilla = grid.ObtenerCasilla(columna, fila);  

        if (casilla == null)  //si las coordenadas no existen aunque es poco probable que pase
        {
            Debug.LogWarning("Las coordenadas no pertenecen al mapa.");
            return;
        }

        if (!casilla.esTransitable || movimiento.EstaOcupada(casilla))  //si no se puede pasar o ya hay alguien ahí 
        {
            Debug.LogWarning("La casilla no esta disponible para una moneda.");
            return;  //salimos porque no se puede poner la moneda en una casilla que no es accesible
        }

        moneda.transform.position = casilla.transform.position; //si no entra a ningún if, ponemos el objeto moneda en la casilla
    }
}

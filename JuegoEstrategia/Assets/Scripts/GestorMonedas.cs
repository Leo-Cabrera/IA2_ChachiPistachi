using UnityEngine;

public class GestorMonedas: MonoBehaviour
{
    public HexGrid grid;  //la usaremos para guardar la referencia de nuestro mapa
    public MovimientoHex movimiento;  //para saber si sera una casilla a la que pueda moverse el personaje 
    public GameObject moneda;  //guardarmeos el modelo de la moneda

    public int columna = 13;
    public int fila = 7;

    //contadores de las monedas de cada uno
    [SerializeField] private int monedasJugador = 0;
    [SerializeField] private int monedasIA = 0;

    private HexTile casillaMoneda; //para guardar casilla donde se coloca la moneda

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

        casillaMoneda = casilla; //donde este la moneda asignamos esa casilla a casillamoneda
    }

    //recibe el dato de la casilla en la que se esta para ver si es casillamoneda y también el tipo de unidad (si jugador o IA)
    public void RecogerMoneda(HexTile casillaDestino, UnidadHex unidad)  
    {
        
        if (casillaMoneda == null) //si no hay una moneda en esa casilla pues no se hace nada
        {
            return;
        }

        if (casillaDestino == casillaMoneda)  //si hay una moneda en esa casilla
        {
            moneda.SetActive(false);  //desactivamos el gameobject de moneda
            casillaMoneda = null;  //actualizamos casillamoneda porque ya no habrá una moneda en esa casilla

            //la moneda del mapa da 3 monedas 
            if (unidad.esJugador == true)  //si quien ha cogido la moneda es una unidad del jugador, ya usamos el esjugador en unidadhex
            {
                monedasJugador += 3;
            }
            else  //sino habra sido la de la IA
            {
                monedasIA += 3;
            }

            Debug.Log($"Moneda recogida. Jugador: {monedasJugador}, IA: {monedasIA}");  //para comprobar, ya lo haremos en canva
        }
    }
}

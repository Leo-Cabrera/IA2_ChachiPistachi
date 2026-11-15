using UnityEngine;
using System.Collections.Generic;

public class GestorMonedas: MonoBehaviour
{
    public HexGrid grid;  //la usaremos para guardar la referencia de nuestro mapa
    public MovimientoHex movimiento;  //para saber si sera una casilla a la que pueda moverse el personaje 
    public GameObject moneda;  //guardarmeos el modelo de la moneda


    //contadores de las monedas de cada uno
    [SerializeField] private int monedasJugador = 0;
    [SerializeField] private int monedasIA = 0;

    //los vamos a usar para delimitar la zona donde pueden aparecer las monedas
    [SerializeField] private int columnaMinima = 8;
    [SerializeField] private int columnaMaxima = 19;
    [SerializeField] private int filaMinima = 9;
    [SerializeField] private int filaMaxima = 11;

    private HexTile casillaMoneda; //para guardar casilla donde se coloca la moneda

    private GameObject monedaCreada; //guardar la referencia de la moneda que se cree

    public void ColocarMoneda()
    {
        //para comprobar que hemos asignaddo cosas en el inspector antes de continuar
        if (grid == null || movimiento == null || moneda == null)
        {
            Debug.LogError("Faltan referencias en GestorMonedas.");
            return;
        }


        List<HexTile> disponibles = new List<HexTile>();  //lista para las tiles disponibles donde podran apaarecer moendas

        for (int x = columnaMinima; x <= columnaMaxima; x++)  //que aparezcen a partir de los limites que pongamos
        {
            for (int y = filaMinima; y <= filaMaxima; y++)
            {
                HexTile candidata = grid.ObtenerCasilla(x, y);  //en la casilla en la que estamos gaurdamos su referencia

                //que la casilla exista, que sea transitable y que esté libre (el ultimo caso devuelve true si no esta ocupada)
                if (candidata != null && candidata.esTransitable && !movimiento.EstaOcupada(candidata))
                {
                    disponibles.Add(candidata);  //la añadimos a la lista
                }
            }
        }

        Debug.Log($"Casillas disponibles para monedas: {disponibles.Count}");

        //si la lista esta vacia no hay ninguna casilla disponible, no deberia de pasar tho
        if (disponibles.Count == 0)
        {
            Debug.LogWarning("No hay casillas disponibles para colocar la moneda.");
            return;
        }

        //aleatoriamente seleccionamos una casilla disponible de la lista
        int indice = Random.Range(0, disponibles.Count);
        HexTile casilla = disponibles[indice];  //guardamos en casilla la casilla de la lista disponible seleccionada

        //crea una copia del prefab moneda y guarda la referencia de esa copia en monedacreada
        monedaCreada = Instantiate(moneda);

        //mueve la copia a la posición de la casilla seleccionada con el range
        monedaCreada.transform.position = casilla.transform.position;

        casillaMoneda = casilla; //donde este la moneda asignamos esa casilla a casillamoneda, para guardar su referencia
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
            Destroy(monedaCreada);  //eliminamos el gameobject de la moneda creada en base al prefab
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

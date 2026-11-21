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
    [SerializeField] private int monedasPorTanda = 5;  //para poder editar el valor desde el inspector de cuantas monedas quiero que aparezcan

    private GameObject monedaCreada; //guardar la referencia de la moneda que se cree

    private int monedasEnMapa = 0; //cuenta las monedas que siguen en el tablero

    //devuelve el saldo del equipo que tenga el turno 
    public int ObtenerMonedas(bool esJugador)
    {
        if (esJugador)
            return monedasJugador;

        return monedasIA;
    } 

    public void RecompensarTropasQuietas(List<UnidadHex> tropas, bool esJugador)
    {
        int recompensa = 0;  //empieza en 0 cada vez que se llama al metodo asi terminaremos con una cantidad y borraremos al siguiente turno

        foreach (UnidadHex unidad in tropas)  //por cada unidad que tenemos en la lista de tropas
        {
            //si la unidad esta vacia (hemos puesto en la lista que hay mas de las que hay en el mapa, para que las pruebes no den error)
            //o la unidad se ha desplazado == true
            if (unidad == null || unidad.seHaDesplazadoEsteTurno) 
                continue; //entonces continuamos con la siguiente tropa del foreach porque la actual no dara recompensa

            int nivel = 0;  //en caso de no tener conocimiento del nivel, que su base sea el 0
            if (unidad.datosTropa != null)  //mientras tengamos la tropa tenga datos
                nivel = unidad.datosTropa.nivel;  //cambiamos la variable nivel, por el nivel de la tropa (las evoluciones)

            //lo de mathclamp es para que use el nivel si esta entre 0 y 2, asi evitamos problemas. Si es nivel 0 entonces a la recompensa
            //le sumaremos nivel +1, asi que 0 +1, dandonos asi una moneda si eres de nivel 0
            recompensa += Mathf.Clamp(nivel, 0, 2) + 1;
        }

        if (esJugador) //si de quien hemos hecho las comprobaciones es el jugador
            monedasJugador += recompensa;  //sumamos al contador de las monedas del jugador
        else
            monedasIA += recompensa;  //lo mismo si es la IA
    }

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
                if (candidata != null && candidata.esTransitable && !movimiento.EstaOcupada(candidata, null))
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

        //vamos a usar el metodo de unity de math.min porque asi en cantidad se guarda el minimo, o sea si tenemos disponibles 13
        //casillas, se guardara en cantidad 5, en cambio si tenemos disponibles solo 2 casillas pues se guardara este 2 como el minimo
        int cantidad = Mathf.Min(monedasPorTanda, disponibles.Count);

        for (int i = 0; i < cantidad; i++)  //hacemos el for para crear la cantidad seleccionada
        {
            //aleatoriamente seleccionamos una casilla disponible de la lista
            int indice = Random.Range(0, disponibles.Count);
            HexTile casilla = disponibles[indice];  //guardamos en casilla la casilla de la lista disponible seleccionada

            //crea una copia del prefab moneda y guarda la referencia de esa copia en monedacreada
            monedaCreada = Instantiate(moneda);
            Moneda datosMoneda = monedaCreada.GetComponent<Moneda>();  //busca el componente (script) moneda en nuestro objeto moneda
            datosMoneda.valor = Random.Range(2, 6);  //entonces hace cambios en ese componente en la variable de valor para que sea random
            monedasEnMapa++; //sumamos la moneda que acabamos de colocar

            //mueve la copia a la posición de la casilla seleccionada con el range
            monedaCreada.transform.position = casilla.transform.position;
            casilla.monedaEnCasilla = monedaCreada; //así esa casilla se guarda la referencia de la moneda que tiene creada ahi

            //quitamos esa casilla de la lista para que no vuelva a salir en la siguiente iteracion y en una misma haya dos monedas
            disponibles.RemoveAt(indice);
        }
    }

    //recibe el dato de la casilla en la que se esta para ver si es casillamoneda y también el tipo de unidad (si jugador o IA)
    public void RecogerMoneda(HexTile casillaDestino, UnidadHex unidad)  
    {

        if (casillaDestino.monedaEnCasilla != null)  //si hay una moneda en esa casilla
        {
            //leemos el valor de la moneda antes de retirarla del mapa
            Moneda datosMoneda = casillaDestino.monedaEnCasilla.GetComponent<Moneda>();  //busca el script de esa moneda especifica
            int valorRecogido = datosMoneda.valor;  //del script Moneda, se queda con el valor que tenga

            Destroy(casillaDestino.monedaEnCasilla);  //eliminamos la moneda que se encuentra en la casilla del jugador/IA
            casillaDestino.monedaEnCasilla = null;  //actualizamos casillaDestino porque ya no habrá una moneda en esa casilla

            //la moneda del mapa dara el saldo aleatorio que le haya tocado
            if (unidad.esJugador == true)  //si quien ha cogido la moneda es una unidad del jugador, ya usamos el esjugador en unidadhex
            {
                monedasJugador += valorRecogido;  //sumamos el valor de la moneda que haya cogido
            }
            else  //sino habra sido la de la IA
            {
                monedasIA += valorRecogido;
            }

            Debug.Log($"Moneda recogida. Jugador: {monedasJugador}, IA: {monedasIA}");  //para comprobar, ya lo haremos en canva

            monedasEnMapa--;

            if (monedasEnMapa == 0)  //cuando ya no queden monedas en el tablero, volveremos a colocar aleatoriamente 
            {
                ColocarMoneda();  //utilizando esta llamada que es el metodo que crea las 5 aleatorias
            }
        }
    }
}

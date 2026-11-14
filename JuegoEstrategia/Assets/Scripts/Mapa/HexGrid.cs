
using UnityEngine;
using System.Collections.Generic;

public class HexGrid : MonoBehaviour
{
    [Header("Configuracion del mapa")]

    // Numero de columnas y filas que tendra el mapa
    public int ancho = 10;
    public int alto = 10;

     // Tamaño usado para calcular la separacion entre casillas
    public float tamanoHexagono = 1f;

    [Header("Prefab de la casilla")]

    // Prefab que se clonara para crear cada casilla del mapa
    public GameObject prefabHexagono;

    // Guarda las casillas creadas, accesibles por sus coordenadas
    private HexTile[,] casillas;

    public bool mapaCreado = false;

    //gestor al que pediremos colocar la moneda cuando el mapa ya esté creado para que no dé problemas
    public GestorMonedas gestorMonedas;

    // Sprite que se asignara a cada tipo de terreno
    [Header("Sprites de terreno")]
    public Sprite spritePlanta;
    public Sprite spriteFuego;
    public Sprite spriteTierra;
    public Sprite spriteAgua;


    [Header("Generación por zonas")]
    public int numeroZonas = 8;
    public int radioZona = 2;

    [System.Serializable]
    public class ZonaTerreno
    {
        public TipoTerreno tipo;
        public int centroX;
        public int centroY;
    }

    private List<ZonaTerreno> zonas = new List<ZonaTerreno>();

    void Start()
    {
        CrearMapa();
    }

     // Crea las casillas y asigna a cada una sus coordenadas y terreno
    void CrearMapa()
    {
        // Comprueba que se haya asignado un prefab en el Inspector
        if (prefabHexagono == null)
        {
            Debug.LogError("Asigna el prefab de la casilla en el componente HexGrid.", this);
            return;
        }

        // Comprueba que el prefab tenga un SpriteRenderer en la raiz o en un hijo
        if (prefabHexagono.GetComponentInChildren<SpriteRenderer>() == null)
        {
            Debug.LogError("El prefab necesita un SpriteRenderer en el objeto raiz o en un hijo.", prefabHexagono);
            return;
        }

        // Comprueba que el prefab tenga el componente HexTile en la raíz o en un hijo
        if (prefabHexagono.GetComponentInChildren<HexTile>() == null)
        {
            Debug.LogError("El prefab necesita el componente HexTile en el objeto raiz o en un hijo.", prefabHexagono);
            return;
        }

        // Prepara una matriz para guardar cada casilla por sus coordenadas
        casillas = new HexTile[ancho, alto];

        CrearZonas();

        // Recorre todas las columnas
        for (int x = 0; x < ancho; x++)
        {

            // Recorre todas las filas de esta columna
            for (int y = 0; y < alto; y++)
            {

                //Calcula donde aparece esta casilla
                Vector3 posicion = ObtenerPosicionMundo(x, y);

                //Calcula una copia del prefab en esa posicion y la hace hijo de HexGrid
                GameObject nuevaCasilla = Instantiate(prefabHexagono,posicion,Quaternion.identity,transform);

                //Le pone un nombre para identificarla en la jerarquia
                nuevaCasilla.name = "Hexagono_" + x + "_" + y;

                 // Busca el componente HexTile en la casilla o en alguno de sus hijos
                HexTile hexTile = nuevaCasilla.GetComponentInChildren<HexTile>();

                 // Si no lo encuentra, muestra un error y elimina la casilla creada
                if (hexTile == null)
                {
                    Debug.LogError("No se encontró HexTile en la casilla creada.", nuevaCasilla);
                    Destroy(nuevaCasilla);
                    continue;
                }

                // Guarda las coordenadas de esta casilla
                hexTile.coordenadaX = x;
                hexTile.coordenadaY = y;

                 // Elige aleatoriamente el tipo de terreno
                TipoTerreno terreno = ObtenerTerrenoPorZona(x, y);

                // Configura las estadisticas de la casilla y le pasa el sprite adecuado
                hexTile.ConfigurarTerreno(terreno, ObtenerSprite(terreno));

                // Guarda la referencia a la casilla en la matriz
                casillas[x, y] = hexTile;

            }
        }

        mapaCreado = true;

        //despues de asignarle un gestor, le pedimos colocar la moneda estando ya el mapa creado
        if (gestorMonedas != null)
        {
            gestorMonedas.ColocarMoneda();
        }
    }

    // Devuelve un tipo de terreno aleatorio segun las probabilidades indicadas
    TipoTerreno ObtenerTerrenoAleatorio()
    {
        int valor = Random.Range(0, 100);

        // 60 % de probabilidad
        if (valor < 60)
        {
            return TipoTerreno.Tierra;
        }
        // 20 % de probabilidad
        else if (valor < 80)
        {
            return TipoTerreno.Fuego;
        }
        // 15 % de probabilidad
        else if (valor < 95)
        {
            return TipoTerreno.Planta;
        }
        // 5 % de probabilidad
        else
        {
            return TipoTerreno.Agua;
        }
    }

    // Calcula la posicion de una casilla segun su columna y fila
    Vector3 ObtenerPosicionMundo(int x, int y)
    {
        // Separacion horizontal entre hexagonos
        float pasoX = tamanoHexagono * 0.75f;

        // Separacion vertical entre hexagonos
        float pasoY = tamanoHexagono;

        // Posicion horizontal
        float posicionX = pasoX * x;

        // Posicion vertical
        float posicionY = pasoY * (y + (x % 2) * 0.5f);

        // Calcula el tamaño total aproximado del mapa
        float maxX = pasoX * (ancho - 1) + tamanoHexagono;
        float maxY = pasoY * (alto - 1 + ((ancho - 1) % 2) * 0.5f)+ tamanoHexagono;

        // Resta la mitad del tamaño para centrar el mapa en el objeto HexGrid
        posicionX -= maxX / 2f;
        posicionY -= maxY / 2f;

        // Devuelve la posicion desplazada segun la posicion del objeto HexGrid
        return transform.position + new Vector3(posicionX, posicionY, 0f);
    }

    // Devuelve el sprite segun el tipo de terreno
    Sprite ObtenerSprite(TipoTerreno terreno)
    {
        switch (terreno)
        {
            case TipoTerreno.Planta: return spritePlanta;
            case TipoTerreno.Fuego:  return spriteFuego;
            case TipoTerreno.Tierra: return spriteTierra;
            case TipoTerreno.Agua:   return spriteAgua;
            default: return null;
        }
    }

    public HexTile ObtenerCasilla(int x, int y)
    {
        // Comprueba que las coordenadas estén dentro del mapa
        if (x < 0 || x >= ancho || y < 0 || y >= alto)
        {
            return null;
        }

        // Devuelve la casilla que se encuentra en esas coordenadas
        return casillas[x, y];
    }

    public HexTile[,] ObtenerTodasLasCasillas()
    {
        return casillas;
    }

    void CrearZonas()
    {
        zonas.Clear();

        TipoTerreno[] terrenos ={ TipoTerreno.Tierra, TipoTerreno.Planta, TipoTerreno.Fuego, TipoTerreno.Agua };

        for (int i = 0; i < numeroZonas; i++)
        {
            int x = Random.Range(0, ancho);
            int y = Random.Range(0, alto);

            TipoTerreno tipo = terrenos[i % terrenos.Length];

            ZonaTerreno zona = new ZonaTerreno();
            zona.tipo = tipo;
            zona.centroX = x;
            zona.centroY = y;

            zonas.Add(zona);
        }
    }

    TipoTerreno ObtenerTerrenoPorZona(int x, int y)
    {
        ZonaTerreno zonaCercana = null;
        float distanciaMinima = float.MaxValue;

        foreach (ZonaTerreno zona in zonas)
        {
            float distancia = Vector2.Distance(new Vector2(x, y), new Vector2(zona.centroX, zona.centroY));

            //distancia += Random.Range(-0.8f, 0.8f);

            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                zonaCercana = zona;
            }
        }

        return zonaCercana.tipo;
    }

}

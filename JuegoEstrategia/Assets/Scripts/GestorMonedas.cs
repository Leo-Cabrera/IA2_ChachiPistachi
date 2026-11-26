
using UnityEngine;
using System;
using System.Collections.Generic;

public class GestorMonedas : MonoBehaviour
{
    public event Action OnMonedasActualizadas;

    [Header("Referencias")]
    public HexGrid grid;
    public MovimientoHex movimiento;
    public GameObject moneda;

    [Header("Monedas de los equipos")]
    [SerializeField] private int monedasJugador = 0;
    [SerializeField] private int monedasIA = 0;

    [Header("Zona de aparición")]
    [SerializeField] private int columnaMinima = 8;
    [SerializeField] private int columnaMaxima = 19;
    [SerializeField] private int filaMinima = 9;
    [SerializeField] private int filaMaxima = 11;
    [SerializeField] private int monedasPorTanda = 5;

    private GameObject monedaCreada;
    private int monedasEnMapa = 0;

    public int ObtenerMonedas(bool esJugador)
    {
        return esJugador ? monedasJugador : monedasIA;
    }

    private void NotificarCambioMonedas()
    {
        OnMonedasActualizadas?.Invoke();
    }

    public bool GastarMonedas(bool esJugador, int coste)
    {
        if (coste <= 0 || ObtenerMonedas(esJugador) < coste)
            return false;

        if (esJugador)
            monedasJugador -= coste;
        else
            monedasIA -= coste;

        NotificarCambioMonedas();
        return true;
    }

    public void RecompensarTropasQuietas(
        List<UnidadHex> tropas,
        bool esJugador)
    {
        if (tropas == null)
            return;

        int recompensaTotal = 0;

        foreach (UnidadHex unidad in tropas)
        {
            if (unidad == null)
                continue;

            // El jugador no ejecuta StayStill.
            // Para él, basta con no haber realizado ninguna otra acción.
            if (esJugador)
            {
                if (!unidad.PuedeRecibirRecompensaPorQuedarseQuietoJugador())
                    continue;
            }
            else
            {
                // La IA conserva su comprobación original.
                if (!unidad.PuedeRecibirRecompensaStayStill())
                    continue;
            }

            int nivel = unidad.datosTropa != null
                ? unidad.datosTropa.nivel
                : 0;

            int recompensaUnidad = Mathf.Clamp(nivel, 0, 2) + 1;

            recompensaTotal += recompensaUnidad;

            unidad.haRecibidoRecompensaStayStillEsteTurno = true;

            string nombre = unidad.datosTropa != null
                ? unidad.datosTropa.nombreTropa
                : unidad.name;

            Debug.Log(
                "[RECOMPENSA TURNO QUIETO] " +
                nombre + " recibe " +
                recompensaUnidad + " monedas."
            );
        }

        if (recompensaTotal <= 0)
            return;

        if (esJugador)
            monedasJugador += recompensaTotal;
        else
            monedasIA += recompensaTotal;

        NotificarCambioMonedas();

        Debug.Log(
            "[MONEDAS] " +
            (esJugador ? "Jugador" : "IA") +
            " recibe " + recompensaTotal +
            " monedas. Saldo jugador: " +
            monedasJugador +
            " | Saldo IA: " +
            monedasIA
        );
    }

    public void ColocarMoneda()
    {
        if (grid == null || movimiento == null || moneda == null)
        {
            Debug.LogError(
                "Faltan referencias en GestorMonedas."
            );
            return;
        }

        List<HexTile> disponibles = new List<HexTile>();

        for (int x = columnaMinima; x <= columnaMaxima; x++)
        {
            for (int y = filaMinima; y <= filaMaxima; y++)
            {
                HexTile candidata = grid.ObtenerCasilla(x, y);

                if (candidata != null &&
                    candidata.esTransitable &&
                    candidata.monedaEnCasilla == null &&
                    !movimiento.EstaOcupada(candidata, null))
                {
                    disponibles.Add(candidata);
                }
            }
        }

        Debug.Log(
            "Casillas disponibles para monedas: " +
            disponibles.Count
        );

        if (disponibles.Count == 0)
        {
            Debug.LogWarning(
                "No hay casillas disponibles para colocar monedas."
            );
            return;
        }

        int cantidad = Mathf.Min(
            monedasPorTanda,
            disponibles.Count
        );

        for (int i = 0; i < cantidad; i++)
        {
            int indice = UnityEngine.Random.Range(
                0,
                disponibles.Count
            );

            HexTile casilla = disponibles[indice];

            monedaCreada = Instantiate(moneda);

            Moneda datosMoneda =
                monedaCreada.GetComponent<Moneda>();

            if (datosMoneda == null)
            {
                Debug.LogError(
                    "El prefab de moneda no tiene el componente Moneda."
                );

                Destroy(monedaCreada);
                continue;
            }

            datosMoneda.valor = UnityEngine.Random.Range(2, 6);

            monedasEnMapa++;

            monedaCreada.transform.position =
                casilla.transform.position;

            casilla.monedaEnCasilla = monedaCreada;

            disponibles.RemoveAt(indice);
        }
    }

    public void RecogerMoneda(
        HexTile casillaDestino,
        UnidadHex unidad)
    {
        if (casillaDestino == null || unidad == null)
            return;

        GameObject monedaEnCasilla = casillaDestino.monedaEnCasilla;

        if (monedaEnCasilla == null)
            return;

        Moneda datosMoneda =
            monedaEnCasilla.GetComponent<Moneda>();

        if (datosMoneda == null)
        {
            Debug.LogWarning(
                "La moneda de la casilla no tiene componente Moneda."
            );
            return;
        }

        int valorRecogido = datosMoneda.valor;

        // Liberamos la casilla antes de destruir el objeto.
        casillaDestino.monedaEnCasilla = null;

        Destroy(monedaEnCasilla);

        if (unidad.esJugador)
            monedasJugador += valorRecogido;
        else
            monedasIA += valorRecogido;

        monedasEnMapa = Mathf.Max(0, monedasEnMapa - 1);

        NotificarCambioMonedas();

        Debug.Log(
            "[MONEDA RECOGIDA] " +
            unidad.name +
            " recoge " + valorRecogido +
            " monedas. Saldo jugador: " +
            monedasJugador +
            " | Saldo IA: " +
            monedasIA
        );

        if (monedasEnMapa == 0)
            ColocarMoneda();
    }
}

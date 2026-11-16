
using UnityEngine;

public class PesosTerreno : MonoBehaviour
{
    // 2 = favorito
    // 1 = neutral
    // 0 = intransitable

    public int ObtenerAfinidad(DatosTropa tropa, TipoTerreno terreno)
    {
        if (tropa == null) return 0;

        switch (tropa.tipoTropa)
        {
            case TipoTropa.Happiny:
            case TipoTropa.Timbur:
                return terreno == TipoTerreno.Agua ? 0 : 1;

            case TipoTropa.Oshawott:
                switch (terreno)
                {
                    case TipoTerreno.Agua: return 2;
                    case TipoTerreno.Tierra: return 1;
                    case TipoTerreno.Planta: return 1;
                    case TipoTerreno.Fuego: return 0;
                }
                break;

            case TipoTropa.Fennekin:
                switch (terreno)
                {
                    case TipoTerreno.Fuego: return 2;
                    case TipoTerreno.Tierra: return 1;
                    case TipoTerreno.Planta: return 1;
                    case TipoTerreno.Agua: return 0;
                }
                break;

            case TipoTropa.Turtwig:
                switch (terreno)
                {
                    case TipoTerreno.Planta: return 2;
                    case TipoTerreno.Tierra: return 1;
                    case TipoTerreno.Agua: return 1;
                    case TipoTerreno.Fuego: return 0;
                }
                break;
        }

        return 1;
    }

    // Coste de ENTRAR en una casilla.
    // Favorito = 1
    // Neutral = 2
    // Intransitable = 0

    public int ObtenerCoste(DatosTropa tropa, TipoTerreno terreno)
    {
        int afinidad = ObtenerAfinidad(tropa, terreno);

        if (afinidad == 0) return 0;
        if (afinidad == 2) return 1;

        return 2;
    }

    // Presupuesto de movimiento por turno.

    public int ObtenerPresupuesto(DatosTropa tropa)
    {
        if (tropa == null) return 0;

        switch (tropa.tipoTropa)
        {
            case TipoTropa.Happiny:
                return 2 + tropa.nivel;

            case TipoTropa.Timbur:
                return 2 + (tropa.nivel / 2);

            case TipoTropa.Oshawott:
                return 2 + tropa.nivel;

            case TipoTropa.Fennekin:
                return 2 + tropa.nivel;

            case TipoTropa.Turtwig:
                return 2 + tropa.nivel;
        }

        return 2;
    }
}

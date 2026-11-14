using UnityEngine;

public class PesosTerreno : MonoBehaviour
{
    public int ObtenerPeso(DatosTropa tropa, TipoTerreno terreno)
    {
        switch (tropa.tipoTropa)
        {
            case TipoTropa.Happiny:
                return 1 + tropa.nivel;

            case TipoTropa.Timbur:
                return 1 + (tropa.nivel / 2);

            case TipoTropa.Oshawott:
                switch (terreno)
                {
                    case TipoTerreno.Planta: return 0;
                    case TipoTerreno.Tierra: return 1 + tropa.nivel;
                    case TipoTerreno.Fuego: return 1 + tropa.nivel;
                    case TipoTerreno.Agua: return 2 + tropa.nivel;
                }
                break;

            case TipoTropa.Fennekin:
                switch (terreno)
                {
                    case TipoTerreno.Planta: return 1 + tropa.nivel;
                    case TipoTerreno.Tierra: return 1 + tropa.nivel;
                    case TipoTerreno.Fuego: return 2 + tropa.nivel;
                    case TipoTerreno.Agua: return 0;
                }
                break;

            case TipoTropa.Turwig:
                switch (terreno)
                {
                    case TipoTerreno.Planta: return 2 + tropa.nivel;
                    case TipoTerreno.Tierra: return 1 + tropa.nivel;
                    case TipoTerreno.Fuego: return 0;
                    case TipoTerreno.Agua: return 1 + tropa.nivel;
                }
                break;
        }

        return 0;
    }
}

using UnityEngine;

public class DatosTropa : MonoBehaviour
{
    [Header("Informacion de la tropa")]
    public string nombreTropa;

    public TipoTropa tipoTropa;

    public int nivel = 0;
}

public enum TipoTropa
{
    Happiny,
    Timbur,
    Oshawott,
    Fennekin,
    Turwig
}

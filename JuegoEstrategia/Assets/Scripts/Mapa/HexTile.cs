using UnityEngine;

public class HexTile : MonoBehaviour
{
    [Header("Coordenadas")]
    public int coordenadaX;
    public int coordenadaY;

    [Header("Terreno")]
    public TipoTerreno tipoTerreno;
    public bool esTransitable = true;

    [Header("Movimiento")]
    public float costeMovimiento = 1f;


    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void ConfigurarTerreno(TipoTerreno tipo, Sprite sprite)
    {
        tipoTerreno = tipo;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                Debug.LogError(
                    "No se encontró un SpriteRenderer en este HexTile ni en sus hijos.",
                    gameObject
                );
                return;
            }
        }

        spriteRenderer.sprite = sprite;

        switch (tipo)
        {
            case TipoTerreno.Planta:
                esTransitable = true;
                costeMovimiento = 1f;
                break;

            case TipoTerreno.Fuego:
                esTransitable = true;
                costeMovimiento = 2f;
                break;

            case TipoTerreno.Tierra:
                esTransitable = true;
                costeMovimiento = 1f;
                break;

            case TipoTerreno.Agua:
                esTransitable = false;
                costeMovimiento = 999f;
                break;
        }
    }
}
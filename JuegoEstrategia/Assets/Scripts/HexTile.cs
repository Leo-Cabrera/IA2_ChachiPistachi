using UnityEngine;

public class HexTile : MonoBehaviour
{
    [Header("Coordenadas")]
    public int coordenadaX;
    public int coordenadaY;

    [Header("Terreno")]
    public TipoTerreno tipoTerreno;
    public float costeMovimiento = 1f;
    public bool esTransitable = true;

    [Header("Variables tácticas")]
    public int riesgo;
    public int cobertura;
    public int altura;

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
                costeMovimiento = 1f;
                esTransitable = true;
                riesgo = 0;
                cobertura = 0;
                altura = 0;
                break;

            case TipoTerreno.Fuego:
                costeMovimiento = 2f;
                esTransitable = true;
                riesgo = 0;
                cobertura = 3;
                altura = 0;
                break;

            case TipoTerreno.Tierra:
                costeMovimiento = 3f;
                esTransitable = true;
                riesgo = 0;
                cobertura = 2;
                altura = 2;
                break;

            case TipoTerreno.Agua:
                costeMovimiento = 999f;
                esTransitable = false;
                riesgo = 0;
                cobertura = 0;
                altura = 0;
                break;
        }
    }
}
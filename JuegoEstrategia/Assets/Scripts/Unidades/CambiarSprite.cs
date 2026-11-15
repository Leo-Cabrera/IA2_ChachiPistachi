
using UnityEngine;

public class CambiarSprites : MonoBehaviour
{
    public Sprite sprite1;
    public Sprite sprite2;
    public Sprite sprite3;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Sprite inicial
        spriteRenderer.sprite = sprite1;
    }

    public void MostrarSprite1()
    {
        spriteRenderer.sprite = sprite1;
    }

    public void MostrarSprite2()
    {
        spriteRenderer.sprite = sprite2;
    }

    public void MostrarSprite3()
    {
        spriteRenderer.sprite = sprite3;
    }
}

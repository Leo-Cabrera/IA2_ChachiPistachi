
using UnityEngine;

public class CambiarSprites : MonoBehaviour
{
    public Sprite sprite1;
    public Sprite sprite2;
    public Sprite sprite3;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        //guardamos el componente del render porque lo necesitaremos para dibujar luego las evoluciones
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Sprite inicial
        //spriteRenderer.sprite = sprite1;
        ActualizarSpriteSegunNivel();
    }

    public void ActualizarSpriteSegunNivel()
    {
        UnidadHex unidad = GetComponent<UnidadHex>();

        //si la unidad todavia no tiene sus datos asignados, usamos su forma base
        if (unidad == null || unidad.datosTropa == null)
        {
            MostrarSprite1();
            return;
        }

        //el nivel 1 mostramos primera evolucion, en el 2 segunda, si no la default (cualquier otro valor) sera la forma base inicial 
        switch (unidad.datosTropa.nivel)
        {
            case 1:
                MostrarSprite2();
                break;
            case 2:
                MostrarSprite3();
                break;
            default:
                MostrarSprite1();
                break;
        }
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

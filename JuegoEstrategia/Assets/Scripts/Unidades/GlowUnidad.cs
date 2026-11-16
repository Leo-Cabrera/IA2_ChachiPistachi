using UnityEngine;

public class GlowUnidad : MonoBehaviour
{
    SpriteRenderer sr;
    MaterialPropertyBlock mpb;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>(); // busca también en hijos
        mpb = new MaterialPropertyBlock();

        if (sr == null)
            Debug.LogWarning("GlowUnidad: no hay SpriteRenderer en " + name);
    }

    public void SetGlow(bool encendido)
    {
        if (sr == null) return; // evita que el error rompa la selección

        sr.GetPropertyBlock(mpb);
        mpb.SetFloat("_GlowOn", encendido ? 1f : 0f);
        sr.SetPropertyBlock(mpb);
    }
}

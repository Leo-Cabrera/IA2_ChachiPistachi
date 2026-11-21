using UnityEngine;
using TMPro;

public class Moneda : MonoBehaviour
{
    //cantidad que gana el equipo al recoger esta moneda
    public int valor = 2;

    [SerializeField] private TextMeshPro textoValor;

    private void Start()  //se ejecutara una vez para cada copia
    {
        textoValor.text = "x" + valor;  //de esta manera el objeto del prefab, escrubirá el valor que obtenga cada moneda
    }
}

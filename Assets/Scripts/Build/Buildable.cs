using UnityEngine;

[System.Serializable]
public class Buildable
{
    public string name;
    public GameObject prefab;

    // tamaño en tiles (ancho, alto). Usa Vector2Int para evitar errores de casteo.
    public Vector2Int size = Vector2Int.one;
}

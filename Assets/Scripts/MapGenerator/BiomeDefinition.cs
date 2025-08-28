using UnityEngine;
using UnityEngine.Tilemaps;


[System.Serializable]
public struct DecorationOption
{
    public GameObject prefab;
    [Range(0f, 1f)] public float probability;
}



[CreateAssetMenu(fileName = "Biome", menuName = "World/Biome")]
public class BiomeDefinition : ScriptableObject
{
    public string biomeName;
    public TileBase[] groundTile;
    [Range(0f, 1f)] public float minHeight = 0f;
    [Range(0f, 1f)] public float maxHeight = 1f;
    [Range(0f, 1f)] public float minMoisture = 0f;
    [Range(0f, 1f)] public float maxMoisture = 1f;

    [Header("Decoraci�n")]
    public DecorationOption[] decorations;
}

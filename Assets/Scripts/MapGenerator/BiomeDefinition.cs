using UnityEngine;
using UnityEngine.Tilemaps;


[System.Serializable]
public struct DecorationOption
{
    public GameObject prefab;
    [Range(0f, 1f)] public float probability;
}

[System.Serializable]
public struct MobOption
{
    public GameObject prefab;
    [Range(0f, 1f)] public float probability; // chance de spawn
    public int minGroup;
    public int maxGroup;
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

    [Header("Decoración")]
    public DecorationOption[] decorations;

    [Header("Mobs (animales o enemigos)")]
    public MobOption[] mobs;
}

using UnityEngine;

[CreateAssetMenu(fileName = "BiomeLibrary", menuName = "World/BiomeLibrary")]
public class BiomeLibrary : ScriptableObject
{
    public BiomeDefinition[] biomes;

    public BiomeDefinition GetBiome(float height, float moisture)
    {
        foreach (var biome in biomes)
        {
            if (height >= biome.minHeight && height <= biome.maxHeight &&
                moisture >= biome.minMoisture && moisture <= biome.maxMoisture)
            {
                return biome;
            }
        }
        return biomes.Length > 0 ? biomes[0] : null; // fallback
    }
}

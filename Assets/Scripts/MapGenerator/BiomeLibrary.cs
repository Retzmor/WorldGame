using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "BiomeLibrary", menuName = "World/BiomeLibrary")]
public class BiomeLibrary : ScriptableObject
{
    public BiomeDefinition[] biomes;

    // fallback simple: devuelve el primer bioma que cumpla las condiciones.
    // Lo usamos solo como fallback; el WorldGenerator hace una selección más precisa.
    public BiomeDefinition GetBiome(float height, float moisture)
    {
        foreach (var biome in biomes)
        {
            if (biome == null) continue;
            if (height >= biome.minHeight && height <= biome.maxHeight &&
                moisture >= biome.minMoisture && moisture <= biome.maxMoisture)
            {
                return biome;
            }
        }
        return biomes.Length > 0 ? biomes[0] : null; // fallback
    }

    public TileBase GetTileByName(string name)
    {
        foreach (var biome in biomes)
            foreach (var t in biome.groundTile)
                if (t != null && t.name == name)
                    return t;
        return null;
    }

    public GameObject GetDecorationPrefab(string name)
    {
        foreach (var biome in biomes)
            foreach (var d in biome.decorations)
                if (d.prefab != null && d.prefab.name == name)
                    return d.prefab;
        return null;
    }

}

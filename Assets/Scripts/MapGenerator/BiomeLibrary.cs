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
        if (string.IsNullOrEmpty(name)) return null;

        foreach (var biome in biomes)
        {
            if (biome == null) continue;

            var groundField = biome.GetType().GetField("groundTile");
            if (groundField == null) continue;
            var groundVal = groundField.GetValue(biome);
            if (groundVal == null) continue;

            // Caso clásico: TileBase[]
            if (groundVal is TileBase[] tbArr)
            {
                foreach (var t in tbArr)
                    if (t != null && t.name == name)
                        return t;
            }
            else
            {
                // Caso nuevo: TileOption[] (u otro enumerable con .tile)
                if (groundVal is System.Collections.IEnumerable en)
                {
                    foreach (var item in en)
                    {
                        if (item == null) continue;

                        if (item is TileBase tb)
                        {
                            if (tb.name == name) return tb;
                            continue;
                        }

                        var fi = item.GetType().GetField("tile");
                        if (fi != null)
                        {
                            var val = fi.GetValue(item) as TileBase;
                            if (val != null && val.name == name) return val;
                        }
                        else
                        {
                            var pi = item.GetType().GetProperty("tile");
                            if (pi != null)
                            {
                                var val = pi.GetValue(item, null) as TileBase;
                                if (val != null && val.name == name) return val;
                            }
                        }
                    }
                }
            }
        }
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

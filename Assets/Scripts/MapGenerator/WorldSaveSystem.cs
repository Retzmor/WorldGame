using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Sistema de guardado simple: guarda cambios por chunk (tiles modificados y decoraciones).
/// Usa JSON con clases serializables que contienen primitivas (evita problemas con Vector2Int en JsonUtility).
/// </summary>
public class WorldSaveSystem : MonoBehaviour
{
    [Header("Referencias (asignar en inspector)")]
    public BiomeLibrary biomeLibrary;           // para resolver prefabs/tiles por nombre
    public List<TileBase> extraTiles = new();  // p.ej. asigna waterTile aquí si lo deseas

    // Estructuras serializables
    [System.Serializable]
    public class WorldSaveData
    {
        public List<ChunkSaveData> chunks = new List<ChunkSaveData>();
    }

    [System.Serializable]
    public class ChunkSaveData
    {
        public int chunkX;
        public int chunkY;
        public List<TileChange> changedTiles = new List<TileChange>();
        public List<DecorationSave> decorations = new List<DecorationSave>();
    }

    [System.Serializable]
    public class TileChange
    {
        public int x, y, z;
        public string tileID; // "null" para vacío
    }

    [System.Serializable]
    public class DecorationSave
    {
        public string prefabName;
        public float x, y, z;
        public float qx, qy, qz, qw;
        public bool active;
    }

    // Diccionario en memoria: clave chunk
    private readonly Dictionary<Vector2Int, ChunkSaveData> worldSaveDict = new();

    // -----------------------
    // API pública
    // -----------------------

    public ChunkSaveData GetChunkSave(Vector2Int chunkCoord)
    {
        worldSaveDict.TryGetValue(chunkCoord, out var chunk);
        return chunk;
    }

    // Guardar cambio de tile (pos absoluta en mundo)
    public void SaveTileChange(Vector3Int pos, TileBase newTile, Vector2Int chunkCoord)
    {
        var chunk = EnsureChunk(chunkCoord);
        string id = newTile != null ? newTile.name : "null";

        var existing = chunk.changedTiles.Find(t => t.x == pos.x && t.y == pos.y && t.z == pos.z);
        if (existing != null)
        {
            existing.tileID = id;
        }
        else
        {
            chunk.changedTiles.Add(new TileChange { x = pos.x, y = pos.y, z = pos.z, tileID = id });
        }
    }

    // Guardar/actualizar estado de una decoración (active = true => existe; false => destruida)
    // El obj puede ser cualquier GameObject instanciado (decoración). Busca por prefabName + posición cercana.
    public void SaveDecorationChange(GameObject obj, Vector2Int chunkCoord, bool active)
    {
        if (obj == null) return;
        var chunk = EnsureChunk(chunkCoord);

        string prefabName = GetPrefabName(obj);
        Vector3 p = obj.transform.position;
        float px = p.x, py = p.y, pz = p.z;
        Quaternion q = obj.transform.rotation;

        // buscar existente por prefabName y cercanía (evita duplicados)
        DecorationSave found = null;
        foreach (var d in chunk.decorations)
        {
            if (d.prefabName == prefabName)
            {
                // distancia simple
                float dx = d.x - px, dy = d.y - py, dz = d.z - pz;
                if (dx * dx + dy * dy + dz * dz < 0.01f) // tolerancia
                {
                    found = d;
                    break;
                }
            }
        }

        if (found != null)
        {
            found.x = px; found.y = py; found.z = pz;
            found.qx = q.x; found.qy = q.y; found.qz = q.z; found.qw = q.w;
            found.active = active;
        }
        else
        {
            chunk.decorations.Add(new DecorationSave
            {
                prefabName = prefabName,
                x = px,
                y = py,
                z = pz,
                qx = q.x,
                qy = q.y,
                qz = q.z,
                qw = q.w,
                active = active
            });
        }
    }

    // Buscar TileBase por nombre: revisa biomes (ground tiles) y extraTiles
    public TileBase GetTileBaseByName(string name)
    {
        if (name == null) return null;
        if (name == "null") return null;

        if (biomeLibrary != null && biomeLibrary.biomes != null)
        {
            foreach (var b in biomeLibrary.biomes)
            {
                if (b == null || b.groundTile == null) continue;
                foreach (var t in b.groundTile)
                {
                    if (t != null && t.name == name) return t;
                }
            }
        }

        if (extraTiles != null)
        {
            foreach (var t in extraTiles)
            {
                if (t != null && t.name == name) return t;
            }
        }

        return null;
    }

    // Buscar prefab de decoración por nombre (revisa todos los biomas)
    public GameObject GetDecorationPrefabByName(string prefabName)
    {
        if (biomeLibrary == null || biomeLibrary.biomes == null) return null;
        foreach (var b in biomeLibrary.biomes)
        {
            if (b == null || b.decorations == null) continue;
            foreach (var d in b.decorations)
            {
                if (d.prefab != null && d.prefab.name == prefabName) return d.prefab;
            }
        }
        return null;
    }

    // Guardar todo a archivo
    public void SaveWorld(string fileName = "world.json")
    {
        string path = Application.persistentDataPath + "/" + fileName;
        var data = new WorldSaveData { chunks = new List<ChunkSaveData>(worldSaveDict.Values) };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"WorldSaveSystem: mundo guardado en {path}");
    }

    // Cargar desde archivo
    public void LoadWorld(string fileName = "world.json")
    {
        string path = Application.persistentDataPath + "/" + fileName;
        if (!File.Exists(path))
        {
            Debug.Log("WorldSaveSystem: no hay archivo de guardado. Se iniciará mundo procedimental.");
            return;
        }

        string json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<WorldSaveData>(json);
        worldSaveDict.Clear();
        if (data == null || data.chunks == null) return;
        foreach (var c in data.chunks)
        {
            var key = new Vector2Int(c.chunkX, c.chunkY);
            worldSaveDict[key] = c;
        }
        Debug.Log($"WorldSaveSystem: mundo cargado desde {path} ({worldSaveDict.Count} chunks en memoria)");
    }

    // -----------------------
    // Helpers privados
    // -----------------------
    private ChunkSaveData EnsureChunk(Vector2Int chunkCoord)
    {
        if (!worldSaveDict.TryGetValue(chunkCoord, out var chunk))
        {
            chunk = new ChunkSaveData { chunkX = chunkCoord.x, chunkY = chunkCoord.y };
            worldSaveDict[chunkCoord] = chunk;
        }
        return chunk;
    }

    private string GetPrefabName(GameObject obj)
    {
        var pr = obj.GetComponent<PrefabReference>();
        if (pr != null && pr.prefab != null) return pr.prefab.name;
        return obj.name.Replace("(Clone)", "").Trim();
    }
}

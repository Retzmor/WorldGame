using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using static GameManager;

/// <summary>
/// Sistema de guardado simple: guarda cambios por chunk (tiles modificados y decoraciones).
/// Usa JSON con clases serializables que contienen primitivas (evita problemas con Vector2Int en JsonUtility).
/// </summary>
public class WorldSaveSystem : MonoBehaviour
{

    [Header("Referencias (asignar en inspector)")]
    public BiomeLibrary biomeLibrary;           // para resolver prefabs/tiles por nombre
    public List<TileBase> extraTiles = new();  // p.ej. asigna waterTile aquí si lo deseas

   
    public int currentSlot = 1;


    //data actual de la partida seleccionada
    public WorldMeta currentData;

    // Estructuras serializables
    [System.Serializable]
    public class WorldSaveData
    {
        public List<ChunkSaveData> chunks = new List<ChunkSaveData>();
        public PlayerSaveData player;  // ⬅️ NUEVO
    }

    [System.Serializable]
    public class PlayerSaveData
    {
        public float posX, posY;
        public List<ItemSave> inventory;
    }

    [System.Serializable]
    public class ItemSave
    {
        public string itemID;
        public int quantity;
        public int durability;
        public bool equipped;
        public int slotIndex; // ✅ nuevo campo para saber dónde estaba
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
        if (string.IsNullOrEmpty(name)) return null;
        if (name == "null") return null;

        if (biomeLibrary != null && biomeLibrary.biomes != null)
        {
            foreach (var b in biomeLibrary.biomes)
            {
                if (b == null) continue;

                // ==== Manejo compatible con ambos formatos ====
                // - Si groundTile es ahora TileOption[] (cada opción tiene .tile)
                // - O si groundTile sigue siendo TileBase[]
                // Para ser robusto hacemos una iteración genérica y comprobamos cada item.

                var groundField = b.GetType().GetField("groundTile");
                if (groundField == null) continue;
                var groundVal = groundField.GetValue(b);
                if (groundVal == null) continue;

                // Si es TileBase[]
                if (groundVal is TileBase[] tbArr)
                {
                    foreach (var t in tbArr)
                        if (t != null && t.name == name) return t;
                }
                else
                {
                    // Intentar enumerar elementos y extraer 'tile' (caso TileOption[] u otro)
                    if (groundVal is System.Collections.IEnumerable en)
                    {
                        foreach (var item in en)
                        {
                            if (item == null) continue;

                            // Si el elemento ya es TileBase
                            if (item is TileBase tb)
                            {
                                if (tb.name == name) return tb;
                                continue;
                            }

                            // Si el elemento tiene un campo o propiedad 'tile' que sea TileBase
                            var fi = item.GetType().GetField("tile");
                            if (fi != null)
                            {
                                var val = fi.GetValue(item) as TileBase;
                                if (val != null && val.name == name) return val;
                            }
                            else
                            {
                                // intentar propiedad 'tile'
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
    public void SaveWorld()
    {
        string path = Application.persistentDataPath + "/" + GetSlotFileName();
        var data = new WorldSaveData { chunks = new List<ChunkSaveData>(worldSaveDict.Values) };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"WorldSaveSystem: mundo guardado en {path}");
    }

    // Cargar desde archivo
    public void LoadWorld()
    {
        string path = Application.persistentDataPath + "/" + GetSlotFileName();
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


    private void Start()
    {
        Debug.Log("Ruta de guardado: " + Application.persistentDataPath);
    }


    public void DeleteWorld()
    {
        string path = Application.persistentDataPath + "/" + GetSlotFileName();
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"WorldSaveSystem: archivo de guardado eliminado en {GetSlotFileName()}");
        }
        else
        {
            Debug.Log("WorldSaveSystem: no había archivo de guardado para borrar.");
        }

        worldSaveDict.Clear(); // también vacía la memoria en runtime
    }


    public void SavePlayer(Transform playerTransform, List<ItemSave> inventory)
    {
        Debug.Log($"[SavePlayer] Inventario recibido: {inventory?.Count ?? -1}");
        if (inventory != null)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                var it = inventory[i];
                Debug.Log($"Item {i}: id={it.itemID}, qty={it.quantity}, dura={it.durability}");
            }
        }
        if (!worldSaveDict.TryGetValue(Vector2Int.zero, out _))
        {
            // asegura al menos un chunk dummy (para que el save no quede vacío)
            worldSaveDict[new Vector2Int(0, 0)] = new ChunkSaveData { chunkX = 0, chunkY = 0 };
        }

        // Crear PlayerSaveData
        PlayerSaveData playerData = new PlayerSaveData();
        playerData.posX = playerTransform.position.x;
        playerData.posY = playerTransform.position.y;


        // Copiar inventario
        playerData.inventory = new List<ItemSave>(inventory);
                        

        // Guardar dentro del mundo
        WorldSaveData data = new WorldSaveData { chunks = new List<ChunkSaveData>(worldSaveDict.Values), player = playerData };

        string path = Application.persistentDataPath + "/" + GetSlotFileName();
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);

        Debug.Log($"WorldSaveSystem: mundo+jugador guardados en {path}");
    }

    // Cargar datos del jugador
    public PlayerSaveData LoadPlayer()
    {
        string path = Application.persistentDataPath + "/" + GetSlotFileName();
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<WorldSaveData>(json);

        worldSaveDict.Clear();
        if (data != null && data.chunks != null)
        {
            foreach (var c in data.chunks)
            {
                var key = new Vector2Int(c.chunkX, c.chunkY);
                worldSaveDict[key] = c;
            }
        }

        Debug.Log($"WorldSaveSystem: mundo+jugador cargados desde {path}");
        return data.player;
    }


    public string GetSlotFileName()
    {
        return $"world_slot{currentSlot}.json";
    }

    public void ChangeCurrentData(WorldMeta worldData)
    {
        currentData = worldData;
        currentSlot = currentData.slot;
    }
}

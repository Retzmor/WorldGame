using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;
using static WorldSaveSystem;

public class WorldGenerator : MonoBehaviour
{
    [Inject] private DiContainer _container;
    [Header("Refs")]
    public Tilemap groundTilemap;
    public Tilemap waterTilemap;
    public Transform player;
    public BiomeLibrary biomeLibrary;
    public Transform decorationParent; // opcional parent para decoraciones

    [Header("Chunk Settings")]
    public int chunkSize = 32;
    public int viewRadiusChunks = 3;

    [Header("Noise Settings")]
    public int seed = 12345;
    public float terrainScale = 20f;
    public float moistureScale = 50f;
    [Tooltip("Escala usada sólo para dividir/selectar biomas. Valores menores => biomas más pequeños.")]
    public float biomeScale = 8f;

    [Header("Water Settings")]
    [Range(0f, 1f)] public float waterLevel = 0.3f;
    public TileBase waterTile;

    [Header("Respawn Settings")]
    public float respawnInterval = 5f;
    public int maxMobsGlobal = 10; // límite global de mobs activos

    [Header("Save System")]
    public WorldSaveSystem saveSystem; // asignar en inspector

    [Header("Decoration Settings")]
    public float minDecorationSpacing = 1f; // distancia mínima en unidades del mundo

    // Offsets para ruido (determinístico por seed)
    private System.Random prng;
    private float terrainOffsetX, terrainOffsetY;
    private float moistureOffsetX, moistureOffsetY;
    private float biomeOffsetX, biomeOffsetY;

    // Estructuras
    private readonly Dictionary<Vector2Int, bool> loadedChunks = new();
    private readonly List<Vector2Int> chunksToDraw = new();
    private readonly Dictionary<Vector2Int, List<GameObject>> chunkObjects = new();
    private readonly Dictionary<Vector2Int, BiomeDefinition> chunkBiomes = new();

    // Pooling
    private readonly Dictionary<GameObject, Queue<GameObject>> objectPool = new();

    private Coroutine loaderRoutine;
    private Coroutine respawnRoutine;

    // Buffers reutilizables
    private Vector3Int[] tilePositions;
    private TileBase[] groundTileBuffer;
    private TileBase[] waterTileBuffer;

    void Awake()
    {
        prng = new System.Random(seed);
        terrainOffsetX = prng.Next(-100000, 100000);
        terrainOffsetY = prng.Next(-100000, 100000);
        moistureOffsetX = prng.Next(-100000, 100000);
        moistureOffsetY = prng.Next(-100000, 100000);
        biomeOffsetX = prng.Next(-100000, 100000);
        biomeOffsetY = prng.Next(-100000, 100000);

        int count = chunkSize * chunkSize;
        tilePositions = new Vector3Int[count];
        groundTileBuffer = new TileBase[count];
        waterTileBuffer = new TileBase[count];
    }

    void Start()
    {
        saveSystem = FindAnyObjectByType<WorldSaveSystem>();
        if (saveSystem != null)
        {
            // cargar mundo
            saveSystem.LoadWorld();

            // cargar jugador
            var playerData = saveSystem.LoadPlayer();
            if (playerData != null)
            {
                // colocar jugador en posición guardada
                player.position = new Vector3(playerData.posX, playerData.posY);
                //player.rotation = new Quaternion(playerData.rotX, playerData.rotY, playerData.rotZ, playerData.rotW);
                Debug.Log(playerData.posX);
                // restaurar inventario aquí (según tu sistema de inventario)
                Debug.Log($"Inventario cargado con {playerData.inventory.Count} items");
            }
        }
        loaderRoutine = StartCoroutine(LoaderLoop());
        respawnRoutine = StartCoroutine(RespawnLoop());

        
    }

    void OnApplicationQuit()
    {
        if (saveSystem != null)
        {
            saveSystem.SaveWorld(); // ⬅️ guarda al cerrar juego
            List<ItemSave> currentInventory = new List<ItemSave>();
            // TODO: aquí rellena con los ítems reales del inventario del jugador
            saveSystem.SavePlayer(player, currentInventory);
        }
    }


    void OnDisable()
    {
        if (loaderRoutine != null) StopCoroutine(loaderRoutine);
        if (respawnRoutine != null) StopCoroutine(respawnRoutine);
    }

    IEnumerator LoaderLoop()
    {
        var wait = new WaitForSeconds(0.1f);
        while (true)
        {
            UpdateVisibleChunks();

            int budget = 1;
            while (chunksToDraw.Count > 0 && budget-- > 0)
            {
                var c = chunksToDraw[0];
                chunksToDraw.RemoveAt(0);
                GenerateChunk(c);
                loadedChunks[c] = true;
                if (chunkSize >= 48) yield return null;
            }

            yield return wait;
        }
    }

    void UpdateVisibleChunks()
    {
        Vector2Int playerChunk = WorldToChunk(player.position);
        HashSet<Vector2Int> desired = new();

        for (int cy = -viewRadiusChunks; cy <= viewRadiusChunks; cy++)
        {
            for (int cx = -viewRadiusChunks; cx <= viewRadiusChunks; cx++)
            {
                var c = new Vector2Int(playerChunk.x + cx, playerChunk.y + cy);
                desired.Add(c);
                if (!loadedChunks.ContainsKey(c))
                {
                    loadedChunks[c] = false;
                    chunksToDraw.Add(c);
                }
            }
        }

        List<Vector2Int> toUnload = new();
        foreach (var kv in loadedChunks)
            if (kv.Value && !desired.Contains(kv.Key))
                toUnload.Add(kv.Key);

        for (int i = 0; i < toUnload.Count; i++)
        {
            UnloadChunk(toUnload[i]);
            loadedChunks.Remove(toUnload[i]);
        }

        chunksToDraw.Sort((a, b) => SqrDist(a, playerChunk).CompareTo(SqrDist(b, playerChunk)));
    }

    static int SqrDist(Vector2Int a, Vector2Int b)
    {
        int dx = a.x - b.x, dy = a.y - b.y;
        return dx * dx + dy * dy;
    }

    public Vector2Int WorldToChunk(Vector3 worldPos)
    {
        int wx = Mathf.FloorToInt(worldPos.x / 0.5f);
        int wy = Mathf.FloorToInt(worldPos.y / 0.5f);
        int cx = FloorDiv(wx, chunkSize);
        int cy = FloorDiv(wy, chunkSize);
        return new Vector2Int(cx, cy);
    }

    static int FloorDiv(int a, int b)
    {
        int q = a / b;
        int r = a % b;
        return (r != 0 && ((r > 0) != (b > 0))) ? q - 1 : q;
    }

    void GenerateChunk(Vector2Int c)
    {
        int count = chunkSize * chunkSize;
        System.Random chunkRng = new System.Random(ChunkSeed(seed, c.x, c.y));
        if (!chunkObjects.ContainsKey(c)) chunkObjects[c] = new List<GameObject>();

        var chunkSave = saveSystem != null ? saveSystem.GetChunkSave(c) : null;

        int i = 0;
        int startX = c.x * chunkSize;
        int startY = c.y * chunkSize;

        BiomeDefinition dominantBiome = null;
        Dictionary<BiomeDefinition, int> biomeCounts = new();

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++, i++)
            {
                int wx = startX + x;
                int wy = startY + y;

                float height = Mathf.PerlinNoise((wx + terrainOffsetX) / terrainScale,
                                                 (wy + terrainOffsetY) / terrainScale);
                float moisture = Mathf.PerlinNoise((wx + moistureOffsetX) / moistureScale,
                                                   (wy + moistureOffsetY) / moistureScale);

                bool isWater = height < waterLevel;
                tilePositions[i] = new Vector3Int(wx, wy, 0);

                if (isWater)
                {
                    groundTileBuffer[i] = null;
                    waterTileBuffer[i] = waterTile;
                    continue;
                }
                else
                {
                    waterTileBuffer[i] = null;
                }

                BiomeDefinition chosen = null;
                float bestScore = float.MaxValue;

                if (biomeLibrary != null && biomeLibrary.biomes != null && biomeLibrary.biomes.Length > 0)
                {
                    foreach (var b in biomeLibrary.biomes)
                    {
                        if (b == null) continue;
                        if (height >= b.minHeight && height <= b.maxHeight &&
                            moisture >= b.minMoisture && moisture <= b.maxMoisture)
                        {
                            float midH = (b.minHeight + b.maxHeight) * 0.5f;
                            float midM = (b.minMoisture + b.maxMoisture) * 0.5f;
                            float dh = height - midH;
                            float dm = moisture - midM;
                            float score = dh * dh + dm * dm;
                            if (score < bestScore)
                            {
                                bestScore = score;
                                chosen = b;
                            }
                        }
                    }

                    if (chosen == null)
                        chosen = biomeLibrary.GetBiome(height, moisture);
                }

                if (chosen != null && chosen.groundTile != null && chosen.groundTile.Length > 0)
                {
                    float roll = (float)chunkRng.NextDouble();
                    float cumulative = 0f;
                    TileBase selectedTile = null;

                    foreach (var tileOpt in chosen.groundTile)
                    {
                        if (tileOpt.tile == null || tileOpt.probability <= 0f) continue;

                        cumulative += tileOpt.probability;
                        if (roll <= cumulative)
                        {
                            selectedTile = tileOpt.tile;
                            break;
                        }
                    }

                    // fallback por si las probabilidades no suman 1 exacto
                    if (selectedTile == null && chosen.groundTile.Length > 0)
                        selectedTile = chosen.groundTile[0].tile;

                    groundTileBuffer[i] = selectedTile;
                    if(!biomeCounts.ContainsKey(chosen)) biomeCounts[chosen] = 0;
                    biomeCounts[chosen]++;
                }
                else
                {
                    groundTileBuffer[i] = null;
                }

                // GENERAR DECORACIONES PROCEDURALES
                if (chosen != null && chosen.decorations != null && chosen.decorations.Length > 0)
                {
                    float roll = (float)chunkRng.NextDouble();
                    float cumulative = 0f;
                    for (int d = 0; d < chosen.decorations.Length; d++)
                    {
                        var deco = chosen.decorations[d];
                        if (deco.prefab == null || deco.probability <= 0f) continue;
                        cumulative += deco.probability;
                        if (roll <= cumulative)
                        {
                            Vector3 spawnPos = new Vector3((wx + 0.5f) * 0.5f, (wy + 0.5f) * 0.5f, 0f);

                            // ✅ Comprobar distancia mínima con decoraciones ya generadas
                            bool tooClose = false;
                            foreach (var other in chunkObjects[c])
                            {
                                if (other == null) continue;
                                if (Vector3.Distance(other.transform.position, spawnPos) < minDecorationSpacing)
                                {
                                    tooClose = true;
                                    break;
                                }
                            }

                            if (tooClose) continue; // saltar este spawn

                            // ✅ Ahora sí instanciar porque pasó la validación
                            var obj = GetFromPool(deco.prefab, spawnPos, Quaternion.identity);
                            if (decorationParent != null) obj.transform.SetParent(decorationParent, true);
                            chunkObjects[c].Add(obj);
                            break;
                        }
                    }
                }
            }
        }

        groundTilemap.SetTiles(tilePositions, groundTileBuffer);
        waterTilemap.SetTiles(tilePositions, waterTileBuffer);

        if (biomeCounts.Count > 0)
        {
            int maxCount = -1;
            foreach (var kv in biomeCounts)
            {
                if (kv.Value > maxCount)
                {
                    maxCount = kv.Value;
                    dominantBiome = kv.Key;
                }
            }
            chunkBiomes[c] = dominantBiome;
        }
        else
        {
            chunkBiomes[c] = null;
        }

        // ----------------------------
        // APLICAR CAMBIOS DEL JSON
        // ----------------------------
        if (chunkSave != null && chunkSave.decorations != null)
        {
            foreach (var deco in chunkSave.decorations)
            {
                // Buscar si ya hay una decoración en esa posición
                Vector3 pos = new Vector3(deco.x, deco.y, deco.z);

                if (!deco.active)
                {
                    // Buscar árbol/procedural que coincida y eliminarlo
                    var toRemove = chunkObjects[c].Find(o =>
                        o != null &&
                        Mathf.Abs(o.transform.position.x - pos.x) < 0.1f &&
                        Mathf.Abs(o.transform.position.y - pos.y) < 0.1f);

                    if (toRemove != null)
                    {
                        chunkObjects[c].Remove(toRemove);
                        ReturnToPool(toRemove);
                    }
                }
                else
                {
                    // Si está marcado como activo pero no existe, recrearlo
                    GameObject prefab = saveSystem.GetDecorationPrefabByName(deco.prefabName);
                    if (prefab != null)
                    {
                        var existing = chunkObjects[c].Find(o =>
                            o != null &&
                            Mathf.Abs(o.transform.position.x - pos.x) < 0.1f &&
                            Mathf.Abs(o.transform.position.y - pos.y) < 0.1f);

                        if (existing == null)
                        {
                            Quaternion rot = new Quaternion(deco.qx, deco.qy, deco.qz, deco.qw);
                            var obj = GetFromPool(prefab, pos, rot);
                            if (decorationParent != null) obj.transform.SetParent(decorationParent, true);
                            chunkObjects[c].Add(obj);
                        }
                    }
                }
            }
        }
    }

    void UnloadChunk(Vector2Int c)
    {
        int i = 0;
        int startX = c.x * chunkSize;
        int startY = c.y * chunkSize;

        for (int y = 0; y < chunkSize; y++)
            for (int x = 0; x < chunkSize; x++, i++)
                tilePositions[i] = new Vector3Int(startX + x, startY + y, 0);

        System.Array.Clear(groundTileBuffer, 0, groundTileBuffer.Length);
        System.Array.Clear(waterTileBuffer, 0, waterTileBuffer.Length);
        groundTilemap.SetTiles(tilePositions, groundTileBuffer);
        waterTilemap.SetTiles(tilePositions, waterTileBuffer);

        if (chunkObjects.TryGetValue(c, out var objects))
        {
            for (int j = objects.Count - 1; j >= 0; j--)
            {
                var obj = objects[j];
                if (obj == null)
                {
                    objects.RemoveAt(j);
                    continue;
                }

               

                ReturnToPool(obj);
                objects.RemoveAt(j);
            }
        }

        chunkBiomes.Remove(c);
    }

    IEnumerator RespawnLoop()
    {
        var wait = new WaitForSeconds(respawnInterval);

        while (true)
        {
            int totalMobs = 0;
            foreach (var list in chunkObjects.Values)
                for (int i = 0; i < list.Count; i++)
                    if (list[i] != null && list[i].CompareTag("Mob") && list[i].activeSelf)
                        totalMobs++;

            if (totalMobs >= maxMobsGlobal)
            {
                yield return wait;
                continue;
            }

            foreach (var kv in loadedChunks)
            {
                if (totalMobs >= maxMobsGlobal) break;
                if (!kv.Value) continue;

                Vector2Int chunk = kv.Key;
                if (!chunkBiomes.TryGetValue(chunk, out var biome)) continue;
                if (biome == null || biome.mobs == null || biome.mobs.Length == 0)
                    continue;

                for (int m = 0; m < biome.mobs.Length; m++)
                {
                    if (totalMobs >= maxMobsGlobal) break;

                    var mobOpt = biome.mobs[m];
                    if (Random.value > mobOpt.probability) continue;

                    int groupSize = Random.Range(mobOpt.minGroup, mobOpt.maxGroup + 1);

                    for (int g = 0; g < groupSize; g++)
                    {
                        if (totalMobs >= maxMobsGlobal) break;

                        Vector3 spawnPos = Vector3.zero;
                        bool valid = false;
                        for (int attempt = 0; attempt < 10 && !valid; attempt++)
                        {
                            float px = chunk.x * chunkSize + Random.Range(0, chunkSize);
                            float py = chunk.y * chunkSize + Random.Range(0, chunkSize);

                            spawnPos = new Vector3((px + 0.5f) * 0.5f, (py + 0.5f) * 0.5f, 0f);
                            Vector3 viewportPos = Camera.main.WorldToViewportPoint(spawnPos);

                            if (viewportPos.x < 0f || viewportPos.x > 1f ||
                                viewportPos.y < 0f || viewportPos.y > 1f)
                                valid = true;
                        }

                        if (valid)
                        {
                            var mob = GetFromPool(mobOpt.prefab, spawnPos, Quaternion.identity);
                            mob.tag = "Mob";

                            
                            var IAMob = mob.GetComponent<Mob>();
                            if (IAMob != null)
                            {
                                IAMob.world = this;
                            }

                            if (!chunkObjects.ContainsKey(chunk)) chunkObjects[chunk] = new List<GameObject>();
                            chunkObjects[chunk].Add(mob);
                            totalMobs++;
                        }
                    }
                }
            }

            yield return wait;
        }
    }

    public void ReassignMobChunk(GameObject mob)
    {
        Vector2Int newChunk = WorldToChunk(mob.transform.position);

        foreach (var kv in chunkObjects)
        {
            if (kv.Value.Remove(mob)) break;
        }

        if (!chunkObjects.ContainsKey(newChunk))
            chunkObjects[newChunk] = new List<GameObject>();

        chunkObjects[newChunk].Add(mob);
    }

    public void DespawnMob(GameObject mob)
    {
        foreach (var kv in chunkObjects)
        {
            kv.Value.Remove(mob);
        }
        ReturnToPool(mob);
    }

    // ✅ Persistencia de cambios en tiles
    public void ChangeTile(Vector3Int pos, TileBase newTile)
    {
        groundTilemap.SetTile(pos, null);
        waterTilemap.SetTile(pos, null);

        if (newTile != null)
        {
            if (waterTile != null && newTile == waterTile)
            {
                waterTilemap.SetTile(pos, newTile);
            }
            else
            {
                groundTilemap.SetTile(pos, newTile);
            }
        }

        if (saveSystem != null)
        {
            Vector2Int chunk = WorldToChunk(pos);
            saveSystem.SaveTileChange(pos, newTile, chunk);
        }
    }

    // ✅ Persistencia de destrucción de decoraciones
    public void NotifyDecorationDestroyed(GameObject obj)
    {
        if (obj == null) return;

        foreach (var kv in chunkObjects)
        {
            if (kv.Value.Remove(obj)) break;
        }

        if (saveSystem != null)
        {
            Vector2Int chunk = WorldToChunk(obj.transform.position);
            saveSystem.SaveDecorationChange(obj, chunk, false);
        }

        ReturnToPool(obj);
    }

    GameObject GetFromPool(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null) return null;

        if (!objectPool.TryGetValue(prefab, out var pool))
        {
            pool = new Queue<GameObject>();
            objectPool[prefab] = pool;
        }

        GameObject obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            obj.transform.SetPositionAndRotation(pos, rot);
            obj.SetActive(true);
            _container.InjectGameObject(obj);
        }
        else
        {
            obj = _container.InstantiatePrefab(prefab, pos, rot, null);
            var pr = obj.GetComponent<PrefabReference>();
            if (pr == null) pr = obj.AddComponent<PrefabReference>();
            pr.prefab = prefab;
        }
        return obj;
    }

    void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);

        var prefabRef = obj.GetComponent<PrefabReference>();
        if (prefabRef == null)
        {
            Debug.LogWarning($"Objeto {obj.name} no tiene PrefabReference, se destruye.");
            Destroy(obj);
            return;
        }

        var prefab = prefabRef.prefab;
        if (!objectPool.ContainsKey(prefab))
            objectPool[prefab] = new Queue<GameObject>();

        objectPool[prefab].Enqueue(obj);
    }

    static int ChunkSeed(int baseSeed, int x, int y)
    {
        unchecked
        {
            int h = baseSeed;
            h = h * 397 ^ x;
            h = h * 397 ^ y;
            return h;
        }
    }


}

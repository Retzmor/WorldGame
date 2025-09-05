using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    [Header("Refs")]
    public Tilemap groundTilemap;
    public Transform player;
    public BiomeLibrary biomeLibrary;

    [Header("Chunk Settings")]
    public int chunkSize = 32;
    public int viewRadiusChunks = 3;

    [Header("Noise Settings")]
    public int seed = 12345;
    public float terrainScale = 20f;
    public float moistureScale = 50f;

    [Header("Respawn Settings")]
    public float respawnInterval = 5f;
    public int maxMobsPerChunk = 10;
    public int maxMobsGlobal = 10;

    private System.Random prng;
    private float terrainOffsetX, terrainOffsetY;
    private float moistureOffsetX, moistureOffsetY;

    private readonly Dictionary<Vector2Int, bool> loadedChunks = new();
    private readonly List<Vector2Int> chunksToDraw = new();
    private readonly Dictionary<Vector2Int, List<GameObject>> chunkObjects = new();
    private readonly Dictionary<Vector2Int, BiomeDefinition> chunkBiomes = new();

    // --- Pooling ---
    private readonly Dictionary<GameObject, Queue<GameObject>> objectPool = new();

    private Coroutine loaderRoutine;
    private Coroutine respawnRoutine;

    // Buffers reutilizables
    private Vector3Int[] tilePositions;
    private TileBase[] tileBuffer;

    void Awake()
    {
        prng = new System.Random(seed);
        terrainOffsetX = prng.Next(-100000, 100000);
        terrainOffsetY = prng.Next(-100000, 100000);
        moistureOffsetX = prng.Next(-100000, 100000);
        moistureOffsetY = prng.Next(-100000, 100000);

        int count = chunkSize * chunkSize;
        tilePositions = new Vector3Int[count];
        tileBuffer = new TileBase[count];
    }

    void OnEnable()
    {
        loaderRoutine = StartCoroutine(LoaderLoop());
        respawnRoutine = StartCoroutine(RespawnLoop());
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

        chunksToDraw.Sort((a, b) =>
            (SqrDist(a, playerChunk)).CompareTo(SqrDist(b, playerChunk)));
    }

    static int SqrDist(Vector2Int a, Vector2Int b)
    {
        int dx = a.x - b.x, dy = a.y - b.y;
        return dx * dx + dy * dy;
    }

    Vector2Int WorldToChunk(Vector3 worldPos)
    {
        int wx = Mathf.FloorToInt(worldPos.x);
        int wy = Mathf.FloorToInt(worldPos.y);
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
        System.Random chunkRng = new System.Random(seed + c.x * 73856093 ^ c.y * 19349663);
        chunkObjects[c] = new List<GameObject>();

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

                var biome = biomeLibrary.GetBiome(height, moisture);
                if (biome != null)
                {
                    if (!biomeCounts.ContainsKey(biome)) biomeCounts[biome] = 0;
                    biomeCounts[biome]++;
                }

                tilePositions[i] = new Vector3Int(wx, wy, 0);
                tileBuffer[i] = (biome != null && biome.groundTile != null && biome.groundTile.Length > 0)
                    ? biome.groundTile[chunkRng.Next(0, biome.groundTile.Length)]
                    : null;

                // --- DECORACIONES ---
                if (biome != null && biome.decorations != null && biome.decorations.Length > 0)
                {
                    float roll = (float)chunkRng.NextDouble();
                    float cumulative = 0f;
                    for (int d = 0; d < biome.decorations.Length; d++)
                    {
                        var deco = biome.decorations[d];
                        if (deco.prefab == null || deco.probability <= 0f) continue;
                        cumulative += deco.probability;
                        if (roll <= cumulative)
                        {
                            var obj = GetFromPool(deco.prefab,
                                new Vector3(wx + 0.5f, wy + 0.5f, 0f), Quaternion.identity);

                            var sr = obj.GetComponent<SpriteRenderer>();
                            if (sr != null) sr.sortingOrder = -(int)(wy);

                            chunkObjects[c].Add(obj);
                            break;
                        }
                    }
                }
            }
        }
        groundTilemap.SetTiles(tilePositions, tileBuffer);

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
    }

    void UnloadChunk(Vector2Int c)
    {
        int i = 0;
        int startX = c.x * chunkSize;
        int startY = c.y * chunkSize;

        for (int y = 0; y < chunkSize; y++)
            for (int x = 0; x < chunkSize; x++, i++)
                tilePositions[i] = new Vector3Int(startX + x, startY + y, 0);

        System.Array.Clear(tileBuffer, 0, tileBuffer.Length);
        groundTilemap.SetTiles(tilePositions, tileBuffer);

        if (chunkObjects.TryGetValue(c, out var objects))
        {
            for (int j = 0; j < objects.Count; j++)
                ReturnToPool(objects[j]);

            chunkObjects.Remove(c);
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
                    if (list[i] != null && list[i].CompareTag("Mob"))
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
                if (biome == null || biome.mobs.Length == 0) continue;

                int mobCount = 0;
                var chunkList = chunkObjects[chunk];
                for (int i = 0; i < chunkList.Count; i++)
                    if (chunkList[i] != null && chunkList[i].CompareTag("Mob"))
                        mobCount++;

                if (mobCount >= maxMobsPerChunk) continue;

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

                            spawnPos = new Vector3(px + 0.5f, py + 0.5f, 0f);
                            Vector3 viewportPos = Camera.main.WorldToViewportPoint(spawnPos);

                            if (viewportPos.x < 0f || viewportPos.x > 1f ||
                                viewportPos.y < 0f || viewportPos.y > 1f)
                                valid = true;
                        }

                        if (valid)
                        {
                            var mob = GetFromPool(mobOpt.prefab, spawnPos, Quaternion.identity);
                            mob.tag = "Mob";
                            chunkList.Add(mob);
                            totalMobs++;
                        }
                    }
                }
            }
            yield return wait;
        }
    }

    // --- Object Pooling ---
    GameObject GetFromPool(GameObject prefab, Vector3 pos, Quaternion rot)
    {
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
        }
        else
        {
            obj = Instantiate(prefab, pos, rot);
        }
        return obj;
    }

    void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);

        GameObject prefab = obj; // Para simplificar: se asume que usas mismo prefab como key
        if (!objectPool.ContainsKey(prefab))
            objectPool[prefab] = new Queue<GameObject>();

        objectPool[prefab].Enqueue(obj);
    }
}

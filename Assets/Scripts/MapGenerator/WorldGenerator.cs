using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public int maxMobsPerChunk = 10;   // límite de mobs por chunk
    public int maxMobsGlobal = 10;     // límite global de mobs

    private System.Random prng;
    private float terrainOffsetX, terrainOffsetY;
    private float moistureOffsetX, moistureOffsetY;

    private readonly Dictionary<Vector2Int, bool> loadedChunks = new();
    private readonly List<Vector2Int> chunksToDraw = new();
    private readonly Dictionary<Vector2Int, List<GameObject>> chunkObjects = new();
    private readonly Dictionary<Vector2Int, BiomeDefinition> chunkBiomes = new();

    private Coroutine loaderRoutine;
    private Coroutine respawnRoutine;

    void Awake()
    {
        prng = new System.Random(seed);
        terrainOffsetX = prng.Next(-100000, 100000);
        terrainOffsetY = prng.Next(-100000, 100000);
        moistureOffsetX = prng.Next(-100000, 100000);
        moistureOffsetY = prng.Next(-100000, 100000);
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

        foreach (var c in toUnload)
        {
            UnloadChunk(c);
            loadedChunks.Remove(c);
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
        var positions = new Vector3Int[count];
        var tiles = new TileBase[count];

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

                positions[i] = new Vector3Int(wx, wy, 0);
                if (biome != null && biome.groundTile != null && biome.groundTile.Length > 0)
                {
                    tiles[i] = biome.groundTile[chunkRng.Next(0, biome.groundTile.Length)];
                }

                // --- DECORACIONES ---
                if (biome != null && biome.decorations != null && biome.decorations.Length > 0)
                {
                    var validDecorations = biome.decorations
                        .Where(d => d.prefab != null && d.probability > 0f)
                        .ToArray();

                    if (validDecorations.Length > 0)
                    {
                        float roll = (float)chunkRng.NextDouble();
                        float cumulative = 0f;

                        foreach (var deco in validDecorations)
                        {
                            cumulative += deco.probability;
                            if (roll <= cumulative)
                            {
                                var obj = Instantiate(deco.prefab,
                                    new Vector3(wx + 0.5f, wy + 0.5f, 0f), Quaternion.identity);

                                var sr = obj.GetComponent<SpriteRenderer>();
                                if (sr != null)
                                    sr.sortingOrder = -(int)(wy);

                                chunkObjects[c].Add(obj);
                                break;
                            }
                        }
                    }
                }
            }
        }
        groundTilemap.SetTiles(positions, tiles);

        if (biomeCounts.Count > 0)
        {
            dominantBiome = biomeCounts.OrderByDescending(kv => kv.Value).First().Key;
            chunkBiomes[c] = dominantBiome;
        }
    }

    void UnloadChunk(Vector2Int c)
    {
        int count = chunkSize * chunkSize;
        var positions = new Vector3Int[count];
        var tiles = new TileBase[count];

        int i = 0;
        int startX = c.x * chunkSize;
        int startY = c.y * chunkSize;

        for (int y = 0; y < chunkSize; y++)
            for (int x = 0; x < chunkSize; x++, i++)
                positions[i] = new Vector3Int(startX + x, startY + y, 0);

        groundTilemap.SetTiles(positions, tiles);

        if (chunkObjects.TryGetValue(c, out var objects))
        {
            foreach (var obj in objects)
                if (obj != null) Destroy(obj);
            chunkObjects.Remove(c);
        }

        if (chunkBiomes.ContainsKey(c))
            chunkBiomes.Remove(c);
    }

    IEnumerator RespawnLoop()
    {
        var wait = new WaitForSeconds(respawnInterval);

        while (true)
        {
            int totalMobs = chunkObjects.Values
                .SelectMany(list => list)
                .Where(o => o != null && o.CompareTag("Mob"))
                .Count();

            if (totalMobs >= maxMobsGlobal)
            {
                yield return wait;
                continue;
            }

            foreach (var kv in loadedChunks)
            {
                if (totalMobs >= maxMobsGlobal) break; // 🔹 detener si ya se alcanzó el límite global

                if (!kv.Value) continue;
                Vector2Int chunk = kv.Key;

                if (!chunkBiomes.ContainsKey(chunk)) continue;
                var biome = chunkBiomes[chunk];
                if (biome == null || biome.mobs.Length == 0) continue;

                int mobCount = chunkObjects[chunk]
                    .Where(o => o != null && o.CompareTag("Mob"))
                    .Count();

                if (mobCount >= maxMobsPerChunk) continue;

                foreach (var mobOpt in biome.mobs)
                {
                    if (totalMobs >= maxMobsGlobal) break; // 🔹 cortar aquí también

                    float roll = Random.value;
                    if (roll <= mobOpt.probability)
                    {
                        int groupSize = Random.Range(mobOpt.minGroup, mobOpt.maxGroup + 1);

                        for (int g = 0; g < groupSize; g++)
                        {
                            if (totalMobs >= maxMobsGlobal) break;

                            Vector3 spawnPos;
                            bool valid = false;
                            int attempts = 0;

                            do
                            {
                                float px = chunk.x * chunkSize + Random.Range(0, chunkSize);
                                float py = chunk.y * chunkSize + Random.Range(0, chunkSize);

                                spawnPos = new Vector3(px + 0.5f, py + 0.5f, 0f);

                                Vector3 viewportPos = Camera.main.WorldToViewportPoint(spawnPos);

                                if (viewportPos.x < 0f || viewportPos.x > 1f ||
                                    viewportPos.y < 0f || viewportPos.y > 1f)
                                {
                                    valid = true;
                                }

                                attempts++;
                            }
                            while (!valid && attempts < 10);

                            if (valid)
                            {
                                var mob = Instantiate(mobOpt.prefab, spawnPos, Quaternion.identity);
                                mob.tag = "Mob";
                                chunkObjects[chunk].Add(mob);
                                totalMobs++; // 🔹 incrementar contador global
                            }
                        }
                    }
                }
            }

            yield return wait;
        }
    }
}

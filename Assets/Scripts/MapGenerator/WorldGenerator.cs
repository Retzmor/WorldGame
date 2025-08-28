using System.Collections;
using System.Collections.Generic;
using System.Linq; // para Where()
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

    private System.Random prng;
    private float terrainOffsetX, terrainOffsetY;
    private float moistureOffsetX, moistureOffsetY;

    private readonly Dictionary<Vector2Int, bool> loadedChunks = new();
    private readonly List<Vector2Int> chunksToDraw = new();
    private readonly Dictionary<Vector2Int, List<GameObject>> chunkObjects = new();
    private Coroutine loaderRoutine;

    void Awake()
    {
        prng = new System.Random(seed);
        terrainOffsetX = prng.Next(-100000, 100000);
        terrainOffsetY = prng.Next(-100000, 100000);
        moistureOffsetX = prng.Next(-100000, 100000);
        moistureOffsetY = prng.Next(-100000, 100000);
    }

    void OnEnable() => loaderRoutine = StartCoroutine(LoaderLoop());
    void OnDisable() { if (loaderRoutine != null) StopCoroutine(loaderRoutine); }

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

        // Generador determinista para este chunk
        System.Random chunkRng = new System.Random(seed + c.x * 73856093 ^ c.y * 19349663);

        chunkObjects[c] = new List<GameObject>();

        int i = 0;
        int startX = c.x * chunkSize;
        int startY = c.y * chunkSize;

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
                positions[i] = new Vector3Int(wx, wy, 0);
                if (biome.groundTile != null && biome.groundTile.Length > 0)
                {
                    tiles[i] = biome.groundTile[chunkRng.Next(0, biome.groundTile.Length)];
                }
                else
                {
                    tiles[i] = null;
                }

                // --- DECORACIÓN CON PROBABILIDADES INDIVIDUALES ---
                if (biome.decorations != null && biome.decorations.Length > 0)
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
                                break; // solo una decoración por tile
                            }
                        }
                    }
                }
            }
        }
        groundTilemap.SetTiles(positions, tiles);
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

        // Destruir decoraciones
        if (chunkObjects.TryGetValue(c, out var objects))
        {
            foreach (var obj in objects)
                if (obj != null) Destroy(obj);
            chunkObjects.Remove(c);
        }
    }
}

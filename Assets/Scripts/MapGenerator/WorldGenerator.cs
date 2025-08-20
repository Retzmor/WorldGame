using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum BiomeType { Water, Sand, Grass, Forest, Mountain }

[System.Serializable]
public class BiomeTiles
{
    public TileBase water;
    public TileBase sand;
    public TileBase grass;
    public TileBase forest;
    public TileBase mountain;

    public TileBase Get(BiomeType b) => b switch
    {
        BiomeType.Water => water,
        BiomeType.Sand => sand,
        BiomeType.Grass => grass,
        BiomeType.Forest => forest,
        _ => mountain
    };
}

public class WorldGenerator : MonoBehaviour
{
    [Header("Refs")]
    public Tilemap groundTilemap;
    public Transform player; // quién centra la carga de chunks (puede ser la cámara)

    [Header("Tiles")]
    public BiomeTiles biomeTiles;

    [Header("Chunking")]
    [Tooltip("Tamaño de cada chunk en tiles (usa potencias de 2 para orden).")]
    public int chunkSize = 32;
    [Tooltip("Radio de carga en chunks alrededor del jugador.")]
    public int viewRadiusChunks = 3;

    [Header("Seed & Noise")]
    public int seed = 12345;
    public string seedInput = "";
    [Tooltip("Escala del relieve/altura (ruido). Valores mayores -> cambios más suaves.")]
    public float terrainScale = 20f;
    [Tooltip("Escala del mapa de biomas (grande para zonas amplias).")]
    public float biomeScale = 100f;
    [Tooltip("Octavas para dar detalle al terreno (0 = solo Perlin básico).")]
    public int octaves = 3;
    [Range(0f, 1f)] public float persistence = 0.5f;
    [Range(1f, 4f)] public float lacunarity = 2f;

    [Header("Umbrales biomas")]
    [Range(0f, 1f)] public float waterThreshold = 0.30f;
    [Range(0f, 1f)] public float sandThreshold = 0.38f;   // entre agua y pasto
    [Range(0f, 1f)] public float forestThreshold = 0.70f; // sobre pasto
    [Range(0f, 1f)] public float mountainThreshold = 0.82f;

    // Estado
    private System.Random prng;
    private float terrainOffsetX, terrainOffsetY;
    private float biomeOffsetX, biomeOffsetY;

    private readonly Dictionary<Vector2Int, bool> loadedChunks = new();
    private readonly List<Vector2Int> chunksToDraw = new();
    private Coroutine loaderRoutine;

    void Awake()
    {
        if (!groundTilemap) groundTilemap = GetComponent<Tilemap>();
        if (!player) player = Camera.main ? Camera.main.transform : transform;

        // --- elegir semilla ---
        int seedValue;
        if (!string.IsNullOrEmpty(seedInput))
        {
            if (int.TryParse(seedInput, out int parsed))
                seedValue = parsed;               // si seedInput es número
            else
                seedValue = seedInput.GetHashCode(); // si seedInput es texto
        }
        else
        {
            seedValue = seed; // fallback al int público
        }

        prng = new System.Random(seedValue);
        terrainOffsetX = prng.Next(-100000, 100000);
        terrainOffsetY = prng.Next(-100000, 100000);
        biomeOffsetX = prng.Next(-100000, 100000);
        biomeOffsetY = prng.Next(-100000, 100000);
    }

    void OnEnable()
    {
        loaderRoutine = StartCoroutine(LoaderLoop());
    }

    void OnDisable()
    {
        if (loaderRoutine != null) StopCoroutine(loaderRoutine);
    }

    IEnumerator LoaderLoop()
    {
        var wait = new WaitForSeconds(0.1f); // limita trabajo por segundo
        while (true)
        {
            UpdateVisibleChunks();
            // Genera como mucho N chunks por iteración para evitar stutters
            int budget = 1;
            while (chunksToDraw.Count > 0 && budget-- > 0)
            {
                var c = chunksToDraw[0];
                chunksToDraw.RemoveAt(0);
                GenerateChunk(c);
                loadedChunks[c] = true;
                // cede un frame si el chunk es grande
                if (chunkSize >= 48) yield return null;
            }
            yield return wait;
        }
    }

    void UpdateVisibleChunks()
    {
        Vector2Int playerChunk = WorldToChunk(player.position);

        // Marcar deseados
        HashSet<Vector2Int> desired = new();
        for (int cy = -viewRadiusChunks; cy <= viewRadiusChunks; cy++)
        {
            for (int cx = -viewRadiusChunks; cx <= viewRadiusChunks; cx++)
            {
                var c = new Vector2Int(playerChunk.x + cx, playerChunk.y + cy);
                desired.Add(c);
                if (!loadedChunks.ContainsKey(c))
                {
                    loadedChunks[c] = false; // marcado pero no generado
                    chunksToDraw.Add(c);
                }
            }
        }

        // Descargar los que ya no están en radio
        // (opcional: comentar si prefieres nunca limpiar)
        List<Vector2Int> toUnload = new();
        foreach (var kv in loadedChunks)
            if (kv.Value && !desired.Contains(kv.Key))
                toUnload.Add(kv.Key);

        foreach (var c in toUnload)
        {
            UnloadChunk(c);
            loadedChunks.Remove(c);
        }

        // Ordena la cola por distancia (carga “en espiral”)
        chunksToDraw.Sort((a, b) =>
            (SqrDist(a, playerChunk)).CompareTo(SqrDist(b, playerChunk)));
    }

    static int SqrDist(Vector2Int a, Vector2Int b)
    {
        int dx = a.x - b.x, dy = a.y - b.y; return dx * dx + dy * dy;
    }

    Vector2Int WorldToChunk(Vector3 worldPos)
    {
        int wx = Mathf.FloorToInt(worldPos.x);
        int wy = Mathf.FloorToInt(worldPos.y);
        // división tipo floor para negativos
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

        int i = 0;
        int startX = c.x * chunkSize;
        int startY = c.y * chunkSize;

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++, i++)
            {
                int wx = startX + x;
                int wy = startY + y;

                float h = FractalNoise(wx, wy, terrainScale, terrainOffsetX, terrainOffsetY);
                float b = Mathf.PerlinNoise(
                    (wx + biomeOffsetX) / biomeScale,
                    (wy + biomeOffsetY) / biomeScale
                );

                BiomeType biome = ClassifyBiome(h, b);

                positions[i] = new Vector3Int(wx, wy, 0);
                tiles[i] = biomeTiles.Get(biome);
            }
        }

        groundTilemap.SetTiles(positions, tiles);
    }

    void UnloadChunk(Vector2Int c)
    {
        int count = chunkSize * chunkSize;
        var positions = new Vector3Int[count];
        var tiles = new TileBase[count]; // nullls

        int i = 0;
        int startX = c.x * chunkSize;
        int startY = c.y * chunkSize;

        for (int y = 0; y < chunkSize; y++)
            for (int x = 0; x < chunkSize; x++, i++)
                positions[i] = new Vector3Int(startX + x, startY + y, 0);

        groundTilemap.SetTiles(positions, tiles); // borra
    }

    float FractalNoise(int wx, int wy, float scale, float offx, float offy)
    {
        if (octaves <= 0)
        {
            return Mathf.PerlinNoise((wx + offx) / scale, (wy + offy) / scale);
        }

        float amp = 1f;
        float freq = 1f;
        float value = 0f;
        float norm = 0f;

        for (int o = 0; o < octaves; o++)
        {
            float nx = ((wx + offx) / scale) * freq;
            float ny = ((wy + offy) / scale) * freq;
            float v = Mathf.PerlinNoise(nx, ny);
            value += v * amp;
            norm += amp;
            amp *= persistence;
            freq *= lacunarity;
        }
        return value / Mathf.Max(0.0001f, norm);
    }

    BiomeType ClassifyBiome(float height, float biomeNoise)
    {
        // Primero agua/arena por altura; luego pasto/bosque/montaña por altura y ruido de bioma
        if (height < waterThreshold) return BiomeType.Water;
        if (height < sandThreshold) return BiomeType.Sand;

        // sobre pasto, usamos el ruido de bioma para variar bosque
        if (height > mountainThreshold) return BiomeType.Mountain;
        if (height > forestThreshold && biomeNoise > 0.55f) return BiomeType.Forest;

        return BiomeType.Grass;
    }
}

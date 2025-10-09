using UnityEngine;
using System;
using System.IO;
using Random = UnityEngine.Random;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public WorldSaveSystem saveSystem; // referencia al sistema de guardado
    private string metaFile => Path.Combine(Application.persistentDataPath, "worlds.json");
    public static event Action OnCreateNewWorld;



    [System.Serializable]
    public class WorldMeta
    {
        public string name;
        public string seed;
        public string mode;
        public string difficult;
        public string version;
        public string createdAt;
        public int slot;
    }

    [System.Serializable]
    public class WorldMetaList
    {
        public System.Collections.Generic.List<WorldMeta> worlds = new();
    }
    private WorldMetaList metaList = new ();


    public void InitWorld()
    {
        OnCreateNewWorld?.Invoke();
        Debug.Log("perra");
    }

    // Crear un mundo y añadirlo a worlds.json
    public void CreateNewWorld(string seed, string name  = "New World" , GameMode mode = GameMode.Survival, Difficult difficult = Difficult.normal , string createdAt = null)
    {
        if (string.IsNullOrEmpty(createdAt))
            createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // slot automático (ej: mundo 1, mundo 2...)
        int slot = metaList.worlds.Count + 1;

        if(seed == string.Empty)
        {
            seed = Random.Range(0, int.MaxValue).ToString();
        }

        // guardar metadatos
        var meta = new WorldMeta
        {
            name = name,
            seed = seed,
            mode = mode.ToString(),
            difficult = difficult.ToString(),
            createdAt = createdAt,
            version = Application.version.ToString(),
            slot = slot
        };

        metaList.worlds.Add(meta);
        SaveMetaData();

        // asignar slot en el saveSystem y crear archivo del mundo
        saveSystem.currentSlot = slot;
        saveSystem.SaveWorld();

        Debug.Log($"🌍 Mundo creado: {name} (slot {slot})");
    }

    // Guardar worlds.json
    private void SaveMetaData()
    {
        string json = JsonUtility.ToJson(metaList, true);
        File.WriteAllText(metaFile, json);
    }

    // Cargar worlds.json
    public WorldMetaList LoadMetaData()
    {
        if (File.Exists(metaFile))
        {
            string json = File.ReadAllText(metaFile);
            return metaList = JsonUtility.FromJson<WorldMetaList>(json);
        }
        else
        {
            return  new WorldMetaList();
        }
    }

    public System.Collections.Generic.List<WorldMeta> GetWorlds()
    {
        return metaList.worlds;
    }


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (saveSystem == null)
        {
            saveSystem = gameObject.AddComponent<WorldSaveSystem>();
        }
    }


    public void QuitGame()
    {
        Application.Quit();
    }
}

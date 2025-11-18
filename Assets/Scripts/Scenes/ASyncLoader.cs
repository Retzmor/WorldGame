using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ASyncLoader : MonoBehaviour
{
    public static ASyncLoader Instance;

    [Header("MenuScreens")]
    [SerializeField] private GameObject LoadingScreen;
    [SerializeField] private GameObject MainMenu;

    [Header("Slider")]
    [SerializeField] private Slider loadingSlider;

    private Action loadWorldAction;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        if (GameObject.Find("MainMenu") != null)
        {
            MainMenu = GameObject.Find("MainMenu");
        }

        loadWorldAction = () => LoadLevelBtn("ScenaGym");
        GameManager.OnLoadWorld += loadWorldAction;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {

        GameManager.OnLoadWorld -= loadWorldAction;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadLevelBtn(string levelLoad)
    {
        Time.timeScale = 1f;
        MainMenu = GameObject.Find("CanvasMainMenu");
        MainMenu?.SetActive(false);
        
        LoadingScreen?.SetActive(true);
        StartCoroutine(LoadLevelAsync(levelLoad));
    }

    IEnumerator LoadLevelAsync(string levelToLoad)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        loadOperation.allowSceneActivation = false;

        // 🔹 Progreso durante la carga de la escena
        while (loadOperation.progress < 0.9f)
        {
            loadingSlider.value = Mathf.Clamp01(loadOperation.progress / 0.9f * 0.9f); // llega hasta 90%
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        loadOperation.allowSceneActivation = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "ScenaGym")
            return;

        StartCoroutine(WaitAndGenerateWorld());
    }

    IEnumerator WaitAndGenerateWorld()
    {
        yield return new WaitForSeconds(0.1f);

        WorldGenerator worldGen = FindAnyObjectByType<WorldGenerator>();
        if (worldGen == null)
        {
            Debug.LogError("❌ No se encontró WorldGenerator en la escena.");
            yield break;
        }

        bool worldReady = false;
        WorldGenerator.OnWorldGenerated += () => worldReady = true;

        worldGen.StartWorldGeneration();

        // 🔹 Mientras el mundo se genera, actualizamos el slider con progreso real
        while (!worldReady)
        {
            if (worldGen.GenerationProgress >= 0f) // variable float en tu generador
            {
                // Asume que GenerationProgress va de 0 a 1
                loadingSlider.value = 0.9f + (worldGen.GenerationProgress * 0.1f);
            }
            yield return null;
        }

        loadingSlider.value = 1f;
        Debug.Log("✅ Mundo generado correctamente.");

        yield return new WaitForSeconds(0.3f);
        LoadingScreen.SetActive(false);
    }
}

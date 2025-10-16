using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class ButtonSlot : MonoBehaviour
{

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text creationDateAndDifficult;
    [SerializeField] private TMP_Text versionAndGameMode;
    private WorldMeta gameData;
    public Shadow shadow;
    [SerializeField] private ShowWorldsController worldsController;

    // ⏱ Variables for double click
    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.25f; // seconds
    public void SetData(WorldMeta data) 
    {
        gameData = data;
        nameText.text = data.name;
        creationDateAndDifficult.text = $" {data.difficult}, {data.createdAt}";
        versionAndGameMode.text = $"GameMode {data.mode}, version {data.version}";
    }

    public void SlotSelected()
    {

        if(shadow == null)
        {
            Debug.Log("SlotWithoutShadow");
        }
        else
        {
            worldsController.DeselectButtons();
            shadow.enabled = true;

            WorldSaveSystem worldSave = GameObject.FindAnyObjectByType<WorldSaveSystem>();
            worldSave.ChangeCurrentData(gameData);
            // activar botton de jugar el mundo
            //activar el boton de editar mundo,
            //activar boton de volver al mundo
            worldsController.ActivateButtonOfTheWorld();

        }
    }

    public void SlotDeselect()
    {
        shadow.enabled = false;
    }



    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
        worldsController = GameObject.FindAnyObjectByType<ShowWorldsController>();
    }
    private void OnDisable()
    {
        SlotDeselect();
    }

    private void OnClick()
    {
        float timeSinceLastClick = Time.time - lastClickTime;
        lastClickTime = Time.time;
        SlotSelected();

        if (timeSinceLastClick <= doubleClickThreshold)
        {

            //WorldSaveSystem worldSave = GameObject.FindAnyObjectByType<WorldSaveSystem>();
            //worldSave.ChangeCurrentData(gameData);

            //todo init the world
            GameManager.instance.LoadWorld();

        }

    }

}

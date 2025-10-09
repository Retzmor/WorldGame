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

    public void SetData(WorldMeta data) 
    {
        gameData = data;
        nameText.text = data.name;
        creationDateAndDifficult.text = $" {data.difficult}, {data.createdAt}";
        versionAndGameMode.text = $"GameMode {data.mode}, version {data.version}";
    }

    public void SlotSelected()
    {
        GameObject.FindAnyObjectByType<WorldSaveSystem>().currentData = gameData;

        if(shadow == null)
        {
            Debug.Log("SlotWithoutShadow");
        }
        else
        {
            worldsController.DeselectButtons();
            shadow.enabled = true;
        }
    }

    public void SlotDeselect()
    {
        shadow.enabled = false;
    }



    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(SlotSelected);
        worldsController = GameObject.FindAnyObjectByType<ShowWorldsController>();
    }

}

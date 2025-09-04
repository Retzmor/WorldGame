using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public GameObject inventoryPanel;
    public GameObject hotbarPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool CanMoveBetweenPanels(GameObject item, GameObject targetPanel)
    {
        return true;
    }

    public void RefreshLayouts()
    {
        Canvas.ForceUpdateCanvases();
    }
}

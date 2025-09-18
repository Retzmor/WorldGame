using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject inventoryPanel;

    private bool isInventoryOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            isInventoryOpen = !isInventoryOpen;

            inventoryPanel.SetActive(isInventoryOpen);
        }
    }
}

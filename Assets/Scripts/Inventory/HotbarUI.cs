using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    [SerializeField] private Inventory inventory;   
    [SerializeField] private Button[] hotbarSlots;
    private int currentSlot = 0;

    private void Update()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                SelectSlot(i);
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            UseCurrentItem();
        }
    }

    private void SelectSlot(int slotIndex)
    {
        currentSlot = slotIndex;
        Debug.Log("Slot seleccionado: " + (slotIndex + 1));
    }

    private void UseCurrentItem()
    {
        Button slotButton = hotbarSlots[currentSlot];
        if (slotButton.transform.childCount > 0)
        {
            slotButton.onClick.Invoke(); 
            Debug.Log("Usaste el item en slot " + (currentSlot + 1));
        }
    }
}

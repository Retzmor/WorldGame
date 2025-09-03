using UnityEngine;

public class InventoryButtons : MonoBehaviour
{
    private Inventory inventory;

    void Awake()
    {
        inventory = Object.FindFirstObjectByType<Inventory>();
    }

    public void UseItem()
    {
       // inventory.UseInventoryItems(gameObject.name);
    }
}

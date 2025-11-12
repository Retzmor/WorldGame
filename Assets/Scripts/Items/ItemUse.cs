using TMPro;
using UnityEngine;
using Zenject;

public class ItemUse : MonoBehaviour
{
    [Inject] Inventory inventory;
    [SerializeField] int healthToGive = 20;
    public GameObject itemPrefab;
    public ItemData itemData;
    [SerializeField] public GameObject worldPrefap;
    public string itemName;
    private int healAmount;
    public void SetItem(string name, int heal, GameObject worldPrefab)
    {
        itemName = name;
        healAmount = heal;
        this.worldPrefap = worldPrefab;
    }

    public GameObject GetWorldPrefab()
    {
        return worldPrefap;
    }

    public void UseButton()
    {
        if(gameObject.name.Contains("Potion"))
        {
            HealthPlayer player = FindAnyObjectByType<HealthPlayer>();
            if (player != null)
                player.HealHealth(healthToGive);
            inventory.InventoryItems[itemName]--;
            TextMeshProUGUI text = GetComponentInChildren<TextMeshProUGUI>();
            text.text = inventory.InventoryItems[itemName].ToString();
            if (inventory.InventoryItems[itemName] <= 0)
            {
                inventory.InventoryItems.Remove(itemName);
                Destroy(gameObject);
            }
        }
    }
 }



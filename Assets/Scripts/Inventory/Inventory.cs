using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour
{
    [SerializeField] GameObject[] slots;
    [SerializeField] GameObject[] hotbarSlots;
    TextMeshProUGUI text;

    public Dictionary<string, int> InventoryItems = new Dictionary<string, int>();

    public void AddItem(GameObject itemPrefab, string itemName, int amount, Sprite icon, int healAmount)
    {
        if (!AddItemToHotbar(itemPrefab, icon, itemName, healAmount, amount))
        {
            AddItemToInventory(itemPrefab, itemName, amount, icon);
        }
    }

    private void AddItemToInventory(GameObject itemToAdd, string itemName, int itemAmount, Sprite icon)
    {
        bool itemPlaced = false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].transform.childCount > 0 &&
                slots[i].transform.GetChild(0).name == itemName)
            {
                InventoryItems[itemName] += itemAmount;
                TextMeshProUGUI text = slots[i].GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = InventoryItems[itemName].ToString();
                itemPlaced = true;
                break;
            }
        }

        if (!itemPlaced)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].transform.childCount == 0)
                {
                    GameObject item = Instantiate(itemToAdd, slots[i].transform);
                    item.transform.localPosition = Vector3.zero;
                    item.name = itemName;

                    InventoryItems[itemName] = itemAmount;

                    TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                        text.text = InventoryItems[itemName].ToString();

                    break;
                }
            }
        }
    }

    private bool AddItemToHotbar(GameObject itemPrefab, Sprite itemSprite, string itemName, int healAmount, int amount)
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i].transform.childCount > 0)
            {
                Transform existingItem = hotbarSlots[i].transform.GetChild(0);
                string existingName = existingItem.name.Replace("(Clone)", "");
                if (existingName == itemName)
                {
                    InventoryItems[itemName] += amount;
                    TextMeshProUGUI text = existingItem.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                        text.text = InventoryItems[itemName].ToString();
                    return true;
                }
            }
        }

        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i].transform.childCount == 0)
            {
                GameObject itemButton = Instantiate(itemPrefab, hotbarSlots[i].transform);
                itemButton.transform.localPosition = Vector3.zero;
                itemButton.transform.localScale = Vector3.one;
                itemButton.name = itemName;

                Image img = itemButton.GetComponent<Image>();
                if (img != null && itemSprite != null)
                    img.sprite = itemSprite;

                ItemUse itemUse = itemButton.GetComponent<ItemUse>();
                if (itemUse != null)
                    itemUse.SetItem(itemName, healAmount);

                InventoryItems[itemName] = amount;

                TextMeshProUGUI text = itemButton.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = InventoryItems[itemName].ToString();

                return true;
            }
        }

        return false;
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour
{
    [SerializeField] GameObject[] slots;
    [SerializeField] GameObject[] backPack;
    [SerializeField] GameObject[] hotbarSlots;
    TextMeshProUGUI text;

    bool isInstantiated;

    public Dictionary<string, int> InventoryItems = new Dictionary<string, int>();

    public void CheckSlotsAvailability(GameObject itemToAdd, string itemName, int itemAmount)
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

                    slots[i].GetComponent<SlotsScripts>().isUsed = true;
                    break;
                }
            }
        }
        Sprite itemSprite = itemToAdd.GetComponent<Image>()?.sprite;
        AddItemToHotbar(itemToAdd, itemSprite, itemName, 20); 
    }

    public void UseInventoryItems(string itemName)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].transform.childCount > 0 &&
                slots[i].transform.GetChild(0).gameObject.name == itemName)
            {
                InventoryItems[itemName]--;

                TextMeshProUGUI text = slots[i].GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = InventoryItems[itemName].ToString();

                for (int j = 0; j < hotbarSlots.Length; j++)
                {
                    if (hotbarSlots[j].transform.childCount > 0)
                    {
                        Transform hotbarItem = hotbarSlots[j].transform.GetChild(0);
                        if (hotbarItem.name == itemName)
                        {
                            TextMeshProUGUI hotbarText = hotbarItem.GetComponentInChildren<TextMeshProUGUI>();
                            if (hotbarText != null)
                            {
                                if (InventoryItems[itemName] > 0)
                                    hotbarText.text = InventoryItems[itemName].ToString();
                                else
                                    hotbarText.text = ""; 
                            }
                            break;
                        }
                    }
                }

                if (InventoryItems[itemName] <= 0)
                {
                    Destroy(slots[i].transform.GetChild(0).gameObject);
                    slots[i].GetComponent<SlotsScripts>().isUsed = false;
                    InventoryItems.Remove(itemName);

                    for (int j = 0; j < hotbarSlots.Length; j++)
                    {
                        if (hotbarSlots[j].transform.childCount > 0 &&
                            hotbarSlots[j].transform.GetChild(0).name == itemName)
                        {
                            Destroy(hotbarSlots[j].transform.GetChild(0).gameObject);
                            break;
                        }
                    }
                }
                break;
            }
        }
    }

    public void AddItemToHotbar(GameObject itemPrefab, Sprite itemSprite, string itemName, int healAmount)
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i].transform.childCount > 0)
            {
                Transform existingItem = hotbarSlots[i].transform.GetChild(0);
                string existingName = existingItem.name.Replace("(Clone)", "");
                if (existingName == itemName)
                {
                    TextMeshProUGUI text = existingItem.GetComponentInChildren<TextMeshProUGUI>();
                    if (InventoryItems.ContainsKey(itemName))
                        text.text = InventoryItems[itemName].ToString();
                    return; 
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

                TextMeshProUGUI text = itemButton.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null && InventoryItems.ContainsKey(itemName))
                    text.text = InventoryItems[itemName].ToString();
                break; 
            }
        }
    }
}

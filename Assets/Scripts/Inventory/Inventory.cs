using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Inventory : MonoBehaviour
{
    [SerializeField] GameObject[] slots;
    [SerializeField] GameObject[] backPack;
    TextMeshProUGUI text;

    bool isInstantiated;

    public Dictionary<string, int> InventoryItems = new Dictionary<string, int>();

    public void CheckSlotsAvailability(GameObject itemToAdd, string itemName, int itemAmount)
    {
        isInstantiated = false;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].transform.childCount > 0)
            {
                slots[i].GetComponent<SlotsScripts>().isUsed = true;
            }

            else if (!isInstantiated && !slots[i].GetComponent<SlotsScripts>().isUsed)
            {
                if (!InventoryItems.ContainsKey(itemName))
                {
                    GameObject item = Instantiate(itemToAdd, slots[i].transform.position, Quaternion.identity);
                    item.transform.SetParent(slots[i].transform, false);
                    item.transform.localPosition = Vector3.zero;
                    item.name = itemName.Replace("Clone", "");
                    isInstantiated = true;
                    slots[i].GetComponent<SlotsScripts>().isUsed = true;
                    InventoryItems.Add(itemName, itemAmount);
                    text = slots[i].GetComponentInChildren<TextMeshProUGUI>();
                    text.text = itemName.ToString();
                    break;
                }

                else
                {
                    for(int j = 0; j < slots.Length; j++)
                    {
                        InventoryItems[itemName] += itemAmount;
                        text = slots[j].GetComponentInChildren<TextMeshProUGUI>();
                        text.text = InventoryItems[itemName].ToString();
                        break;
                    }

                    break;
                }
            }
        }
    }

    public void UseInvetoryItems(string itemName)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            text = slots[i].GetComponentInChildren<TextMeshProUGUI>();
            if (slots[i].transform.GetChild(0).gameObject.name == itemName)
            {
                InventoryItems[itemName]--;
                text.text = InventoryItems[itemName].ToString();

                if (InventoryItems[itemName] <= 0)
                {
                    Destroy(slots[i].transform.GetChild(0).gameObject);
                    slots[i].GetComponent<SlotsScripts>().isUsed = false;
                    InventoryItems.Remove(itemName);
                }

                break;
            }
        }
    }
}

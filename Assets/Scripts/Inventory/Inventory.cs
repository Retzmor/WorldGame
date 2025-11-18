using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class Inventory : MonoBehaviour
{
    [Inject] private DiContainer _container;
    [SerializeField] GameObject[] slots;
    [SerializeField] GameObject[] hotbarSlots;
    [SerializeField] HotbarController hotbar;
    public Dictionary<string, int> InventoryItems = new Dictionary<string, int>();
    [SerializeField] private List<ItemData> itemDatabase; // asigna en inspector

    public void AddItem(GameObject itemPrefab, string itemName, int amount, Sprite icon, int healAmount, GameObject worldPrefab)
    {
        if (!AddItemToHotbar(itemPrefab, icon, itemName, healAmount, amount, worldPrefab))
        {
            AddItemToInventory(itemPrefab, itemName, amount, icon);
            //aca tambien se puede hacer lo del inventario
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
    private bool AddItemToHotbar(GameObject itemPrefab, Sprite itemSprite, string itemName, int healAmount, int amount, GameObject worldPrefab)
    {
        int selectedIndex = hotbar != null ? hotbar.CurrentSlotIndex : -1;
        if (selectedIndex >= 0 && selectedIndex < hotbarSlots.Length &&
            hotbarSlots[selectedIndex].transform.childCount == 0)
        {
            GameObject itemButton = _container.InstantiatePrefab(itemPrefab, hotbarSlots[selectedIndex].transform);
            itemButton.transform.localPosition = Vector3.zero;
            itemButton.transform.localScale = Vector3.one;
            itemButton.name = itemName;

            Image img = itemButton.GetComponent<Image>();
            if (img != null && itemSprite != null)
                img.sprite = itemSprite;

            ItemUse itemUse = itemButton.GetComponent<ItemUse>();
            if (itemUse != null)
                itemUse.SetItem(itemName, healAmount, worldPrefab);

            InventoryItems[itemName] = amount;

            TextMeshProUGUI text = itemButton.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = InventoryItems[itemName].ToString();

            return true;
        }
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
                GameObject itemButton = _container.InstantiatePrefab(itemPrefab, hotbarSlots[i].transform);

                //ACA SE DEFINE EN QUE HOTBAR SE COLOCA UN OBJETO EN EL INVENTARIO


                itemButton.transform.localPosition = Vector3.zero;
                itemButton.transform.localScale = Vector3.one;
                itemButton.name = itemName;

                Image img = itemButton.GetComponent<Image>();
                if (img != null && itemSprite != null)
                    img.sprite = itemSprite;

                ItemUse itemUse = itemButton.GetComponent<ItemUse>();
                if (itemUse != null)
                    itemUse.SetItem(itemName, healAmount, worldPrefab);

                InventoryItems[itemName] = amount;

                TextMeshProUGUI text = itemButton.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = InventoryItems[itemName].ToString();

                return true;
            }
        }
        return false;
    }

    public List<WorldSaveSystem.ItemSave> GetInventoryForSave()
    {
        List<WorldSaveSystem.ItemSave> saveList = new();
        Debug.Log("Llega al metodo de guardar inventario");

        // HOTBAR
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i].transform.childCount > 0)
            {
                Transform item = hotbarSlots[i].transform.GetChild(0);
                string itemID = item.name.Replace("(Clone)", "").Trim();

                int quantity = 1;
                TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null && int.TryParse(text.text, out int q))
                    quantity = q;

                saveList.Add(new WorldSaveSystem.ItemSave
                {
                    itemID = itemID,
                    quantity = quantity,
                    durability = 100,
                    equipped = true,
                    slotIndex = i // ✅ guarda índice exacto del slot
                });
            }
        }

        // INVENTARIO
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].transform.childCount > 0)
            {
                Transform item = slots[i].transform.GetChild(0);
                string itemID = item.name.Replace("(Clone)", "").Trim();

                int quantity = 1;
                TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null && int.TryParse(text.text, out int q))
                    quantity = q;

                saveList.Add(new WorldSaveSystem.ItemSave
                {
                    itemID = itemID,
                    quantity = quantity,
                    durability = 100,
                    equipped = false,
                    slotIndex = i // ✅ guarda índice exacto del slot
                });
            }
        }

        return saveList;
    }


    public void LoadInventory(List<WorldSaveSystem.ItemSave> savedItems)
    {
        if (savedItems == null || savedItems.Count == 0)
        {
            Debug.Log("No hay ítems para cargar.");
            return;
        }

        // Limpia inventario actual
        foreach (var slot in slots)
            if (slot.transform.childCount > 0)
                Destroy(slot.transform.GetChild(0).gameObject);

        foreach (var slot in hotbarSlots)
            if (slot.transform.childCount > 0)
                Destroy(slot.transform.GetChild(0).gameObject);

        InventoryItems.Clear();

        foreach (var itemSave in savedItems)
        {
            ItemData data = itemDatabase.Find(i => i.itemName == itemSave.itemID);
            if (data == null)
            {
                Debug.LogWarning($"No se encontró ItemData para '{itemSave.itemID}'");
                continue;
            }

            GameObject parentSlot = null;

            // 🔢 coloca en el mismo índice guardado
            if (itemSave.equipped)
            {
                if (itemSave.slotIndex >= 0 && itemSave.slotIndex < hotbarSlots.Length)
                    parentSlot = hotbarSlots[itemSave.slotIndex];
            }
            else
            {
                if (itemSave.slotIndex >= 0 && itemSave.slotIndex < slots.Length)
                    parentSlot = slots[itemSave.slotIndex];
            }

            if (parentSlot == null)
            {
                Debug.LogWarning($"Slot inválido para '{itemSave.itemID}' (index {itemSave.slotIndex})");
                continue;
            }

            // Instancia el ítem
            GameObject itemObj = Instantiate(data.prefab, parentSlot.transform);
            itemObj.name = data.itemName;
            itemObj.transform.localPosition = Vector3.zero;
            itemObj.transform.localScale = Vector3.one;

            // Sprite
            Image img = itemObj.GetComponent<Image>();
            if (img != null && data.itemSprite != null)
                img.sprite = data.itemSprite;

            // Comportamiento
            ItemUse itemUse = itemObj.GetComponent<ItemUse>();
            if (itemUse != null)
                itemUse.SetItem(data.itemName, data.healAmount, data.worldPrefab);

            // Cantidad
            InventoryItems[data.itemName] = itemSave.quantity;
            TextMeshProUGUI text = itemObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = itemSave.quantity.ToString();
        }

        Debug.Log($"Inventario cargado con {InventoryItems.Count} ítems restaurados en sus slots exactos.");
    }

}
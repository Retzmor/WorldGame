using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;
public class HotbarController : MonoBehaviour
{
    [SerializeField] private GameObject[] hotbarSlots;
    [SerializeField] private AttackPlayer playerAttack;
    [SerializeField] private InputActionReference dropAction;

    [Inject] Inventory inventory;
    [Inject] private DiContainer _container;
    private int currentSlotIndex = -1;
    public int CurrentSlotIndex => currentSlotIndex;
    private void OnEnable()
    {
        if (dropAction != null)
        {
            dropAction.action.performed += OnDrop;
            dropAction.action.Enable();
        }
    }
    private void OnDisable()
    {
        if (dropAction != null)
        {
            dropAction.action.performed -= OnDrop;
            dropAction.action.Disable();
        }
    }
    private void OnDrop(InputAction.CallbackContext ctx)
    {
        DropOneItemFromHand();
    }
    private void Update()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
    }
    private void DropOneItemFromHand()
    {
        GameObject selectedItem = GetSelectedItem();
        if (selectedItem == null)
        {
            return;
        }

        ItemUse itemData = selectedItem.GetComponent<ItemUse>();
        if (itemData == null) return;

        string itemName = itemData.itemName;
        if (!inventory.InventoryItems.ContainsKey(itemName)) return;

        inventory.InventoryItems[itemName]--;

        if (itemData.worldPrefap != null)
        {
            Vector3 dropPos = playerAttack.transform.position + playerAttack.transform.right * 1f;
            _container.InstantiatePrefab(itemData.worldPrefap, dropPos, Quaternion.identity, null);
            Debug.Log("Instancie el objeto");
        }

        if (inventory.InventoryItems[itemName] <= 0)
        {
            inventory.InventoryItems.Remove(itemName);
            Destroy(selectedItem);

            if (playerAttack.currentWeapon != null)
            {
                Destroy(playerAttack.currentWeapon);
                playerAttack.currentWeapon = null;
            }
        }
        else
        {
            TextMeshProUGUI text = selectedItem.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = inventory.InventoryItems[itemName].ToString();
        }
    }
    private void SelectSlot(int index)
    {
        currentSlotIndex = index;
        GameObject selectedUI = GetSelectedItem();
        UpdateSlotHighlights();
        if (selectedUI != null && playerAttack != null)
        {
            ItemUse itemData = selectedUI.GetComponent<ItemUse>();
            if (playerAttack.currentWeapon != null)
                Destroy(playerAttack.currentWeapon);
            if (itemData != null && itemData.itemPrefab != null)
            {
                if (playerAttack.currentWeapon != null)
                Destroy(playerAttack.currentWeapon);
                GameObject newWeapon = Instantiate(itemData.itemPrefab, playerAttack.pivotRight);
                playerAttack.currentWeapon = newWeapon;
                playerAttack.currentWeapon = newWeapon;
                playerAttack.MoveWeaponToHand(playerAttack.pivotRight);
            }
        }
        else if (playerAttack != null)
        {
            if (playerAttack.currentWeapon != null)
                Destroy(playerAttack.currentWeapon);

            playerAttack.currentWeapon = null;
        }
    }

    public GameObject GetSelectedItem()
    {
        if (currentSlotIndex >= 0 && currentSlotIndex < hotbarSlots.Length &&
            hotbarSlots[currentSlotIndex].transform.childCount > 0)
        {
            return hotbarSlots[currentSlotIndex].transform.GetChild(0).gameObject;
        }
        return null;
    }

    private void UpdateSlotHighlights()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            Image slotImage = hotbarSlots[i].GetComponent<Image>();
            if (slotImage != null)
            {
                if (i == currentSlotIndex)
                    slotImage.color = Color.yellow; 
                else
                    slotImage.color = Color.white;  
            }
        }
    }
    
}

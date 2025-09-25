using UnityEngine;
using UnityEngine.UI;

public class HotbarController : MonoBehaviour
{
    [SerializeField] private GameObject[] hotbarSlots;
    [SerializeField] private AttackPlayer playerAttack; 

    private int currentSlotIndex = -1;

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

    private void SelectSlot(int index)
    {
        currentSlotIndex = index;

        GameObject selectedUI = GetSelectedItem();
        if (selectedUI != null && playerAttack != null)
        {
            ItemUse itemData = selectedUI.GetComponent<ItemUse>();
            if (itemData != null && itemData.itemPrefab != null)
            {
                if (playerAttack.currentWeapon != null)
                    Destroy(playerAttack.currentWeapon);

                GameObject newWeapon = Instantiate(itemData.itemPrefab, playerAttack.pivotRight);
                playerAttack.currentWeapon = newWeapon;
                // worldPrefab.transform.SetParent(attack.transform);
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
}

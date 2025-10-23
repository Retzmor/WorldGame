using NUnit.Framework.Interfaces;
using UnityEditor.SceneManagement;
using UnityEngine;
using Zenject;

public class ItemPickUp : MonoBehaviour
{
    [Inject] Inventory inventory;
    [Inject] HotbarController hotbar;
    [Inject] AttackPlayer playerAttack;
    [SerializeField] public ItemData itemData;
    [SerializeField] public int amount = 1;
    Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (inventory != null)
            {
                inventory.AddItem(
                    itemData.prefab,
                    itemData.itemName,
                    amount,
                    itemData.itemSprite,
                    itemData.healAmount,
                    itemData.worldPrefab
                );

                if (hotbar != null)
                {
                    GameObject selectedUI = hotbar.GetSelectedItem();
                    if (selectedUI != null && selectedUI.name == itemData.itemName)
                    {
                        if (playerAttack.currentWeapon != null)
                            Destroy(playerAttack.currentWeapon);

                        GameObject newWeapon = Instantiate(itemData.worldPrefab, playerAttack.pivotRight);
                        playerAttack.currentWeapon = newWeapon;
                        playerAttack.MoveWeaponToHand(playerAttack.pivotRight);
                    }
                }
                Destroy(gameObject);
            }
        }
    }
}

using NUnit.Framework.Interfaces;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount = 1;
    [SerializeField] private Inventory inventory;

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
                    itemData.healAmount
                );

                Destroy(gameObject); 
            }
        }
    }
}

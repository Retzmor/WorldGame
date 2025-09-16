using NUnit.Framework.Interfaces;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    [SerializeField] public ItemData itemData;
    [SerializeField] public int amount = 1;
    [SerializeField] public Inventory inventory;

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

                Destroy(gameObject); 
            }
        }
    }
}

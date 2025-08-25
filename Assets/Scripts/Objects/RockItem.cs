using UnityEngine;

public class RockItem : MonoBehaviour
{
    [SerializeField] GameObject itemToAdd;
    [SerializeField] int amountToAdd;
    [SerializeField] Sprite itemSprite;
    [SerializeField] int healAmount = 20;
    [SerializeField] public Inventory inventory;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inventory.CheckSlotsAvailability(itemToAdd, itemToAdd.name, amountToAdd);
            inventory.AddItemToHotbar(itemToAdd, itemSprite, itemToAdd.name, healAmount);
            inventory.AddItemToHotbar(itemToAdd, itemSprite, itemToAdd.name, healAmount);
            Destroy(gameObject);
        }
    }
}

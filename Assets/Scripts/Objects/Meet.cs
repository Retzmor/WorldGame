using UnityEngine;

public class Meet : MonoBehaviour
{
    [SerializeField] GameObject itemToAdd;
    [SerializeField] int amountToAdd;
    [SerializeField] public Inventory inventory;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inventory.CheckSlotsAvailability(itemToAdd, itemToAdd.name, amountToAdd);
            Sprite itemSprite = itemToAdd.GetComponent<SpriteRenderer>().sprite;
            int healAmount = 20; // o el valor que corresponda
            inventory.AddItemToHotbar(itemToAdd, itemSprite, itemToAdd.name, healAmount);
            Destroy(gameObject);
        }
    }
}

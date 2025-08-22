using UnityEngine;

public class ItemUse : MonoBehaviour
{
    [SerializeField] int healthToGive = 20;
    private string itemName;
    private int healAmount;
    public void SetItem(string name, int heal)
    {
        itemName = name;
        healAmount = heal;
    }

    public void UseButton()
    {
        if(gameObject.name == "Potion(Use)")
        {
            HealthPlayer player = FindAnyObjectByType<HealthPlayer>();
            if (player != null)
                player.HealHealth(healthToGive);
        }
    }
}

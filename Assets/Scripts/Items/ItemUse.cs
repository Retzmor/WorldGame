using UnityEngine;

public class ItemUse : MonoBehaviour
{
    [SerializeField] int healthToGive = 20;

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

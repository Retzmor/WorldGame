using UnityEngine;

public class ItemUse : MonoBehaviour
{
    [SerializeField] int healthToGive = 20;
    public GameObject itemPrefab;
    [SerializeField] GameObject worldPrefap;
    private string itemName;
    private int healAmount;
    public void SetItem(string name, int heal, GameObject worldPrefab)
    {
        itemName = name;
        healAmount = heal;
        this.worldPrefap = worldPrefab;
    }

    public GameObject GetWorldPrefab()
    {
        return worldPrefap;
    }

    public void UseButton()
    {
        Debug.Log("Boton oprimido");
        if(gameObject.name == "Potion(Use)")
        {
            Debug.Log("Cure al player");
            HealthPlayer player = FindAnyObjectByType<HealthPlayer>();
            if (player != null)
                player.HealHealth(healthToGive);
        }
    }
}

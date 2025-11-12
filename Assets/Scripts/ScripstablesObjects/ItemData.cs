using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;       
    public Sprite itemSprite;
    public GameObject worldPrefab; // Erwin del futuro, esto quiza en un futuro de un acoplamiento de locos, hay que ir craneando que hacer
    public GameObject prefab;     
    public int healAmount;
    public WeaponType itemType;
}

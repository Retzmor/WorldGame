using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;       // Nombre del ítem
    public Sprite itemSprite;     // Sprite que se muestra en inventario/hotbar
    public GameObject prefab;     // Prefab que se instancia en hotbar/inventario
    public int healAmount;        // Cantidad de curación o efecto del ítem

}

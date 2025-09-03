using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;       // Nombre del �tem
    public Sprite itemSprite;     // Sprite que se muestra en inventario/hotbar
    public GameObject prefab;     // Prefab que se instancia en hotbar/inventario
    public int healAmount;        // Cantidad de curaci�n o efecto del �tem

}

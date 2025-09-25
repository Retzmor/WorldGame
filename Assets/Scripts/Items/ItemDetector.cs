using UnityEngine;
using UnityEngine.InputSystem;
using Zenject; 

public class ItemDetector : MonoBehaviour
{
    [Inject] Inventory inventory;
    public Vector2 detectionSize = new Vector2(1f, 1f);
    public LayerMask pickableLayer;

    private PlayerInput playerInput;
    private InputAction pickUpAction;

    public Transform weaponHolder;
    private GameObject equippedWeapon;
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        pickUpAction = playerInput.actions["Obtener"];
    }

    private void OnEnable()
    {
        pickUpAction.performed += OnPickUp;
    }

    private void OnDisable()
    {
        pickUpAction.performed -= OnPickUp;
    }

    private void OnPickUp(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, detectionSize, 0, pickableLayer);

        if (hits.Length > 0)
        {
            GameObject newWeapon = hits[0].gameObject;

            WeaponPickUp pickUpData = newWeapon.GetComponent<WeaponPickUp>();
            if (pickUpData != null && pickUpData.itemData != null)
            {
                if (inventory != null)
                {
                    inventory.AddItem(
                        pickUpData.itemData.prefab,
                        pickUpData.itemData.itemName,
                        1,
                        pickUpData.itemData.itemSprite,
                        pickUpData.itemData.healAmount,
                        pickUpData.itemData.worldPrefab
                    );
                }

                EquipWeapon(pickUpData.itemData.worldPrefab);

                Destroy(newWeapon);
                return;
            }
        }
    }
    void EquipWeapon(GameObject weaponPrefab)
    {
        GameObject weaponGame = Instantiate(weaponPrefab);

        equippedWeapon = weaponGame;

        weaponGame.transform.SetParent(weaponHolder, false);

        weaponGame.transform.localPosition = Vector3.zero;
        weaponGame.transform.localRotation = Quaternion.identity;

        SetLayerRecursively(weaponGame, LayerMask.NameToLayer("Equipped"));

        var pickupCol = weaponGame.GetComponent<Collider2D>();
        if (pickupCol) pickupCol.enabled = false;

        var rb = weaponGame.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        GetComponent<AttackPlayer>().currentWeapon = weaponGame;
    }

    void DropWeapon()
    {
        equippedWeapon.transform.SetParent(null);

        SetLayerRecursively(equippedWeapon, LayerMask.NameToLayer("Pickable"));

        var pickupCol = equippedWeapon.GetComponent<Collider2D>();
        if (pickupCol) pickupCol.enabled = true;

        var rb = equippedWeapon.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = true;

        equippedWeapon = null;
        GetComponent<AttackPlayer>().currentWeapon = null;
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, detectionSize);
    }
}

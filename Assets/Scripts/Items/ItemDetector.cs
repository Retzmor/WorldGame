using UnityEngine;
using UnityEngine.InputSystem; // Importante para el nuevo Input System

public class ItemDetector : MonoBehaviour
{
    public Vector2 detectionSize = new Vector2(1f, 1f);
    public LayerMask pickableLayer;

    private PlayerInput playerInput;
    private InputAction pickUpAction;

    private GameObject equippedWeapon;

    private AttackPlayer attackPlayer;

    private void Awake()
    {
        // Referencia al PlayerInput y AttackPlayer
        playerInput = GetComponent<PlayerInput>();
        attackPlayer = GetComponent<AttackPlayer>();

        // Asume que la acción en tu Input Actions se llama "Obtener"
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

            if (equippedWeapon != null)
                DropWeapon();

            EquipWeapon(newWeapon);
        }
    }

    void EquipWeapon(GameObject weapon)
    {
        equippedWeapon = weapon;

        // 👉 ahora siempre se equipa en el pivotArm
        weapon.transform.SetParent(attackPlayer.pivotLeft);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;

        SetLayerRecursively(weapon, LayerMask.NameToLayer("Equipped"));

        var pickupCol = weapon.GetComponent<Collider2D>();
        if (pickupCol) pickupCol.enabled = false;

        var rb = weapon.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        attackPlayer.currentWeapon = weapon;
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
        attackPlayer.currentWeapon = null;
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

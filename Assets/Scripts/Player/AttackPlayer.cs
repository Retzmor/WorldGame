using UnityEngine;
using UnityEngine.InputSystem;

public class AttackPlayer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] public Transform pivotRight;   // mano derecha
    [SerializeField] public Transform pivotLeft;    // mano izquierda
    [SerializeField] public GameObject currentWeapon;
    public ItemData currentWeaponData;
    [SerializeField] public WeaponType? currentWeaponType;

    [Header("Ajustes")]
    [SerializeField, Range(0.05f, 0.5f)] private float deadZoneRadius = 0.15f;
    [SerializeField, Range(5f, 50f)] private float rotationSmooth = 20f;
    [SerializeField, Range(0.1f, 0.5f)] private float flipThreshold = 0.25f;

    private Camera mainCamera;
    private Quaternion lastValidRotation = Quaternion.identity;
    private int facingDirection = 1; // 1 = derecha, -1 = izquierda
    private bool isAttacking = false;
    private Quaternion lockedRotation;

    // 👇 Input System
    private PlayerInput playerInput;
    private InputAction mousePosAction;
    [SerializeField]private BuildSystem buildSystem;


    private void Awake()
    {
        mainCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();

        // Buscar la acción "MousePos" definida en tu Input Actions
        mousePosAction = playerInput.actions["MousePos"];
    }
    private void LateUpdate()
    {
        Vector2 screenPos = mousePosAction.ReadValue<Vector2>();
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(screenPos);
        mouseWorld.z = 0f;
        HandleFlip(mouseWorld);
        if (currentWeapon == null) return;

        // Leer la posición del mouse desde Input System
        HandleWeaponRotation(mouseWorld);
    }

    private void HandleFlip(Vector3 mouseWorld)
    {
        float deltaX = mouseWorld.x - transform.position.x;

        if (deltaX > flipThreshold && facingDirection != 1)
        {
            facingDirection = 1;
            if (playerSprite) playerSprite.flipX = false;
            MoveWeaponToHand(pivotRight);
        }
        else if (deltaX < -flipThreshold && facingDirection != -1)
        {
            facingDirection = -1;
            if (playerSprite) playerSprite.flipX = true;
            MoveWeaponToHand(pivotLeft);
        }
    }

    private void HandleWeaponRotation(Vector3 mouseWorld)
    {
        if (isAttacking)
        {
            currentWeapon.transform.rotation = lockedRotation;
            return;
        }

        Vector2 diff = mouseWorld - currentWeapon.transform.position;
        float dist = diff.magnitude;

        if (dist > deadZoneRadius)
        {
            float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            lastValidRotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }
            currentWeapon.transform.rotation = Quaternion.Lerp(
            currentWeapon.transform.rotation,
            lastValidRotation,
            rotationSmooth * Time.deltaTime
        );
    }

    public void MoveWeaponToHand(Transform newPivot)
    {
        if (currentWeapon != null && newPivot != null)
        {
            currentWeapon.transform.SetParent(newPivot, false);
            currentWeapon.transform.localPosition = Vector3.zero;
            currentWeapon.transform.localRotation = Quaternion.identity;
        }
    }

    public void HitEnemy(InputAction.CallbackContext context)
    {
        if (currentWeapon != null && currentWeapon.TryGetComponent(out Weapon arm) && context.performed && !isAttacking)
        {
            lockedRotation = currentWeapon.transform.rotation;
            if(buildSystem.buildMode == false)
            arm.Attack();
        }
    }

    public void EquipWeapon(GameObject weapon, ItemData itemData)
    {
        currentWeapon = weapon;
        currentWeaponType = itemData.itemType;
        Debug.Log(itemData.itemType);
        currentWeaponData = itemData;
        if (itemData != null)
            currentWeaponType = itemData.itemType;

        MoveWeaponToHand(pivotRight);
        lastValidRotation = currentWeapon.transform.rotation;

        facingDirection = 1;
        if (playerSprite) playerSprite.flipX = false;

        Vector2 screenPos = mousePosAction.ReadValue<Vector2>();
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(screenPos);
        mouseWorld.z = 0f;
        HandleFlip(mouseWorld);
    }
    public void DropWeapon()
    {
        if (currentWeapon == null) return;

        currentWeapon.transform.SetParent(null);
        currentWeapon = null;
        currentWeaponData = null;
        currentWeaponType = null;
    }

}

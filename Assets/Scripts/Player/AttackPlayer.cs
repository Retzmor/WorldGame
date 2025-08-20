using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AttackPlayer : MonoBehaviour
{
    [Header("Configuración de ataque")]
    [SerializeField] private float radiusZoneAttack = 1f;

    [Header("Referencias")]
    [SerializeField] private GameObject _currentArm;
    [SerializeField] private Transform weaponHolder; // Nuevo: el punto donde irá el arma
    [SerializeField] private Transform pivotArm; // Para armas melee

    private Camera mainCamera;
    private bool isAttacking = false;
    private Quaternion lockedRotation;

    public GameObject CurrentArm
    {
        get => _currentArm;
        set
        {
            _currentArm = value;

            if (_currentArm != null)
            {
                // Colocar el arma como hija del Weapon Holder
                _currentArm.transform.SetParent(weaponHolder);
                _currentArm.transform.localPosition = Vector3.zero;
                _currentArm.transform.localRotation = Quaternion.identity;
            }
        }
    }

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        Vector3 direction = (mouseWorldPos - transform.position).normalized;

        if (_currentArm == null) return;

        if (!isAttacking)
        {
            float flipX = transform.position.x > mouseWorldPos.x ? -1f : 1f;
            transform.localScale = new Vector3(flipX, transform.localScale.y, transform.localScale.z);
            _currentArm.transform.up = direction;
            Transform targetPos = transform;
            if (_currentArm.TryGetComponent(out Weapon arm) && arm is ArmMelee)
                targetPos = pivotArm.transform;
            _currentArm.transform.position = targetPos.position;
        }
        else
        {
            _currentArm.transform.rotation = lockedRotation;
        }
    }

    public void HitEnemy(InputAction.CallbackContext context)
    {
        if (_currentArm != null && _currentArm.TryGetComponent(out Weapon arm) && context.performed && !isAttacking)
        {
            lockedRotation = _currentArm.transform.rotation;
            arm.Attack();
        }
    }

}

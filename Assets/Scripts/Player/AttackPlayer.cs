using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class AttackPlayer : MonoBehaviour
{
    [SerializeField] LayerMask layer;
    [SerializeField] float radiusZoneAttack;
    [SerializeField] GameObject _currentArm;
    [SerializeField] float radiusRotation;
    public bool canAttack;

    public GameObject CurrentArm { get => _currentArm; set => _currentArm = value; }

    private void Update()
    {
        Vector3 wordPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        wordPosition.z = 0;
        Vector3 direction = (wordPosition - transform.position).normalized;
        CurrentArm.transform.up = direction;
        CurrentArm.transform.position = transform.position + direction * radiusRotation;
    }
    public void HitEnemy(InputAction.CallbackContext callBack)
    {
        if (callBack.performed)
        {
            Arms arm = CurrentArm.GetComponent<Arms>();

            Collider2D[] zone = Physics2D.OverlapCircleAll(_currentArm.transform.position, radiusZoneAttack, layer);

            for (int i = 0; i < zone.Length; i++)
            {
                if (zone[i].TryGetComponent(out IHit hit))
                {
                    hit.TakeDamage(arm.damage);
                }
            }
        }
    }

    public void ChangeRangeArm()
    {
        Arms arm = CurrentArm.GetComponent<Arms>();

        if (arm is ArmMelee)
        {
            radiusZoneAttack = 1f;
            radiusRotation = 1f;
        }

        if (arm is ArmsRange)
        {
            radiusZoneAttack = 3f;
            radiusRotation = 3f;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_currentArm.transform.position, radiusZoneAttack);
    }
}


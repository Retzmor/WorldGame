using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class AttackPlayer : MonoBehaviour
{
    [SerializeField] LayerMask layer;
    [SerializeField] float radiusZoneAttack;
    [SerializeField] GameObject _currentArm;
    [SerializeField] GameObject positionArm;
    [SerializeField] float radiusRotation;
    public bool canAttack;

    public GameObject CurrentArm { get => _currentArm; set => _currentArm = value; }

    private void Update()
    {
        Vector3 wordPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        wordPosition.z = 0;
        Vector3 direction = (wordPosition - transform.position).normalized;
        CurrentArm.transform.up = direction;
        CurrentArm.transform.position = positionArm.transform.position;

        if(gameObject.transform.position.x > wordPosition.x)
        {
            gameObject.transform.localScale = new Vector3( -1 ,transform.localScale.y, transform.localScale.z);
        }

        else
        {
            gameObject.transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        }
    }
    public void HitEnemy(InputAction.CallbackContext callBack)
    {
        if (callBack.performed)
        {
            Arms arm = CurrentArm.GetComponent<Arms>();

            if(arm is ArmMelee)
            {
                Collider2D[] zone = Physics2D.OverlapCircleAll(_currentArm.transform.position, radiusZoneAttack, layer);

                for (int i = 0; i < zone.Length; i++)
                {
                    if (zone[i].TryGetComponent(out IHit hit))
                    {
                        hit.TakeDamage(arm.damage);
                    }
                }
            }

            else if(arm is ArmsRange armRange) 
            {
                armRange.Arrow();
            }
        }
    }

    public void ChangeRangeArm()
    {
        Arms arm = CurrentArm.GetComponent<Arms>();

        if (arm is ArmMelee)
        {
            radiusZoneAttack = 0.5f;
            radiusRotation = 0.5f;
        }

        if (arm is ArmsRange)
        {
            radiusZoneAttack = 1f;
            radiusRotation = 1f;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_currentArm.transform.position, radiusZoneAttack);
    }
}


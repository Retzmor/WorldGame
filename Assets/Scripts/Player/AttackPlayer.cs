using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class AttackPlayer : MonoBehaviour
{
    GetItem getItem;
    [SerializeField] LayerMask layerEnemy;
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
            Collider2D[] zone = Physics2D.OverlapCircleAll(_currentArm.transform.position, radiusZoneAttack, layerEnemy);

            for (int i = 0; i < zone.Length; i++)
            {
                if(zone[i].CompareTag("Enemy"))
                {
                    HealthEnemy enemyHealth = zone[i].GetComponent<HealthEnemy>();
                    ArmsRange arm = CurrentArm.GetComponent<ArmsRange>();
                    arm.Attack(enemyHealth);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_currentArm.transform.position, radiusZoneAttack);
    }
}


using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class AttackPlayer : MonoBehaviour
{
    [SerializeField] LayerMask layerEnemy;
    [SerializeField] float radiusZoneAttack;
    [SerializeField] GameObject _currentArm;
    [SerializeField] float radiusRotation;

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
            
        }
        //Collider2D[] zone = Physics2D.OverlapCircleAll(transform.position, radiusZoneAttack, layerEnemy);
        //if (zone.Length > 1)
        //{
        //    foreach (Collider2D c in zone)
        //    {
        //        // Llamar a evento de daño del enemigo
        //    }
        //}
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radiusZoneAttack);
    }
}


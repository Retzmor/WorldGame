using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AttackPlayer : MonoBehaviour
{
    [Header("Configuración de ataque")]
    [SerializeField] private LayerMask layer;
    [SerializeField] private float radiusZoneAttack = 1f;

    [Header("Referencias")]
    [SerializeField] private GameObject _currentArm;
    [SerializeField] private GameObject pivotArm; // Para armas melee
    [SerializeField] private Animator animator; // Añade referencia al Animator

    private Camera mainCamera;
    private bool isAttacking = false;
    private Quaternion lockedRotation;

    public GameObject CurrentArm
    {
        get => _currentArm;
        set => _currentArm = value;
    }

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Calcular dirección hacia el mouse
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3 direction = (mouseWorldPos - transform.position).normalized;

        // Flip del jugador según la posición del mouse
        float flipX = transform.position.x > mouseWorldPos.x ? -1f : 1f;
        transform.localScale = new Vector3(flipX, transform.localScale.y, transform.localScale.z);



        if (_currentArm == null) return; // No hacer nada si no hay arma


        if (!isAttacking)
        {
            // Determinar punto de posicionamiento
            Transform targetPos = transform; // Por defecto: centro del personaje (para rango)
            if (_currentArm.TryGetComponent(out Arms arm) && arm is ArmMelee)
                targetPos = pivotArm.transform; // Si es melee, usar pivot

            // Rotar y posicionar arma
            _currentArm.transform.up = direction;
            _currentArm.transform.position = targetPos.position;
        }
        else
        {
            _currentArm.transform.rotation = lockedRotation;
        }


        

        

       

       
    }

    public void HitEnemy(InputAction.CallbackContext context)
    {
        if (_currentArm.TryGetComponent(out Arms arm) && context.performed && !isAttacking)
        {
            lockedRotation = _currentArm.transform.rotation;

            arm.UseWeapon();

            if (animator != null)
                animator.SetTrigger("Attack");


            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("Hit");
            else
                Debug.LogWarning("AudioManager.Instance es null");


            StartCoroutine(UnlockAfterAttack());
        }

        IEnumerator UnlockAfterAttack()
        {
            isAttacking = true;

            // Esperar duración de la animación actual
            float animTime = animator != null ? animator.GetCurrentAnimatorStateInfo(0).length : 0.3f;
            yield return new WaitForSeconds(animTime);

            isAttacking = false;
        }


    }

}

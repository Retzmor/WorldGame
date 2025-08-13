using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.InputSystem;
public class GetItem : MonoBehaviour
{
    private AttackPlayer playerAttack;
    private bool canObjectAttack = false;

    [SerializeField] private Vector2 sizeBoxDetected;
    [SerializeField] private float angle;
    [SerializeField] private LayerMask layerArm;

    [SerializeField] private string nameLayerDefault = "Default";
    [SerializeField] private string nameLayerArm = "Arm";

    public bool CanObjectAttack { get => canObjectAttack; set => canObjectAttack = value; }

    private void Start()
    {
        playerAttack = GetComponent<AttackPlayer>();
    }

    private void Update()
    {
        DetectedObject(); // Solo para debug si lo quieres ver
    }

    public void GetItemPlayer(GameObject currentArm)
    {
        // Si más adelante hay más tipos de armas, puedes usar un diccionario o enum
        if (currentArm.CompareTag("arm") || currentArm.CompareTag("wood"))
        {
            playerAttack.CurrentArm = currentArm;
        }
    }

    public void CanGetObject(InputAction.CallbackContext callBack)
    {
        if (!callBack.performed)
            return;

        // Llamamos una sola vez a la detección
        Collider2D[] detected = DetectedObject();

        if (detected.Length == 0)
            return;

        GameObject objectDetected = detected[0].gameObject;

        if (objectDetected != null)
        {
            

            // Si el jugador ya tenía un arma, la soltamos
            if (playerAttack.CurrentArm != null)
            {
                playerAttack.CurrentArm.transform.parent = null;
                CambiarLayerRecursivo(playerAttack.CurrentArm, LayerMask.NameToLayer(nameLayerArm));
            }

            // Asignamos el nuevo arma
            playerAttack.CurrentArm = objectDetected;
            CambiarLayerRecursivo(playerAttack.CurrentArm, LayerMask.NameToLayer(nameLayerDefault));

            // Hacemos hijo del jugador
            playerAttack.CurrentArm.transform.parent = transform;
            playerAttack.CurrentArm.transform.localPosition = Vector3.zero;

            canObjectAttack = true;
        }
    }

    public Collider2D[] DetectedObject()
    {
        Collider2D[] items = Physics2D.OverlapBoxAll(transform.position, sizeBoxDetected, angle, layerArm);

        if (items.Length > 0)
        {
            
        }

        return items;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, sizeBoxDetected);
    }

    private void CambiarLayerRecursivo(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            CambiarLayerRecursivo(child.gameObject, newLayer);
        }
    }
}

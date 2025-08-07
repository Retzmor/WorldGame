using Unity.Burst.Intrinsics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
public class GetItem : Player
{
    AttackPlayer playerAttack;
    private bool canObjectAttack = false;
    [SerializeField] Vector2 sizeBoxDetected;
    [SerializeField] float angle;   
    [SerializeField] LayerMask layerArm;

    [SerializeField] string nameLayerDefault = "Default";
    [SerializeField] string nameLayerArm = "Arm";


    public bool CanObjectAttack { get => canObjectAttack; set => canObjectAttack = value; }

    private void Start()
    {
        playerAttack = GetComponent<AttackPlayer>();
    }

    private void Update()
    {
        DetectedObject();
    }

    public void GetItemPlayer(GameObject currentArm) 
     {
        switch(currentArm.tag)
        {
            case "arm":
                playerAttack.CurrentArm = currentArm;
                break;

            case "wood":
                playerAttack.CurrentArm = currentArm;
                break;
        } 
     }

    public void CanGetObject(InputAction.CallbackContext callBack)
    {
        if(callBack.performed && DetectedObject()[0] != null) 
        {
            GameObject objectDetected = DetectedObject()[0].gameObject;

            if(objectDetected != null)
            {
                Debug.Log("Tengo un arma");
                playerAttack.CurrentArm.transform.parent = null;
                CambiarLayerRecursivo(playerAttack.CurrentArm, LayerMask.NameToLayer(nameLayerArm));
                playerAttack.CurrentArm = objectDetected;
                playerAttack.ChangeRangeArm();
                CambiarLayerRecursivo(playerAttack.CurrentArm, LayerMask.NameToLayer(nameLayerDefault));
                playerAttack.CurrentArm.transform.parent = transform;
                canObjectAttack = true;
            }
        }
    }

    public Collider2D[] DetectedObject()
    {
        Collider2D[] items = Physics2D.OverlapBoxAll(transform.position, sizeBoxDetected, angle , layerArm);
        if(items.Length > 0)
        {
            Debug.Log("Un arma");
        }
        return items;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, sizeBoxDetected);
    }

    void CambiarLayerRecursivo(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            CambiarLayerRecursivo(child.gameObject, newLayer);
        }
    }
}

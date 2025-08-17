using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public Weapon currentWeapon;

    public void EquipWeapon(Weapon newWeapon)
    {
        if (currentWeapon != null)
        {
            // Si quieres soltarla
            currentWeapon.transform.parent = null;
        }

        currentWeapon = newWeapon;
        currentWeapon.transform.parent = transform;
        currentWeapon.transform.localPosition = Vector3.zero;
        GetComponent<AttackPlayer>().CurrentArm = currentWeapon.gameObject;
        Debug.Log("Hijo");
    }
}

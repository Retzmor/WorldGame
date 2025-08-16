using UnityEngine;

public abstract class Arms : MonoBehaviour
{
    public int damage = 10;
    [SerializeField] protected Weapon weaponType;
    
    public abstract void UseWeapon();
}

using UnityEngine;

public class ArmsRange : Arms
{
    [SerializeField] GameObject Bullet;

    public override void UseWeapon()
    {
        GameObject obj = Instantiate(Bullet, transform.position, Quaternion.identity);
        obj.transform.right = transform.up;
    }
}

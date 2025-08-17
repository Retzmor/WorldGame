using UnityEngine;

public class ArmsRange : Weapon
{
    [SerializeField] GameObject Bullet;

    public override void AnimationAttack()
    {
        throw new System.NotImplementedException();
    }

    public override void Attack()
    {
        if (Time.time - lastShotTime < 1f / fireRate)
            return;

        lastShotTime = Time.time;

        GameObject obj = Instantiate(Bullet, transform.position, Quaternion.identity);
        obj.transform.right = transform.up;
    }
}

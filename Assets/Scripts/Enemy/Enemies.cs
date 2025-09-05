using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class Enemies : Damageable
{

    [SerializeField] GameObject _gameObject;
    [SerializeField] private int _damage;
    [SerializeField] Transform[] bones;
    [SerializeField] int force;
    public int Damage { get => _damage; set => _damage = value; }


    protected override void Start()
    {
        base.Start(); // llama a Damageable.Start()
    }


    protected override void Death()
    {
        foreach (Transform bone in bones)
        {
            bone.SetParent(null);
            bone.gameObject.SetActive(true);

            Vector2 direction = bone.transform.position - transform.position;

            Vector2 randomOffset = new Vector2(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f)
        );
            direction += randomOffset;
            direction.Normalize();

            if (bone.TryGetComponent(out Rigidbody2D rb))
            {
                rb.AddForce(direction * force, ForceMode2D.Impulse);
                rb.linearDamping = 5f;
            }
        }

        Destroy(gameObject);
    }
}

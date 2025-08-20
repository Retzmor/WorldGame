using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class Enemies : MonoBehaviour, IHit
{
    NavMeshAgent _agent;
    [SerializeField] GameObject _gameObject;
    private int _damage;

    [SerializeField] float _health = 100;
    [SerializeField] float _currentHealth;
    private DamageFlash damageFlash;
    public int Damage { get => _damage; set => _damage = value; }

    void Start()
    {
        _currentHealth = _health;
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
       damageFlash = GetComponent<DamageFlash>();
    }

    public void Death()
    {
        Destroy(gameObject);
        
    }

    public void TakeDamage(float damage, WeaponType weapon)
    {
        _health -= damage;
        damageFlash.CallDamageFlash();

        if (_health <= 0)
        {
            Death();
        }
        
    }

    void Update()
    {
        _agent.SetDestination(_gameObject.transform.position);

        Vector3 direction = _gameObject.transform.position - transform.position;

        if (direction.x < 0)
            transform.localScale = new Vector3(1, 1, 1); 
        else if (direction.x > 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }
}

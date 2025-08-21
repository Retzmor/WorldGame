using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class Enemies : Damageable
{
    NavMeshAgent _agent;
    [SerializeField] GameObject _gameObject;
    private int _damage;
    public int Damage { get => _damage; set => _damage = value; }


    protected override void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        base.Start(); // llama a Damageable.Start()
        // Aquí tu lógica específica de Animal
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

    protected override void Death()
    {
        gameObject.SetActive(false);
    }
}

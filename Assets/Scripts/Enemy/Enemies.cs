using UnityEngine;
using UnityEngine.AI;
public class Enemies : MonoBehaviour
{
    NavMeshAgent _agent;
    [SerializeField] GameObject _gameObject;
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
    }

    // Update is called once per frame
    void Update()
    {
        _agent.SetDestination(_gameObject.transform.position);
    }
}

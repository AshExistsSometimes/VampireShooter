using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIMovement : MonoBehaviour
{
    public NavMeshAgent agent;

    [Header("Movement")]
    public float baseSpeed = 3.5f;

    private void Awake() // Setup agent
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = baseSpeed;
    }

    public void MoveTo(Vector3 target) // Move to position
    {
        agent.isStopped = false;
        agent.SetDestination(target);
    }

    public void Stop() // Stop movement
    {
        agent.isStopped = true;
    }

    public bool ReachedDestination(float threshold = 1.2f) // Check arrival
    {
        if (!agent.pathPending && agent.remainingDistance <= threshold)
            return true;

        return false;
    }
}

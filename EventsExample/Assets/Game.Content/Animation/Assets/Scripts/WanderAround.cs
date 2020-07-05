using UnityEngine;
using UnityEngine.AI;

public class WanderAround : MonoBehaviour
{
    public NavMeshAgent Agent;

    public float WanderDistance = 10f;

    private void Start()
    {
        if (Agent == null)
            Agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (Agent.remainingDistance <= Agent.stoppingDistance)
            Agent.SetDestination(Agent.RandomPosition(WanderDistance));
    }
}

public static class NavMeshExtensions
{
    public static Vector3 RandomPosition(this NavMeshAgent agent, float radius)
    {
        var randDirection = Random.insideUnitSphere * radius;
        randDirection += agent.transform.position;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, radius, -1);
        return navHit.position;
    }
}
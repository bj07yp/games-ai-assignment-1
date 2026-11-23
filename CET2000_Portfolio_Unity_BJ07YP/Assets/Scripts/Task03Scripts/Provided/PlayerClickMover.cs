using UnityEngine;
using UnityEngine.AI;

// CET2000 Lab 08 - the green player (provided, no changes needed).
// Left-click on the floor and the player walks there on the NavMesh (click-to-move, like an RTS game).
[RequireComponent(typeof(NavMeshAgent))]
public class PlayerClickMover : MonoBehaviour
{
    NavMeshAgent agent;

    void Start() { agent = GetComponent<NavMeshAgent>(); }

    public void MoveTo(Vector3 point)
    {
        if (agent != null && agent.isOnNavMesh) agent.SetDestination(point);
    }
}

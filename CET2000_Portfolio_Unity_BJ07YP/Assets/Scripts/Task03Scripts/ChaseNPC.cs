using UnityEngine;
using UnityEngine.AI;

// CET2000 Lab 08 - Part B: the blue NPC chases the green player across the NavMesh.
// The NavMeshAgent does the hard work: A* over the NavMesh polygons, smoothing the path, moving, slowing down.
// You only have to tell it where to go.
[RequireComponent(typeof(NavMeshAgent))]
public class ChaseNPC : MonoBehaviour
{
    public Transform player;
    [Tooltip("How often to plan a new path to the player, in seconds. 0 = every frame.")]
    public float repathInterval = 0.2f;

    NavMeshAgent agent;
    float nextRepath;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player == null || !agent.isOnNavMesh) return;
        if (Time.time >= nextRepath)                    // time to plan again?
        {
            agent.SetDestination(player.position); // plan a path on the NavMesh and start following it
            nextRepath = Time.time + repathInterval; // the player moves, so plan again a little later
        }
    }

    void OnDisable()
    {
        nextRepath = 0f;                                // plan straight away when switched on again
        if (agent != null && agent.isOnNavMesh) agent.ResetPath();
    }
}

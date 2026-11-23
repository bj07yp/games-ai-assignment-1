using UnityEngine;
using UnityEngine.AI;

// CET2000 Lab 08 - Part F: the hybrid approach from the lecture.
// The NavMesh plans the GLOBAL route to the player; evade steering makes a LOCAL change when the guard is close.
[RequireComponent(typeof(NavMeshAgent))]
public class HybridNavigator : MonoBehaviour
{
    public Transform player, enemy;
    [Tooltip("Evade when the enemy is closer than this (metres).")]
    public float dangerRadius = 3f;
    [Tooltip("How strongly to push away from the enemy, compared with the path.")]
    public float evadeWeight = 1.5f;
    [Tooltip("Expert E2: evade where the enemy is going to be, not where it is.")]
    public bool predictEnemy = false;

    NavMeshAgent agent;
    public NavMeshAgent EnemyAgent { get; private set; }   // the enemy's agent, for its velocity (Expert E2)
    public bool Evading { get; private set; }               // read by the panel

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (enemy != null) EnemyAgent = enemy.GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player == null || enemy == null || !agent.isOnNavMesh) return;
        // TODO 8 (Part F): four steps, see the lab instructions.
        //   1 GLOBAL: send the agent to the player (SetDestination)
        //   2 SEEK:   Vector3 desired = agent.desiredVelocity;
        //   3 LOCAL:  away = transform.position - enemy.position (set away.y = 0)
        //             Evading = away.magnitude < dangerRadius
        //             if Evading, add away.normalized * agent.speed * evadeWeight to desired
        //   4 MOVE:   agent.velocity = Vector3.ClampMagnitude(desired, agent.speed);
        Evading = false;
    }

    void OnDisable()
    {
        Evading = false;
        if (agent != null && agent.isOnNavMesh) agent.ResetPath();
    }
}

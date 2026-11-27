using UnityEngine;
using UnityEngine.AI;

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
        Evading = false;
    }

    void OnDisable()
    {
        Evading = false;
        if (agent != null && agent.isOnNavMesh) agent.ResetPath();
    }
}

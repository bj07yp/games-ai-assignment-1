using UnityEngine;
using UnityEngine.AI;

// CET2000 Lab 08 - Part C: the red guard patrols designer-placed waypoints.
// The waypoints decide WHERE to go next; the NavMesh works out HOW to get there.
[RequireComponent(typeof(NavMeshAgent))]
public class Patrol : MonoBehaviour
{
    public Transform[] waypoints;
    [Tooltip("Off: 1 2 3 4 1 2 3 4 ...   On: 1 2 3 4 3 2 1 2 ...")]
    public bool pingPong = false;
    [Tooltip("How close (metres) counts as 'arrived'.")]
    public float arriveDistance = 0.3f;
    [Tooltip("Expert E3: seconds to stand still at each waypoint.")]
    public float waitTime = 0f;

    int index = 0;      // the waypoint the guard is walking to now
    int step = 1;       // ping-pong direction: +1 forwards, -1 backwards
    NavMeshAgent agent;

    public int CurrentIndex { get { return index; } }
    public int Direction { get { return step; } }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (waypoints.Length > 0 && agent.isOnNavMesh) agent.SetDestination(waypoints[index].position);
    }

    void Update()
    {
        if (waypoints.Length == 0 || !agent.isOnNavMesh) return;
        // TODO 2a (Part C): the guard has arrived when the path is NOT still being planned (agent.pathPending)
        //   AND agent.remainingDistance is no more than arriveDistance. Replace false with that test.
        bool arrived = !agent.pathPending && agent.remainingDistance <= arriveDistance;
        if (!arrived) return;
        index = NextIndex();
        agent.SetDestination(waypoints[index].position);
    }

    int NextIndex()
    {
        if (!pingPong)
            return (index + 1) % waypoints.Length; // loop: 0 1 2 3 0 1 ... (% wraps 4 back to 0)
        if (waypoints.Length < 2) return 0;
        int next = index + step;
        if (next < 0 || next >= waypoints.Length) // would fall off either end: turn round
        {
            step = -step;
            next = index + step;
        }
        return next; // ping-pong: 0 1 2 3 2 1 0 1 ..
    }
}

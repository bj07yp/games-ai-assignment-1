using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// CET2000 Lab 08 - on-screen panel for the NavMesh scene (provided, no changes needed).
// Mouse on the floor: left-click = the green player walks there.
// Keys: 1 = NPC idle, 2 = chase, 3 = hybrid, D = door open/closed, N = show/hide the NavMesh.
public class NavLabHud : MonoBehaviour
{
    public const float PanelWidth = 310f;

    public NavMeshBaker baker;
    public PlayerClickMover player;
    public NavMeshAgent npc;
    public ChaseNPC chase;
    public HybridNavigator hybrid;
    public Patrol guard;
    public GameObject door;
    public string steeringScene = "Lab08_Steering";

    public enum NpcMode { Idle, Chase, Hybrid }
    NpcMode mode = NpcMode.Chase;
    float radius;
    float npcStuck, guardStill;
    LineRenderer pathLine, dangerRing;
    NavMeshAgent guardAgent;
    GUIStyle box, title, small, hint;
    Vector2 scroll;

    void Start()
    {
        LabDraw.FixMaterials();
        radius = baker.agentRadius;
        guardAgent = guard.GetComponent<NavMeshAgent>();
        pathLine = LabDraw.Line("NPC path (corners)", new Color(0.95f, 0.55f, 0.05f), 0.08f);
        dangerRing = LabDraw.Line("Danger radius", new Color(0.8f, 0.2f, 0.2f), 0.06f);
        SetMode(NpcMode.Chase);
    }

    void SetMode(NpcMode m)
    {
        mode = m;
        chase.enabled = m == NpcMode.Chase;
        hybrid.enabled = m == NpcMode.Hybrid;
        if (m == NpcMode.Idle && npc.isOnNavMesh) npc.ResetPath();
        npcStuck = 0f;
    }

    void Update()
    {
        // The NPC's path: the corners the NavMeshAgent will walk through (A* over polygons, pulled tight).
        if (npc.isOnNavMesh && npc.hasPath)
        {
            Vector3[] c = npc.path.corners;
            pathLine.loop = false;
            pathLine.positionCount = c.Length;
            for (int i = 0; i < c.Length; i++) pathLine.SetPosition(i, new Vector3(c[i].x, 0.1f, c[i].z));
        }
        else LabDraw.Hide(pathLine);

        if (mode == NpcMode.Hybrid) LabDraw.Circle(dangerRing, guard.transform.position, hybrid.dangerRadius);
        else LabDraw.Hide(dangerRing);

        float toPlayer = Vector3.Distance(Flat(npc.transform.position), Flat(player.transform.position));
        bool npcStill = npc.velocity.magnitude < 0.05f;
        npcStuck = (mode != NpcMode.Idle && npcStill && toPlayer > 2f) ? npcStuck + Time.deltaTime : 0f;
        bool guardStopped = guard.enabled && guardAgent.velocity.magnitude < 0.05f;
        guardStill = guardStopped ? guardStill + Time.deltaTime : 0f;
    }

    static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }

    string Hint()
    {
        if (npcStuck > 1.5f && mode == NpcMode.Chase) return "The NPC is not moving. Is TODO 1 (ChaseNPC.cs) finished?";
        if (npcStuck > 1.5f && mode == NpcMode.Hybrid) return "The NPC is not moving in Hybrid mode. Is TODO 8 (HybridNavigator.cs) finished?";
        if (guardStill > 1.5f + guard.waitTime) return "The guard has stopped at waypoint " + (guard.CurrentIndex + 1) + ". Is TODO 2 (Patrol.cs) finished?";
        if (npc.isOnNavMesh && npc.hasPath && npc.pathStatus == NavMeshPathStatus.PathPartial)
            return "Partial path: the NPC cannot reach the player, so it goes as close as it can.";
        return "";
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0 && e.mousePosition.x > PanelWidth + 20f)
        {
            Vector3 p;
            if (LabDraw.MouseToFloor(e.mousePosition, out p)) { player.MoveTo(p); e.Use(); }
        }
        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Alpha1) SetMode(NpcMode.Idle);
            else if (e.keyCode == KeyCode.Alpha2) SetMode(NpcMode.Chase);
            else if (e.keyCode == KeyCode.Alpha3) SetMode(NpcMode.Hybrid);
            else if (e.keyCode == KeyCode.D && door != null) door.SetActive(!door.activeSelf);
            else if (e.keyCode == KeyCode.N) baker.showNavMesh = !baker.showNavMesh;
        }

        if (box == null)
        {
            box = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, padding = new RectOffset(10, 10, 8, 8) };
            title = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold };
            small = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true };
            hint = new GUIStyle(small) { fontStyle = FontStyle.Bold };
            hint.normal.textColor = new Color(1f, 0.82f, 0.35f);
        }
        GUILayout.BeginArea(new Rect(10, 10, PanelWidth, Screen.height - 20));
        scroll = GUILayout.BeginScrollView(scroll);
        GUILayout.BeginVertical(box);
        GUILayout.Label("Lab 08: NavMesh, waypoints, hybrid", title);

        // NPC
        GUILayout.Label("Blue NPC: " + (mode == NpcMode.Hybrid ? "Hybrid (NavMesh + evade)" : mode.ToString()));
        GUILayout.BeginHorizontal();
        ModeButton(NpcMode.Idle, "1 Idle");
        ModeButton(NpcMode.Chase, "2 Chase (T1)");
        ModeButton(NpcMode.Hybrid, "3 Hybrid (T8)");
        GUILayout.EndHorizontal();
        if (npc.isOnNavMesh)
        {
            string status = npc.hasPath ? (npc.pathStatus == NavMeshPathStatus.PathComplete ? "complete" : "PARTIAL") : "none";
            int corners = npc.hasPath ? npc.path.corners.Length : 0;
            GUILayout.Label(string.Format("Speed {0:0.0} m/s · path {1} · {2} corners · {3:0.0} m left",
                npc.velocity.magnitude, status, corners, npc.hasPath ? npc.remainingDistance : 0f), small);
        }
        Slider("NPC Speed", npc, 0);
        Slider("NPC Acceleration", npc, 1);
        Slider("NPC Angular Speed", npc, 2);
        Slider("NPC Stopping Distance", npc, 3);
        npc.autoBraking = GUILayout.Toggle(npc.autoBraking, " Auto Braking (slow down near the target)");

        if (mode == NpcMode.Hybrid)
        {
            GUILayout.Space(4);
            GUILayout.Label("Danger radius: " + hybrid.dangerRadius.ToString("0.0") + " m", small);
            hybrid.dangerRadius = GUILayout.HorizontalSlider(hybrid.dangerRadius, 1f, 6f);
            GUILayout.Label("Evade weight: " + hybrid.evadeWeight.ToString("0.0"), small);
            hybrid.evadeWeight = GUILayout.HorizontalSlider(hybrid.evadeWeight, 0f, 4f);
            hybrid.predictEnemy = GUILayout.Toggle(hybrid.predictEnemy, " Evade the guard's predicted position (E2)");
            GUILayout.Label(hybrid.Evading ? "EVADING the guard" : "Following the path", hint);
        }

        // Guard
        GUILayout.Space(6);
        GUILayout.Label("Red guard: waypoint " + (guard.CurrentIndex + 1) + " of " + guard.waypoints.Length +
                        (guard.pingPong ? (guard.Direction > 0 ? " (forwards)" : " (backwards)") : ""));
        GUILayout.BeginHorizontal();
        guard.enabled = GUILayout.Toggle(guard.enabled, " Patrol");
        guard.pingPong = GUILayout.Toggle(guard.pingPong, " Ping-pong (T3)");
        GUILayout.EndHorizontal();
        GUILayout.Label("Wait at each waypoint (E3): " + guard.waitTime.ToString("0.0") + " s", small);
        guard.waitTime = GUILayout.HorizontalSlider(guard.waitTime, 0f, 3f);

        // World
        GUILayout.Space(6);
        if (door != null)
        {
            bool closed = GUILayout.Toggle(door.activeSelf, " Door closed (NavMeshObstacle, Carve) (D)");
            if (closed != door.activeSelf) door.SetActive(closed);
        }
        baker.showNavMesh = GUILayout.Toggle(baker.showNavMesh, " Show the NavMesh (N)");
        GUILayout.Label("Bake agent radius: " + radius.ToString("0.00") + " m   (baked: " + baker.BakedRadius.ToString("0.00") +
                        ", " + baker.TriangleCount + " triangles)", small);
        radius = GUILayout.HorizontalSlider(radius, 0.1f, 1.5f);
        if (GUILayout.Button("Bake again with this radius")) { baker.agentRadius = radius; baker.Bake(); }

        string h = Hint();
        if (h.Length > 0) { GUILayout.Space(4); GUILayout.Label(h, hint); }

        GUILayout.Space(6);
        GUILayout.Label("Left-click the floor: the green player walks there. Light blue = NavMesh, ORANGE line = the NPC's path " +
                        "(its corners), purple discs = waypoints, red ring = danger radius (Hybrid).", small);
        if (GUILayout.Button("Steering arena →")) SceneManager.LoadScene(steeringScene);
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    void ModeButton(NpcMode m, string label)
    {
        if (GUILayout.Toggle(mode == m, label, GUI.skin.button) && mode != m) SetMode(m);
    }

    void Slider(string label, NavMeshAgent a, int which)
    {
        float v = which == 0 ? a.speed : which == 1 ? a.acceleration : which == 2 ? a.angularSpeed : a.stoppingDistance;
        float max = which == 0 ? 8f : which == 1 ? 40f : which == 2 ? 720f : 4f;
        GUILayout.Label(label + ": " + v.ToString(which == 2 ? "0" : "0.0"), small);
        v = GUILayout.HorizontalSlider(v, which == 3 ? 0f : 0.5f, max);
        if (which == 0) a.speed = v; else if (which == 1) a.acceleration = v; else if (which == 2) a.angularSpeed = v; else a.stoppingDistance = v;
    }
}

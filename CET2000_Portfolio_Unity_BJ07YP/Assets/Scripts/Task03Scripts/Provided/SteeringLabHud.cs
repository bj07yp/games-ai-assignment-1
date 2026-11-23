using UnityEngine;
using UnityEngine.SceneManagement;

// CET2000 Lab 08 - on-screen panel for the Steering arena (provided, no changes needed).
// Mouse on the floor: left-click = move the green target, right-click = move the red threat.
// Keys: 1-8 choose a behaviour, M = target moves on/off, R = reset the NPC.
public class SteeringLabHud : MonoBehaviour
{
    public const float PanelWidth = 310f;
    public SteeringDemo demo;
    public TargetMover target;
    public Transform threat;
    public string navMeshScene = "Lab08_NavMesh";

    GUIStyle box, title, small, hint;
    Vector2 scroll;

    void Start() { LabDraw.FixMaterials(); }

    void SetMode(SteeringDemo.Mode m)
    {
        demo.mode = m;
        if ((m == SteeringDemo.Mode.Pursuit || m == SteeringDemo.Mode.Evade) && !target.moving) target.StartMoving();
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.mousePosition.x > PanelWidth + 20f)
        {
            Vector3 p;
            if (LabDraw.MouseToFloor(e.mousePosition, out p))
            {
                if (e.button == 0) target.PlaceAt(p);
                else if (e.button == 1 && threat != null) threat.position = new Vector3(p.x, threat.position.y, p.z);
                e.Use();
            }
        }
        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode >= KeyCode.Alpha1 && e.keyCode <= KeyCode.Alpha8) SetMode((SteeringDemo.Mode)(e.keyCode - KeyCode.Alpha1));
            else if (e.keyCode == KeyCode.M) { if (target.moving) target.moving = false; else target.StartMoving(); }
            else if (e.keyCode == KeyCode.R) demo.ResetNpc();
        }

        if (box == null)
        {
            box = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, padding = new RectOffset(10, 10, 8, 8) };
            title = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold };
            small = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true };
            hint = new GUIStyle(small) { fontStyle = FontStyle.Bold };
            hint.normal.textColor = new Color(1f, 0.82f, 0.35f);
        }
        Steering s = demo.S;
        GUILayout.BeginArea(new Rect(10, 10, PanelWidth, Screen.height - 20));
        scroll = GUILayout.BeginScrollView(scroll);
        GUILayout.BeginVertical(box);
        GUILayout.Label("Lab 08: Steering arena", title);
        GUILayout.Label("Behaviour: " + Name(demo.mode));
        Row(SteeringDemo.Mode.Stop, "1 Stop", SteeringDemo.Mode.Seek, "2 Seek (T4)", SteeringDemo.Mode.Flee, "3 Flee (T5)");
        Row(SteeringDemo.Mode.Arrive, "4 Arrive (T6)", SteeringDemo.Mode.Pursuit, "5 Pursuit (T7)", SteeringDemo.Mode.Evade, "6 Evade (T7)");
        GUILayout.BeginHorizontal();
        Button(SteeringDemo.Mode.Wander, "7 Wander (E1)");
        Button(SteeringDemo.Mode.SeekAndFlee, "8 Seek + Flee");
        GUILayout.EndHorizontal();
        bool moves = GUILayout.Toggle(target.moving, " Target moves round an oval (M)");
        if (moves != target.moving) { if (moves) target.StartMoving(); else target.moving = false; }

        if (s != null)
        {
            GUILayout.Space(6);
            Slider("Max speed", ref s.maxSpeed, 1f, 10f, "0.0", " m/s");
            Slider("Max force", ref s.maxForce, 0.5f, 20f, "0.0", "");
            Slider("Mass", ref s.mass, 0.1f, 2f, "0.00", "");
            Slider("Slow radius (Arrive)", ref s.slowRadius, 0.5f, 6f, "0.0", " m");
            Slider("Panic radius (Flee, Evade)", ref demo.panicRadius, 1f, 8f, "0.0", " m");
            Slider("Flee weight (Seek + Flee)", ref demo.fleeWeight, 0f, 5f, "0.0", "");

            GUILayout.Space(6);
            float dist = Steering.Flat(target.transform.position - demo.transform.position).magnitude;
            GUILayout.Label(string.Format("NPC speed: {0:0.00} m/s   velocity ({1:0.0}, {2:0.0})", s.velocity.magnitude, s.velocity.x, s.velocity.z), small);
            GUILayout.Label(string.Format("Steering force: {0:0.00}   ({1:0.0}, {2:0.0})", demo.LastSteering.magnitude, demo.LastSteering.x, demo.LastSteering.z), small);
            GUILayout.Label(string.Format("Distance to target: {0:0.00} m   target speed: {1:0.0} m/s", dist, target.Velocity.magnitude), small);
        }
        if (!string.IsNullOrEmpty(demo.Hint)) GUILayout.Label(demo.Hint, hint);

        GUILayout.Space(6);
        GUILayout.Label("Arrows on the floor: BLUE = current velocity, GREEN = desired velocity, ORANGE = steering force " +
                        "(desired − current). Grey line = where the NPC has been. Rings: slow radius (Arrive), panic radius " +
                        "(Flee, Evade), wander circle. Small blue ring = predicted position (Pursuit, Evade).", small);
        GUILayout.Label("Mouse on the floor: left-click = move the green target, right-click = move the red threat.", small);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset NPC (R)")) demo.ResetNpc();
        if (GUILayout.Button("← NavMesh scene")) SceneManager.LoadScene(navMeshScene);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    static string Name(SteeringDemo.Mode m) { return m == SteeringDemo.Mode.SeekAndFlee ? "Seek + Flee" : m.ToString(); }

    void Row(SteeringDemo.Mode a, string la, SteeringDemo.Mode b, string lb, SteeringDemo.Mode c, string lc)
    {
        GUILayout.BeginHorizontal();
        Button(a, la); Button(b, lb); Button(c, lc);
        GUILayout.EndHorizontal();
    }

    void Button(SteeringDemo.Mode m, string label)
    {
        if (GUILayout.Toggle(demo.mode == m, label, GUI.skin.button) && demo.mode != m) SetMode(m);
    }

    void Slider(string label, ref float value, float min, float max, string format, string unit)
    {
        GUILayout.Label(label + ": " + value.ToString(format) + unit, small);
        value = GUILayout.HorizontalSlider(value, min, max);
    }
}

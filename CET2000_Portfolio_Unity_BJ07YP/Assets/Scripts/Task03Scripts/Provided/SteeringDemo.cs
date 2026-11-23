using UnityEngine;

// CET2000 Lab 08 - runs one steering behaviour on the blue NPC in the Steering arena (provided, no changes needed).
// Each frame: ask Steering.cs for a steering force, Apply it, keep the NPC inside the arena and draw the vectors.
// Arrows: blue = current velocity, green = desired velocity, orange = steering force (from the tip of blue to the tip of green).
[RequireComponent(typeof(Steering))]
public class SteeringDemo : MonoBehaviour
{
    public enum Mode { Stop, Seek, Flee, Arrive, Pursuit, Evade, Wander, SeekAndFlee }
    public Mode mode = Mode.Seek;
    public Transform target;
    public Transform threat;
    [Tooltip("Flee and Evade only run when the threat is closer than this.")]
    public float panicRadius = 4f;
    [Tooltip("Seek + Flee: how much more the flee force counts than the seek force.")]
    public float fleeWeight = 2f;
    public Vector2 arenaHalfSize = new Vector2(11.4f, 7.4f);
    [Tooltip("Arrow length per metre per second.")]
    public float arrowScale = 0.5f;

    public static readonly Color CurrentColour = new Color(0.18f, 0.44f, 0.9f);
    public static readonly Color DesiredColour = new Color(0.15f, 0.62f, 0.33f);
    public static readonly Color SteeringColour = new Color(0.95f, 0.5f, 0.0f);

    public Steering S { get; private set; }
    public Vector3 LastSteering { get; private set; }
    public string Hint { get; private set; }

    TargetMover mover;
    LineRenderer current, desired, steer, ring, marker, link;
    TrailRenderer trail;
    Vector3 startPos;
    Quaternion startRot;

    void Start()
    {
        S = GetComponent<Steering>();
        mover = target != null ? target.GetComponent<TargetMover>() : null;
        startPos = transform.position;
        startRot = transform.rotation;
        current = LabDraw.Line("Arrow: current velocity", CurrentColour, 0.1f);
        desired = LabDraw.Line("Arrow: desired velocity", DesiredColour, 0.1f);
        steer = LabDraw.Line("Arrow: steering force", SteeringColour, 0.1f);
        ring = LabDraw.Line("Ring", new Color(0.75f, 0.3f, 0.3f), 0.05f);
        marker = LabDraw.Line("Marker", new Color(0.2f, 0.3f, 0.75f), 0.06f);
        link = LabDraw.Line("Prediction line", new Color(0.4f, 0.45f, 0.6f), 0.04f);

        var trailGo = new GameObject("Trail");                // a separate object: one renderer per GameObject
        trailGo.transform.SetParent(transform, false);
        trailGo.transform.localPosition = new Vector3(0f, -transform.position.y + 0.05f, 0f);
        trail = trailGo.AddComponent<TrailRenderer>();
        trail.time = 6f;
        trail.startWidth = 0.08f;
        trail.endWidth = 0.02f;
        trail.minVertexDistance = 0.1f;
        trail.sharedMaterial = LabDraw.Unlit(new Color(0.42f, 0.47f, 0.58f));
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    void Update()
    {
        Vector3 before = S.velocity;
        Vector3 f = Compute();
        LastSteering = Steering.Flat(f);
        S.Apply(f);
        KeepInArena();
        Draw(before);
    }

    Vector3 Compute()
    {
        Hint = "";
        Vector3 tp = target != null ? target.position : Vector3.zero;
        Vector3 tv = mover != null ? mover.Velocity : Vector3.zero;
        Vector3 hp = threat != null ? threat.position : Vector3.one * 999f;
        switch (mode)
        {
            case Mode.Seek: return Check(S.Seek(tp), "Seek", "TODO 4", tp);
            case Mode.Flee:
                if (Near(hp)) return Check(S.Flee(hp), "Flee", "TODO 5", hp);
                Hint = "Safe: the threat is outside the panic radius, so the NPC slows down. Right-click near the NPC to move the threat.";
                return Brake();
            case Mode.Arrive: return Check(S.Arrive(tp), "Arrive", "TODO 6", tp);
            case Mode.Pursuit: return Check(S.Pursuit(tp, tv), "Pursuit", "TODO 7", tp);
            case Mode.Evade:
                if (Near(tp)) return Check(S.Evade(tp, tv), "Evade", "TODO 7", tp);
                Hint = "Safe: the green target is outside the panic radius. Tick 'Target moves' and wait for it to come close.";
                return Brake();
            case Mode.Wander: return Check(S.Wander(), "Wander", "Expert E1", transform.position + Vector3.forward * 10f);
            case Mode.SeekAndFlee:
                Vector3 f = S.Seek(tp);
                if (Near(hp)) f += fleeWeight * S.Flee(hp);
                return Check(f, "Seek + Flee", "TODO 4 and 5", tp);
            default: return Brake();
        }
    }

    bool Near(Vector3 p) { return Steering.Flat(p - transform.position).magnitude < panicRadius; }
    Vector3 Brake() { return -S.velocity * 3f; }

    Vector3 Check(Vector3 f, string name, string todo, Vector3 goal)
    {
        bool still = S.velocity.sqrMagnitude < 0.0001f;
        bool farAway = Steering.Flat(goal - transform.position).magnitude > 0.6f;
        if (f.sqrMagnitude < 1e-8f && still && farAway)
            Hint = name + " returned (0, 0, 0), so the NPC cannot move. Is " + todo + " finished?";
        return f;
    }

    void KeepInArena()
    {
        Vector3 p = transform.position, v = S.velocity;
        if (Mathf.Abs(p.x) > arenaHalfSize.x) { p.x = Mathf.Sign(p.x) * arenaHalfSize.x; v.x = 0f; }
        if (Mathf.Abs(p.z) > arenaHalfSize.y) { p.z = Mathf.Sign(p.z) * arenaHalfSize.y; v.z = 0f; }
        transform.position = p;
        S.velocity = v;
    }

    void Draw(Vector3 before)
    {
        Vector3 pos = transform.position;
        Vector3 want = before + LastSteering;                       // desired = current + steering
        LabDraw.Arrow(current, pos, before * arrowScale);
        LabDraw.Arrow(desired, pos, want * arrowScale);
        Vector3 tip = pos + before * arrowScale;
        LabDraw.Arrow(steer, tip, LastSteering * arrowScale);

        LabDraw.Hide(ring); LabDraw.Hide(marker); LabDraw.Hide(link);
        Vector3 tp = target != null ? target.position : Vector3.zero;
        Vector3 tv = mover != null ? mover.Velocity : Vector3.zero;
        switch (mode)
        {
            case Mode.Arrive: LabDraw.Circle(ring, tp, S.slowRadius); break;
            case Mode.Flee:
            case Mode.SeekAndFlee: if (threat != null) LabDraw.Circle(ring, threat.position, panicRadius); break;
            case Mode.Evade: LabDraw.Circle(ring, tp, panicRadius); goto case Mode.Pursuit;
            case Mode.Pursuit:
                Vector3 p = S.PredictPosition(tp, tv);
                LabDraw.Circle(marker, p, 0.35f, 0.08f, 24);
                link.loop = false; link.positionCount = 2;
                link.SetPositions(new[] { new Vector3(tp.x, 0.08f, tp.z), new Vector3(p.x, 0.08f, p.z) });
                break;
            case Mode.Wander:
                Vector3 heading = S.velocity.sqrMagnitude > 0.01f ? S.velocity.normalized : Steering.Flat(transform.forward).normalized;
                LabDraw.Circle(ring, pos + heading * S.wanderDistance, S.wanderRadius);
                if (S.LastWanderPoint != Vector3.zero) LabDraw.Circle(marker, S.LastWanderPoint, 0.15f, 0.08f, 16);
                break;
        }
    }

    public void ResetNpc()
    {
        transform.position = startPos;
        transform.rotation = startRot;
        S.velocity = Vector3.zero;
        if (trail != null) trail.Clear();
    }
}

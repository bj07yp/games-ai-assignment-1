using UnityEngine;

// CET2000 Lab 08 - Parts D and E: steering behaviours (Reynolds, 1999).
// Every behaviour returns a STEERING FORCE using the same pattern from the lecture:
//
//      steering = desired velocity - current velocity
//
// Everything happens on the floor (the XZ plane), so Flat() sets y to 0.
// Apply() (provided, at the bottom) turns the steering force into movement every frame.
public class Steering : MonoBehaviour
{
    [Header("Limits")]
    public float maxSpeed = 5f;          // metres per second
    public float maxForce = 6f;          // the biggest steering force (how sharply it can turn)
    public float mass = 0.3f;            // heavier = slower to react (acceleration = force / mass)

    [Header("Arrive")]
    public float slowRadius = 3f;        // start slowing down this far from the target

    [Header("Pursuit and Evade")]
    public float maxPrediction = 2f;     // never look further ahead than this many seconds

    [Header("Wander (Expert E1)")]
    public float wanderDistance = 2.5f;  // how far ahead the wander circle is
    public float wanderRadius = 1.2f;    // size of the wander circle
    public float wanderJitter = 120f;    // how fast the point drifts round the circle (degrees per second)

    [HideInInspector] public Vector3 velocity;          // the NPC's current velocity
    public Vector3 LastWanderPoint { get; private set; }

    // ---------- Part D ----------

    // SEEK: head straight for the target at full speed.
    public Vector3 Seek(Vector3 target)
    {
        // TODO 4 (Part D): Seek.
        //   1. toTarget = Flat(target - transform.position)
        //   2. desired  = toTarget.normalized * maxSpeed
        //   3. return desired - velocity
        return Vector3.zero;
    }

    // FLEE: run directly away from a threat at full speed.
    public Vector3 Flee(Vector3 threat)
    {
        // TODO 5 (Part D): Flee. The same as Seek, but the direction is position - threat (away from it).
        return Vector3.zero;
    }

    // ARRIVE: seek, but slow down inside slowRadius and stop on the target.
    public Vector3 Arrive(Vector3 target)
    {
        // TODO 6 (Part D): Arrive.
        //   1. toTarget = Flat(target - transform.position); distance = toTarget.magnitude
        //   2. speed = maxSpeed * distance / slowRadius   if distance < slowRadius
        //      speed = maxSpeed                           otherwise
        //   3. desired = toTarget.normalized * speed; return desired - velocity
        return Vector3.zero;
    }

    // ---------- Part E ----------

    // Where will a moving target be in T seconds?  predicted = position + velocity * T
    public Vector3 PredictPosition(Vector3 targetPos, Vector3 targetVel)
    {
        // TODO 7a (Part E): T = distance to the target / maxSpeed, but no more than maxPrediction
        //   (use Mathf.Min). Then return targetPos + targetVel * T.
        return targetPos;
    }

    // PURSUIT: seek where the target is going to be.
    public Vector3 Pursuit(Vector3 targetPos, Vector3 targetVel)
    {
        // TODO 7b (Part E): seek the predicted position. One line.
        return Vector3.zero;
    }

    // EVADE: flee from where the threat is going to be.
    public Vector3 Evade(Vector3 threatPos, Vector3 threatVel)
    {
        // TODO 7c (Part E): flee from the predicted position. One line.
        return Vector3.zero;
    }

    // WANDER (Expert E1): seek a point that drifts slowly round a circle in front of the NPC.
    public Vector3 Wander()
    {
        // Expert E1: see the lab instructions. Keep an angle field, nudge it by a small random amount each frame,
        // put a point on a circle in front of the NPC, store it in LastWanderPoint and return Seek(LastWanderPoint).
        return Vector3.zero;
    }

    // ---------- Provided: turn a steering force into movement ----------
    public void Apply(Vector3 steering)
    {
        steering = Vector3.ClampMagnitude(Flat(steering), maxForce);                         // limit the force
        Vector3 acceleration = steering / mass;                                               // Reynolds: a = F / m
        velocity = Vector3.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxSpeed); // velocity += a x dt
        transform.position += velocity * Time.deltaTime;                                     // position += velocity x dt
        if (velocity.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(velocity);
    }

    public static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }
}

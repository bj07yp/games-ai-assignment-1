using UnityEngine;

// CET2000 Lab 08 - the green target in the Steering arena (provided, no changes needed).
// Either stands still (left-click moves it) or runs round an oval so you can test Pursuit and Evade.
public class TargetMover : MonoBehaviour
{
    public bool moving = false;
    public Vector2 ovalRadius = new Vector2(7f, 4.5f);
    public float speed = 2.5f;                 // metres per second along the oval

    public Vector3 Velocity { get; private set; }   // read by Pursuit and Evade

    float angle;
    Vector3 lastPosition;

    void Start() { lastPosition = transform.position; }

    void Update()
    {
        if (moving)
        {
            float perimeter = Mathf.PI * (ovalRadius.x + ovalRadius.y);
            angle += speed / perimeter * Mathf.PI * 2f * Time.deltaTime;
            transform.position = new Vector3(Mathf.Cos(angle) * ovalRadius.x, transform.position.y, Mathf.Sin(angle) * ovalRadius.y);
        }
        Vector3 v = (transform.position - lastPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        v.y = 0f;
        Velocity = v;
        lastPosition = transform.position;
    }

    public void PlaceAt(Vector3 point)
    {
        moving = false;
        transform.position = new Vector3(point.x, transform.position.y, point.z);
        lastPosition = transform.position;
        Velocity = Vector3.zero;
    }

    public void StartMoving()
    {
        // start on the oval at the point nearest to where the target is now
        angle = Mathf.Atan2(transform.position.z / ovalRadius.y, transform.position.x / ovalRadius.x);
        moving = true;
    }
}

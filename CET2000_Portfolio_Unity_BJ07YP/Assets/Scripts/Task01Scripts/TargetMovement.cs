using UnityEngine;

// A Kinematic body gets no OnCollisionEnter2D calls from static walls, so instead of waiting for a
// collision it looks ahead with a CircleCast each step and reflects its direction before it hits.
[RequireComponent(typeof(Rigidbody2D))]
public class TargetMovement_Template : MonoBehaviour
{
    public float moveSpeed = 1.5f; // how fast the target moves per second
    public bool moveTarget = true;  // if this was false the target would stay stationary
    public LayerMask obstacleLayer; 

    private Rigidbody2D rb;
    private Vector2 direction;
    private float radius = 0.2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); //initialises the targets rigidbody
        rb.bodyType = RigidbodyType2D.Kinematic; //sets the targets rigidbody body type to kinematic
        CircleCollider2D circle = GetComponent<CircleCollider2D>(); //links to the targets circlecollider
        if (circle != null) radius = circle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
    }

    void Start()
    {
        direction = Random.insideUnitCircle.normalized; // allows the target to start moving in a random direction each time the scene is started in game mode
        if (direction == Vector2.zero) direction = Vector2.right; // makes sure the target still moves if its random direction returns a 0,0
    }

    void FixedUpdate()
    {
        if (!moveTarget) return;
        float step = moveSpeed * Time.fixedDeltaTime;
        RaycastHit2D hit = Physics2D.CircleCast(rb.position, radius, direction, step + 0.05f, obstacleLayer);
        if (hit.collider != null)
        {
            direction = Vector2.Reflect(direction, hit.normal).normalized;   // makes the target 'bounce' off of any colliders it hits 
            return;
        }
        rb.MovePosition(rb.position + direction * step);
    }
}
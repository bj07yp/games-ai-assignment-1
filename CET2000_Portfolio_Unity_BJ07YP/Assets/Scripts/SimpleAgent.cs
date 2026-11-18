using UnityEngine;
// Hides "field is never used" warnings until you complete the TODOs.
#pragma warning disable 0169, 0414

// The agent runs the loop SENSE -> ACT once per physics step (FixedUpdate).
[RequireComponent(typeof(Rigidbody2D))]
public class SimpleAgent : MonoBehaviour
{
    [Header("Agent Settings")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 180f;          // degrees per second
    public float visionRange = 5f;
    public LayerMask targetLayer;               // set to TargetLayer (the CET2000 menu does this for you)
    public LayerMask obstacleLayer;             // set to ObstacleLayer

    [Header("Links")]
    public PerformanceLogger logger;   

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Physics queries and Rigidbody2D movement belong in FixedUpdate, not Update.
    void FixedUpdate()
    {
        Sense();
        Move();
    }

    // TODO 1: Implement sensing using raycasts (target & obstacle)
    void Sense()
    {
        // Shows the vision ray in the Scene view (turn on Gizmos to see it in the Game view too).
        Debug.DrawRay(transform.position, transform.up * visionRange, Color.red);


        // Example to start from (uncomment, then improve it):
        RaycastHit2D targetHit = Physics2D.Raycast(transform.position, transform.up, visionRange, targetLayer);
        if (targetHit.collider != null)
        {
            Debug.Log("Target detected");
            moveDirection = (targetHit.collider.transform.position - transform.position).normalized;      // chase
             //moveDirection = -(targetHit.collider.transform.position - transform.position).normalized;  // avoid
        }
        else
        {
            moveDirection = transform.up;                                                                 // keep going forward
        }
        
        RaycastHit2D obstacleHit = Physics2D.Raycast(transform.position, transform.up, 3f, obstacleLayer);
        if (obstacleHit.collider != null)
        {
            moveDirection = -(obstacleHit.collider.transform.position - transform.position).normalized; // turn to avoid
             //moveDirection = Quaternion.Euler(0, 0, 90) * transform.up;
        }
       
    }
       
        //Ideas: one forward ray only sees a thin line. Try several rays in a fan, or check the target's
        //distance first and then cast one ray straight at it.

    // TODO 2: Move the agent along moveDirection and rotate it to face the direction of travel
    void Move()
    {
        // Example to start from (uncomment):
        if (moveDirection == Vector2.zero) { rb.linearVelocity = Vector2.zero; return; }
        float targetAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;   // sprite faces +Y
        rb.MoveRotation(Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime));
        rb.linearVelocity = (Vector2)transform.up * moveSpeed;
    }

    // Called by Unity when the agent bumps into something.
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Obstacle"))
        {
            Debug.Log("Collision with " + col.collider.name);
            // TODO 3: count it, e.g.  if (logger != null) logger.IncrementCollisions();
        }
        else if (col.collider.CompareTag("Target"))
        {
            Debug.Log("Target reached!");
            // TODO 3: count it, e.g.  if (logger != null) logger.IncrementReached();
        }
    }
}
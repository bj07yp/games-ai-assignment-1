using UnityEngine;
// Hides "field is never used" warnings until you complete the TODOs.
#pragma warning disable 0169, 0414

// The agent runs the loop SENSE -> ACT once per physics step (FixedUpdate).
[RequireComponent(typeof(Rigidbody2D))]
public class SimpleAgent : MonoBehaviour
{
    [Header("Agent Settings")]
    public float moveSpeed = 2f; // how fast the agent moves
    public float rotationSpeed = 180f; // the degrees at which the agent can turn per second
    public float visionRange = 5f; // how far the agent can 'see' using a raycast
    public LayerMask targetLayer;               
    public LayerMask obstacleLayer;             

    [Header("Links")]
    public PerformanceLogger logger;   

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Sense(); //calls the Sense method
        Move();  //calls the Move method
    }

    void Sense()
    {
        // Shows the raycast visiblly in the game view to help track how it moves 
        Debug.DrawRay(transform.position, transform.up * visionRange, Color.red);


        
        RaycastHit2D targetHit = Physics2D.Raycast(transform.position, transform.up, visionRange, targetLayer);
        if (targetHit.collider != null)
        {
            Debug.Log("Target detected");
            //moveDirection = (targetHit.collider.transform.position - transform.position).normalized;      // this code snippet makes the agent chase the target when its raycast passes over it and the raycast follows where it goes allowsing the agent to follow
             moveDirection = -(targetHit.collider.transform.position - transform.position).normalized;  // this code snippet makes the agents raycast turn away from the targets collider when they overlap, making the agent 'avoid' the target
        }
        else
        {
            moveDirection = transform.up; 
        }
        
        RaycastHit2D obstacleHit = Physics2D.Raycast(transform.position, transform.up, 3f, obstacleLayer); // try circlecast to create a fan!!!!
        if (obstacleHit.collider != null)
        {
            //moveDirection = -(obstacleHit.collider.transform.position - transform.position).normalized; // this code snippet is the same as the above code that allows the agent to avoid the target, helping the agent turn away from objects it might collide with 
             moveDirection = Quaternion.Euler(0, 0, 90) * transform.up; 
        }


       
    }
       
        //Idea: one forward ray only sees a thin line. Try several rays in a fan, or check the target's
        //distance first and then cast one ray straight at it.

    // TODO 2: Move the agent along moveDirection and rotate it to face the direction of travel
    void Move()
    {
        if (moveDirection == Vector2.zero) { rb.linearVelocity = Vector2.zero; return; }
        float targetAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;   // sprite faces +Y
        rb.MoveRotation(Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime));
        rb.linearVelocity = (Vector2)transform.up * moveSpeed;
    }

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
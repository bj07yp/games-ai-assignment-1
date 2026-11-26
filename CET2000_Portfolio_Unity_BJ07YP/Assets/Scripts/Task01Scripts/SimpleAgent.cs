using UnityEngine;
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
        // This shows the raycast as a red line that helps to visibly track where the agent is looking.
        Debug.DrawRay(transform.position, transform.up * visionRange, Color.red);


        //this creates a raycast that checks if the agent sees the target and if it does, depending on which line of code is uncommented, it will either chase the target or avoid it.
        RaycastHit2D targetHit = Physics2D.Raycast(transform.position, transform.up, visionRange, targetLayer);
        if (targetHit.collider != null)
        {
            Debug.Log("Target detected");
            moveDirection = (targetHit.collider.transform.position - transform.position).normalized;      // this code snippet makes the agents raycast follow the target when they overlap, making the agent 'chase' the target
            // moveDirection = -(targetHit.collider.transform.position - transform.position).normalized;  // this code snippet makes the agents raycast turn away from the targets collider when they overlap, making the agent 'avoid' the target
        }
        else
        {
            moveDirection = transform.up; //keep moving forward if no target is detected
        }
        
        RaycastHit2D obstacleHit = Physics2D.Raycast(transform.position, transform.up, 3f, obstacleLayer);
        if (obstacleHit.collider != null)
        {
            //moveDirection = -(obstacleHit.collider.transform.position - transform.position).normalized; // this code snippet is the same as the above code that allows the agent to avoid the target, helping the agent turn away from objects it might collide with 
             moveDirection = Quaternion.Euler(0, 0, 90) * transform.up; 
        }


       
    }

    void Move() // this method is what allows the agent to move and rotate towards the target, or away from it depending on which line of code is uncommented in the Sense method
    {
        if (moveDirection == Vector2.zero) { rb.linearVelocity = Vector2.zero; return; }
        float targetAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;   // Calculates the angle to the target direction
        rb.MoveRotation(Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime));
        rb.linearVelocity = (Vector2)transform.up * moveSpeed;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Obstacle")) // when the agent collides with an obstacle.
        {
            Debug.Log("Collision with " + col.collider.name); 
        }
        else if (col.collider.CompareTag("Target")) // when the agent collides with the target.
        {
            Debug.Log("Target reached!");
        }
    }
}
using UnityEngine;

// CET2000 Lab 04 - moves the target around the arena and bounces it off walls and obstacles.
// (Ready to use; you don't need to change this file.)
// A Kinematic body gets no OnCollisionEnter2D calls from static walls, so instead of waiting for a
// collision it looks ahead with a CircleCast each step and reflects its direction before it hits.
[RequireComponent(typeof(Rigidbody2D))]
public class TargetMovement_Template : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public bool moveTarget = true;              // untick for a stationary target
    public LayerMask obstacleLayer;             // walls + obstacles (set by the CET2000 menu)

    private Rigidbody2D rb;
    private Vector2 direction;
    private float radius = 0.2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        CircleCollider2D circle = GetComponent<CircleCollider2D>();
        if (circle != null) radius = circle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
    }

    void Start()
    {
        direction = Random.insideUnitCircle.normalized;
        if (direction == Vector2.zero) direction = Vector2.right;
    }

    void FixedUpdate()
    {
        if (!moveTarget) return;
        float step = moveSpeed * Time.fixedDeltaTime;
        RaycastHit2D hit = Physics2D.CircleCast(rb.position, radius, direction, step + 0.05f, obstacleLayer);
        if (hit.collider != null)
        {
            direction = Vector2.Reflect(direction, hit.normal).normalized;   // bounce
            return;
        }
        rb.MovePosition(rb.position + direction * step);
    }
}
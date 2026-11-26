using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyBirdAI : MonoBehaviour
{
    [Header("References")]
    public EnemyZone enemyZone;

    [Header("Movement Settings")]
    public float chaseSpeed = 8f;
    public float returnSpeed = 6f;
    public float rotationSpeed = 5f;

    private Rigidbody rb;
    private Vector3 homePosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // Save the bird's starting position
        homePosition = transform.position;
    }

    void FixedUpdate()
    {
        if (enemyZone != null &&
            enemyZone.playerInside &&
            enemyZone.player != null)
        {
            Chase(enemyZone.player.position, chaseSpeed);
        }
        else
        {
            ReturnHome();
        }
    }

    void Chase(Vector3 targetPosition, float speed)
    {
        Vector3 direction =
            targetPosition - transform.position;

        // Stop when very close to the target
        if (direction.magnitude < 1f)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        direction.Normalize();

        // Move toward the target
        rb.linearVelocity = direction * speed;

        // Turn toward movement direction
        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );
    }

    void ReturnHome()
    {
        Vector3 direction =
            homePosition - transform.position;

        // Bird is home
        if (direction.magnitude < 0.5f)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        direction.Normalize();

        rb.linearVelocity = direction * returnSpeed;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );
    }
}
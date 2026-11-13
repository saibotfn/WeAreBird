using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TestPlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float verticalSpeed = 7f;
    public float rotationSpeed = 120f;

    private Rigidbody rb;
    private Vector3 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void Update()
    {
        // Move forward and backward
        float forward = Input.GetAxis("Vertical");

        // Turn left and right
        float turn = Input.GetAxis("Horizontal");

        // Rotate the player
        transform.Rotate(
            Vector3.up,
            turn * rotationSpeed * Time.deltaTime
        );

        // Move up and down
        float vertical = 0f;

        if (Input.GetKey(KeyCode.Space))
            vertical = 1f;

        if (Input.GetKey(KeyCode.LeftControl))
            vertical = -1f;

        // Calculate movement direction
        movement =
            transform.forward * forward * moveSpeed +
            Vector3.up * vertical * verticalSpeed;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movement;
    }
}
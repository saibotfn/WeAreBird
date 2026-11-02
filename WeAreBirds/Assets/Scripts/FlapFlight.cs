using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FlapFlight : MonoBehaviour
{
    [SerializeField] Transform leftHand;
    [SerializeField] Transform rightHand;
    [SerializeField] float minFlapSpeed = 1.0f;
    [SerializeField] float flapStrength = 6.0f;
    [SerializeField] float maxSpeed = 15f;
    [SerializeField] float glideSpreadDistance = 0.5f; // hands this far apart = wings out
    [SerializeField] float glideSinkRate = 0.6f;   // m/s downward while gliding
    [SerializeField] float glideResponse = 3f;     // how quickly we settle to that sink rate
    [SerializeField] float normalDamping = 0.5f;
    [SerializeField] float glideDamping = 0.05f;   // much less drag while wings are out
    [SerializeField] float glideFullSpeed = 4f;
    [SerializeField] Transform head;              // the Main Camera
    [SerializeField] float turnSpeed = 90f;       // degrees per second at full tilt
    [SerializeField] float bankDeadzone = 0.08f;  // meters of height difference we ignore
    [SerializeField] float glideForwardAccel = 2.5f; // m/s² of forward push while gliding
    [SerializeField] float glideCruiseSpeed = 8f;    // the push stops once horizontal speed reaches this

    Rigidbody rb;
    Vector3 prevLeft;
    Vector3 prevRight;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearDamping = normalDamping;
    }

    void Start()
    {
        prevLeft = transform.InverseTransformPoint(leftHand.position);
        prevRight = transform.InverseTransformPoint(rightHand.position);
    }

    void Update()
    {
        Vector3 curLeft = transform.InverseTransformPoint(leftHand.position);
        Vector3 curRight = transform.InverseTransformPoint(rightHand.position);

        Vector3 leftVel = (curLeft - prevLeft) / Time.deltaTime;
        Vector3 rightVel = (curRight - prevRight) / Time.deltaTime;

        bool flapped = false;
        flapped |= ApplyFlap(leftVel);
        flapped |= ApplyFlap(rightVel);

        float spread = Vector3.Distance(curLeft, curRight);
        bool gliding = !flapped && spread > glideSpreadDistance;

        rb.useGravity = !gliding;
        rb.linearDamping = gliding ? glideDamping : normalDamping;

        if (gliding)
        {
            ApplyGlide();
            ApplyBank(curLeft, curRight);
        }

        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

        prevLeft = curLeft;
        prevRight = curRight;
    }

    bool ApplyFlap(Vector3 localVel)
    {
        if (localVel.y > -minFlapSpeed) return false;

        Vector3 worldVel = transform.TransformDirection(localVel);
        rb.AddForce(-worldVel * flapStrength * Time.deltaTime, ForceMode.VelocityChange);
        return true;
    }

    void ApplyGlide()
    {
        Vector3 v = rb.linearVelocity;
        float horizontalSpeed = new Vector3(v.x, 0f, v.z).magnitude;
        float speedFactor = Mathf.Clamp01(horizontalSpeed / glideFullSpeed);

        // gentle sink, as before
        float targetY = -glideSinkRate;
        if (v.y < targetY)
            v.y = Mathf.Lerp(v.y, targetY, glideResponse * speedFactor * Time.deltaTime);

        // forward push along the direction you're looking, flattened so looking up or down doesn't change it
        if (horizontalSpeed < glideCruiseSpeed)
        {
            Vector3 fwd = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            v += fwd * glideForwardAccel * Time.deltaTime;
        }

        rb.linearVelocity = v;
    }

    void ApplyBank(Vector3 left, Vector3 right)
    {
        float heightDiff = left.y - right.y; // right hand higher = negative = turn left

        if (Mathf.Abs(heightDiff) < bankDeadzone) return;

        float yaw = heightDiff * turnSpeed * Time.deltaTime;
        transform.RotateAround(head.position, Vector3.up, yaw);

        // keep the velocity pointing where we're now facing
        rb.linearVelocity = Quaternion.AngleAxis(yaw, Vector3.up) * rb.linearVelocity;
    }
}
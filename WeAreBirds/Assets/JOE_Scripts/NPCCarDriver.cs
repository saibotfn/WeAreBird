using UnityEngine;

/// <summary>
/// Simple NPC car that drives along a loop of waypoints.
/// - Steers smoothly toward the next waypoint
/// - Slows down for sharp turns
/// - Brakes if something is in front of it (raycast)
///
/// Setup:
/// 1. Put this on your car GameObject (car's forward = blue Z axis).
/// 2. Make an empty GameObject "Route" with child empties as waypoints, in driving order.
/// 3. Drag "Route" into the Route field. All children become waypoints automatically.
/// </summary>
public class NPCCarDriver : MonoBehaviour
{
    [Header("Route")]
    public Transform route;                 // parent object holding waypoints as children
    public bool loop = true;
    public bool startAtFirstWaypoint = true;   // true = always start at the first child (WP1)
    public float waypointReachDistance = 4f;

    [Header("Driving")]
    public float maxSpeed = 12f;            // m/s (~43 km/h)
    public float cornerSpeed = 5f;          // speed in sharp turns
    public float acceleration = 4f;
    public float brakeForce = 10f;
    public float turnSpeed = 90f;           // degrees per second

    [Header("Obstacle Detection")]
    public float lookAheadDistance = 8f;
    public LayerMask obstacleLayers = ~0;
    public Vector3 sensorOffset = new Vector3(0f, 0.5f, 2f);

    Transform[] waypoints;
    int currentIndex;
    float currentSpeed;
    bool finished;

    void Start()
    {
        if (route == null)
        {
            Debug.LogWarning($"{name}: No route assigned.");
            enabled = false;
            return;
        }

        waypoints = new Transform[route.childCount];
        for (int i = 0; i < route.childCount; i++)
            waypoints[i] = route.GetChild(i);

        currentIndex = startAtFirstWaypoint ? 0 : GetClosestWaypoint();
    }

    void Update()
    {
        if (finished || waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        // Reached waypoint? Go to next one.
        if (toTarget.magnitude < waypointReachDistance)
        {
            currentIndex++;
            if (currentIndex >= waypoints.Length)
            {
                if (loop) currentIndex = 0;
                else { finished = true; return; }
            }
            return;
        }

        // Steering
        Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRot, turnSpeed * Time.deltaTime);

        // Target speed: slow down the sharper the turn ahead
        float angle = Vector3.Angle(transform.forward, toTarget);
        float desiredSpeed = Mathf.Lerp(maxSpeed, cornerSpeed, angle / 90f);

        // Anti-circling: if the waypoint is off to the side, slow down enough
        // that the turning circle is small enough to actually reach it
        if (angle > 20f)
        {
            float maxSpeedToReach = turnSpeed * Mathf.Deg2Rad * toTarget.magnitude * 0.5f;
            desiredSpeed = Mathf.Min(desiredSpeed, Mathf.Max(maxSpeedToReach, 1.5f));
        }

        // Brake for obstacles in front
        Vector3 sensorPos = transform.TransformPoint(sensorOffset);
        if (Physics.Raycast(sensorPos, transform.forward, out RaycastHit hit,
                            lookAheadDistance, obstacleLayers, QueryTriggerInteraction.Ignore)
            && hit.transform.root != transform.root)
        {
            // Closer obstacle = slower, full stop when very close
            desiredSpeed = Mathf.Lerp(0f, desiredSpeed, (hit.distance - 2f) / lookAheadDistance);
        }

        // Accelerate / brake toward desired speed
        float rate = desiredSpeed > currentSpeed ? acceleration : brakeForce;
        currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, rate * Time.deltaTime);

        transform.position += transform.forward * currentSpeed * Time.deltaTime;
    }

    int GetClosestWaypoint()
    {
        int closest = 0;
        float best = float.MaxValue;
        for (int i = 0; i < waypoints.Length; i++)
        {
            float d = (waypoints[i].position - transform.position).sqrMagnitude;
            if (d < best) { best = d; closest = i; }
        }
        return closest;
    }

    // Draws the route and sensor in the Scene view
    void OnDrawGizmos()
    {
        if (route != null && route.childCount > 1)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < route.childCount; i++)
            {
                Vector3 a = route.GetChild(i).position;
                Gizmos.DrawSphere(a, 0.4f);
                int next = i + 1;
                if (next < route.childCount) Gizmos.DrawLine(a, route.GetChild(next).position);
                else if (loop) Gizmos.DrawLine(a, route.GetChild(0).position);
            }
        }

        Gizmos.color = Color.red;
        Vector3 s = transform.TransformPoint(sensorOffset);
        Gizmos.DrawLine(s, s + transform.forward * lookAheadDistance);
    }
}
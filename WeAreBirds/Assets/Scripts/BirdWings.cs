using UnityEngine;

public class BirdWings : MonoBehaviour
{
    [Header("Body follows head")]
    public Transform head;                       // Main Camera
    public Vector3 bodyOffset = new Vector3(0f, -0.4f, -0.1f);   // tweak this

    [Header("Left wing")]
    public Transform leftShoulder, leftTip, leftHand;
    [Header("Right wing")]
    public Transform rightShoulder, rightTip, rightHand;

    Quaternion leftRest, rightRest;

    void Start()
    {
        leftRest = leftShoulder.localRotation;
        rightRest = rightShoulder.localRotation;
    }

    void LateUpdate()
    {
        // body: follow head position, only copy the yaw (left/right turn)
        Quaternion yaw = Quaternion.Euler(0f, head.eulerAngles.y, 0f);
        //transform.SetPositionAndRotation(head.position + yaw * bodyOffset, yaw);

        Aim(leftShoulder, leftTip, leftHand, leftRest);
        Aim(rightShoulder, rightTip, rightHand, rightRest);
    }

    void Aim(Transform shoulder, Transform tip, Transform hand, Quaternion rest)
    {
        shoulder.localRotation = rest;   // reset so roll can't build up
        Vector3 current = tip.position - shoulder.position;
        Vector3 desired = hand.position - shoulder.position;
        if (current.sqrMagnitude < 0.0001f || desired.sqrMagnitude < 0.0001f) return;
        shoulder.rotation = Quaternion.FromToRotation(current, desired) * shoulder.rotation;
    }
}
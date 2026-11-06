using UnityEngine;

public class BirdWings : MonoBehaviour
{
    public Transform leftShoulder, leftTip, leftHand;     // Wing.Left, Wing.Left.002, Left Controller
    public Transform rightShoulder, rightTip, rightHand;  // Wing.Right, Wing.Right.002, Right Controller

    void LateUpdate()
    {
        Aim(leftShoulder, leftTip, leftHand);
        Aim(rightShoulder, rightTip, rightHand);
    }

    void Aim(Transform shoulder, Transform tip, Transform hand)
    {
        Vector3 current = tip.position - shoulder.position;
        Vector3 desired = hand.position - shoulder.position;
        if (current.sqrMagnitude < 0.0001f || desired.sqrMagnitude < 0.0001f) return;
        shoulder.rotation = Quaternion.FromToRotation(current, desired) * shoulder.rotation;
    }
}
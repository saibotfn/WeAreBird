using UnityEngine;

namespace AlderBird
{
    /// <summary>
    /// Animation and optional head aim only. Move this GameObject from your own
    /// locomotion/AI code. No input-system, XR, navigation or rigging package required.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BirdController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Transform head;
        [SerializeField] private Vector3 headAimAxis = Vector3.forward;
        [SerializeField] private Transform lookTarget;
        [SerializeField, Range(0f, 1f)] private float lookWeight;
        [SerializeField, Range(0f, 70f)] private float maximumYaw = 45f;
        [SerializeField, Range(0f, 45f)] private float maximumPitch = 25f;
        [SerializeField, Min(0f)] private float lookResponse = 8f;
        private float currentLookWeight;

        public Animator Animator => animator;
        public Transform LookTarget { get => lookTarget; set => lookTarget = value; }
        public float LookWeight { get => lookWeight; set => lookWeight = Mathf.Clamp01(value); }

        public void Configure(Animator value, Transform headBone, Vector3 localAim)
        {
            animator = value;
            head = headBone;
            headAimAxis = localAim.normalized;
        }

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (animator != null) animator.applyRootMotion = false;
        }

        // Each command cancels stale triggers, so rapid interaction cannot queue
        // an old takeoff or hop to fire after a later landing request.
        private void Command(string trigger)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;
            foreach (string name in Commands) animator.ResetTrigger(name);
            animator.SetTrigger(trigger);
        }
        private static readonly string[] Commands =
            { "Idle", "Takeoff", "Land", "Hop", "LookAround", "WingFold" };

        public void Idle() => Command("Idle");
        public void TakeOff() => Command("Takeoff");
        public void Land() => Command("Land");
        public void Hop() => Command("Hop");
        public void LookAround() => Command("LookAround");
        public void FoldWings() => Command("WingFold");
        public void SetGliding(bool enabled)
        {
            if (animator != null) animator.SetBool("Gliding", enabled);
        }
        public void LookAt(Transform target, float weight = 1f)
        {
            lookTarget = target;
            lookWeight = Mathf.Clamp01(weight);
        }
        public void ClearLookTarget() { lookTarget = null; lookWeight = 0f; }

        private void LateUpdate()
        {
            if (head == null || animator == null || !animator.isActiveAndEnabled) return;
            float desiredWeight = lookTarget != null ? lookWeight : 0f;
            currentLookWeight = Mathf.Lerp(currentLookWeight, desiredWeight,
                1f - Mathf.Exp(-lookResponse * Time.deltaTime));
            if (lookTarget == null || currentLookWeight < 0.001f) return;
            Vector3 toTarget = lookTarget.position - head.position;
            if (toTarget.sqrMagnitude < 0.0001f) return;
            Vector3 local = transform.InverseTransformDirection(toTarget.normalized);
            float yaw = Mathf.Clamp(Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg,
                -maximumYaw, maximumYaw);
            float pitch = Mathf.Clamp(-Mathf.Atan2(local.y,
                Mathf.Sqrt(local.x * local.x + local.z * local.z)) * Mathf.Rad2Deg,
                -maximumPitch, maximumPitch);
            Vector3 targetDirection = transform.rotation *
                (Quaternion.Euler(pitch, yaw, 0f) * Vector3.forward);
            Vector3 animatedDirection = head.TransformDirection(headAimAxis);
            Quaternion aim = Quaternion.FromToRotation(animatedDirection, targetDirection);
            // The Animator writes a complete Head rotation every frame before this pass.
            head.rotation = Quaternion.Slerp(head.rotation, aim * head.rotation, currentLookWeight);
        }
    }
}

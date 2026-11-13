using UnityEngine;
using UnityEngine.InputSystem;

namespace AlderBird
{
    [RequireComponent(typeof(BirdController))]
    public sealed class BirdFlightController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 4f;
        [SerializeField, Min(0f)] private float verticalSpeed = 3f;
        [SerializeField, Min(0f)] private float turnSpeed = 8f;

        private BirdController bird;
        private bool hasStartedFlight;

        private void Awake()
        {
            bird = GetComponent<BirdController>();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            float horizontal = 0f;
            float forward = 0f;
            if (keyboard.aKey.isPressed) horizontal -= 1f;
            if (keyboard.dKey.isPressed) horizontal += 1f;
            if (keyboard.sKey.isPressed) forward -= 1f;
            if (keyboard.wKey.isPressed) forward += 1f;

            Vector3 direction = Vector3.ClampMagnitude(
                transform.right * horizontal + transform.forward * forward, 1f);
            Vector3 movement = direction * moveSpeed;

            if (keyboard.spaceKey.isPressed)
                movement += Vector3.up * verticalSpeed;

            if (movement.sqrMagnitude > 0f)
            {
                if (!hasStartedFlight)
                {
                    bird.TakeOff();
                    hasStartedFlight = true;
                }

                bird.SetGliding(true);
                transform.position += movement * Time.deltaTime;
            }

            if (direction.sqrMagnitude > 0f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    1f - Mathf.Exp(-turnSpeed * Time.deltaTime));
            }
        }
    }
}

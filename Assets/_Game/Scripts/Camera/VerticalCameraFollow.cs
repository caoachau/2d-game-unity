using UnityEngine;

namespace Summit.Game.Camera
{
    public sealed class VerticalCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 framingOffset = new(0f, 2.25f);
        [SerializeField, Min(0.01f)] private float riseSmoothTime = 0.22f;
        [SerializeField, Min(0.01f)] private float fallSmoothTime = 0.09f;
        [SerializeField] private bool lockHorizontal;

        private Vector3 velocity;
        private Vector3 shakeOffset;
        private float shakeRemaining;
        private float shakeStrength;
        private const float ShakeDuration = .18f;

        public void Shake(float strength)
        {
            shakeStrength = Mathf.Max(shakeStrength, strength);
            shakeRemaining = ShakeDuration;
        }

        public void CancelShake()
        {
            transform.position -= shakeOffset;
            shakeOffset = Vector3.zero;
            shakeRemaining = shakeStrength = 0f;
        }

        public void Initialize(Transform followTarget)
        {
            target = followTarget;
            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (target == null || Time.timeScale <= 0f)
            {
                return;
            }

            Vector3 position = transform.position - shakeOffset;
            float desiredX = lockHorizontal ? position.x : target.position.x + framingOffset.x;
            float desiredY = target.position.y + framingOffset.y;
            float smoothTime = desiredY < position.y ? fallSmoothTime : riseSmoothTime;
            Vector3 desired = new(desiredX, desiredY, position.z);
            shakeRemaining = Mathf.Max(0f, shakeRemaining - Time.deltaTime);
            shakeOffset = shakeRemaining > 0f
                ? (Vector3)(Random.insideUnitCircle * (shakeStrength * shakeRemaining / ShakeDuration))
                : Vector3.zero;
            if (shakeRemaining <= 0f) shakeStrength = 0f;
            transform.position = Vector3.SmoothDamp(position, desired, ref velocity, smoothTime) + shakeOffset;
        }

        public void SnapToTarget()
        {
            if (target == null)
            {
                return;
            }

            transform.position = new Vector3(target.position.x + framingOffset.x,
                target.position.y + framingOffset.y, transform.position.z);
            velocity = Vector3.zero;
            shakeOffset = Vector3.zero;
            shakeRemaining = shakeStrength = 0f;
        }
    }
}

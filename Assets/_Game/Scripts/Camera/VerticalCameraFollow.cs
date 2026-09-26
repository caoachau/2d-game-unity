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

        public void Initialize(Transform followTarget)
        {
            target = followTarget;
            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 position = transform.position;
            float desiredX = lockHorizontal ? position.x : target.position.x + framingOffset.x;
            float desiredY = target.position.y + framingOffset.y;
            float smoothTime = desiredY < position.y ? fallSmoothTime : riseSmoothTime;
            Vector3 desired = new(desiredX, desiredY, position.z);
            transform.position = Vector3.SmoothDamp(position, desired, ref velocity, smoothTime);
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
        }
    }
}

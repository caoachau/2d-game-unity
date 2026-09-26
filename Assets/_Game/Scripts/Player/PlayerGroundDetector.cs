using Summit.Game.Configuration;
using UnityEngine;

namespace Summit.Game.Player
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class PlayerGroundDetector : MonoBehaviour
    {
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private LayerMask groundLayers = ~0;

        private readonly RaycastHit2D[] hits = new RaycastHit2D[8];
        private ContactFilter2D contactFilter;
        private PlayerMovementConfig config;
        private float lastGroundedTime = float.NegativeInfinity;

        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            if (bodyCollider == null)
            {
                bodyCollider = GetComponent<Collider2D>();
            }
            RebuildFilter();
        }

        public void Initialize(PlayerMovementConfig movementConfig)
        {
            config = movementConfig;
            RebuildFilter();
        }

        public bool Refresh()
        {
            if (bodyCollider == null || config == null)
            {
                IsGrounded = false;
                return false;
            }

            int count = bodyCollider.Cast(Vector2.down, contactFilter, hits, config.GroundCheckDistance);
            bool touchingGround = false;
            for (int i = 0; i < count; i++)
            {
                if (hits[i].normal.y >= config.MinimumGroundNormal)
                {
                    touchingGround = true;
                    break;
                }
            }

            if (touchingGround)
            {
                lastGroundedTime = Time.time;
            }

            IsGrounded = touchingGround || Time.time - lastGroundedTime <= config.GroundGraceDuration;
            return IsGrounded;
        }

        private void RebuildFilter()
        {
            contactFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = groundLayers,
                useTriggers = false
            };
        }

        private void OnDrawGizmosSelected()
        {
            if (bodyCollider == null)
            {
                bodyCollider = GetComponent<Collider2D>();
            }

            if (bodyCollider == null)
            {
                return;
            }

            float distance = config != null ? config.GroundCheckDistance : 0.08f;
            Bounds bounds = bodyCollider.bounds;
            Gizmos.color = IsGrounded ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(new Vector3(bounds.center.x, bounds.min.y - distance * 0.5f),
                new Vector3(bounds.size.x * 0.9f, distance, 0f));
        }
    }
}

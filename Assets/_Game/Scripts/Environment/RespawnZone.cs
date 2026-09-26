using UnityEngine;

namespace Summit.Game.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class RespawnZone : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Rigidbody2D targetBody;
        [SerializeField, Min(0f)] private float respawnCooldown = 0.25f;

        private float nextRespawnTime;

        public void Initialize(Transform respawnPoint, Rigidbody2D body = null)
        {
            spawnPoint = respawnPoint;
            targetBody = body;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Rigidbody2D body = other.attachedRigidbody;
            if (spawnPoint == null || body == null ||
                targetBody != null && body != targetBody ||
                Time.unscaledTime < nextRespawnTime)
            {
                return;
            }

            nextRespawnTime = Time.unscaledTime + respawnCooldown;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.position = spawnPoint.position;
            body.transform.position = spawnPoint.position;

            // Rigidbody interpolation can otherwise expose the old position for
            // another frame and make the bottom trigger fire repeatedly.
            Physics2D.SyncTransforms();
        }
    }
}

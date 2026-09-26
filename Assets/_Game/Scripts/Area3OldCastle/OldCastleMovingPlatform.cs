using UnityEngine;

namespace Summit.Game.Area3OldCastle
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class OldCastleMovingPlatform : MonoBehaviour
    {
        [SerializeField] private Vector2 travelOffset = new(3f, 0f);
        [SerializeField, Min(0.05f)] private float cycleSeconds = 3.4f;
        [SerializeField] private float phase;
        private Rigidbody2D body;
        private Vector2 origin;

        public void Configure(Vector2 offset, float seconds, float normalizedPhase = 0f)
        {
            travelOffset = offset;
            cycleSeconds = Mathf.Max(0.05f, seconds);
            phase = normalizedPhase;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            origin = body.position;
        }

        private void FixedUpdate()
        {
            if (body == null) return;
            float normalized = Mathf.Repeat(Time.time / cycleSeconds + phase, 1f);
            float pingPong = 0.5f - 0.5f * Mathf.Cos(normalized * Mathf.PI * 2f);
            body.MovePosition(origin + travelOffset * pingPong);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.65f, 0.2f, 0.9f);
            Vector3 start = UnityEngine.Application.isPlaying && body != null ? body.position : transform.position;
            Gizmos.DrawLine(start, start + (Vector3)travelOffset);
            Gizmos.DrawWireSphere(start + (Vector3)travelOffset, 0.18f);
        }
    }
}

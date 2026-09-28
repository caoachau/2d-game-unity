using Summit.Game.Player;
using UnityEngine;

namespace Summit.Game.Environment
{
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    public sealed class LevelHorizontalBounds : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] backgroundRenderers;

        private Rigidbody2D playerBody;
        private BoxCollider2D playerCollider;
        private float leftEdge;
        private float rightEdge;
        private bool hasBounds;

        private void Awake()
        {
            // Capture the authored map before camera follow moves any backgrounds
            // parented to the camera. Borders stay fixed for the whole scene.
            if (backgroundRenderers == null)
                return;

            foreach (SpriteRenderer background in backgroundRenderers)
            {
                if (background == null || background.sprite == null)
                    continue;

                Bounds bounds = background.bounds;
                leftEdge = hasBounds ? Mathf.Min(leftEdge, bounds.min.x) : bounds.min.x;
                rightEdge = hasBounds ? Mathf.Max(rightEdge, bounds.max.x) : bounds.max.x;
                hasBounds = true;
            }
        }

        private void Start()
        {
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            {
                PlayerMovement player = root.GetComponentInChildren<PlayerMovement>();
                if (player == null)
                    continue;

                playerBody = player.GetComponent<Rigidbody2D>();
                playerCollider = player.GetComponent<BoxCollider2D>();
                break;
            }

            ConfinePlayer(false);
        }

        private void FixedUpdate()
        {
            ConfinePlayer(true);
        }

        private void LateUpdate()
        {
            // Also contain respawns, teleports and displacement from contacts.
            ConfinePlayer(false);
        }

        private void ConfinePlayer(bool limitNextPhysicsStep)
        {
            if (!hasBounds || playerBody == null || playerCollider == null)
                return;

            Bounds colliderBounds = playerCollider.bounds;
            // Keep the entire scaled collider inside, including its offset.
            // Use a local offset so Rigidbody interpolation cannot change the
            // permitted physical position while its rendered position catches up.
            float centerOffsetX = playerCollider.transform.TransformVector(playerCollider.offset).x;
            float minimumX = leftEdge + colliderBounds.extents.x - centerOffsetX;
            float maximumX = rightEdge - colliderBounds.extents.x - centerOffsetX;
            if (minimumX > maximumX)
                minimumX = maximumX = (minimumX + maximumX) * 0.5f;

            Vector2 position = playerBody.position;
            float confinedX = Mathf.Clamp(position.x, minimumX, maximumX);
            if (!Mathf.Approximately(position.x, confinedX))
            {
                position.x = confinedX;
                playerBody.position = position;
                Vector3 transformPosition = playerBody.transform.position;
                transformPosition.x = confinedX;
                playerBody.transform.position = transformPosition;
            }

            Vector2 velocity = playerBody.linearVelocity;
            if ((confinedX <= minimumX && velocity.x < 0f) ||
                (confinedX >= maximumX && velocity.x > 0f))
            {
                velocity.x = 0f;
            }
            else if (limitNextPhysicsStep)
            {
                velocity.x = Mathf.Clamp(velocity.x,
                    (minimumX - confinedX) / Time.fixedDeltaTime,
                    (maximumX - confinedX) / Time.fixedDeltaTime);
            }

            playerBody.linearVelocity = velocity;
        }
    }
}

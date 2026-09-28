using Summit.Game.Configuration;
using Summit.Game.Domain;
using UnityEngine;

namespace Summit.Game.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D body;

        private PlayerMovementConfig config;
        private float moveInput;

        public Vector2 Velocity => body != null ? body.linearVelocity : Vector2.zero;
        public float MoveInput => moveInput;

        private void Awake()
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }
            body.freezeRotation = true;
        }

        public void Initialize(PlayerMovementConfig movementConfig)
        {
            config = movementConfig;
            body.gravityScale = config.GravityScale;
        }

        public void Launch(JumpVelocity jumpVelocity)
        {
            body.gravityScale = config.GravityScale;
            body.linearVelocity = new Vector2(jumpVelocity.Horizontal, jumpVelocity.Vertical);
        }

        public void SetMoveInput(float horizontalInput)
        {
            moveInput = Mathf.Clamp(horizontalInput, -1f, 1f);
        }

        public void SetClimbingVelocity(float verticalInput, float climbSpeed)
        {
            body.gravityScale = 0f;
            body.linearVelocity = new Vector2(0f, Mathf.Clamp(verticalInput, -1f, 1f) * climbSpeed);
        }

        public void StopClimbing()
        {
            if (config != null)
            {
                body.gravityScale = config.GravityScale;
            }

            body.linearVelocity = Vector2.zero;
        }

        public void TickPhysics(bool isGrounded)
        {
            if (config == null)
            {
                return;
            }

            Vector2 velocity = body.linearVelocity;

            if (isGrounded || Mathf.Abs(moveInput) > 0.01f)
            {
                float targetSpeed = moveInput * config.RunSpeed *
                                    (isGrounded ? 1f : config.AirControlMultiplier);
                float acceleration = Mathf.Abs(moveInput) > 0.01f
                    ? (isGrounded ? config.GroundAcceleration : config.AirAcceleration)
                    : config.GroundDeceleration;
                velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed,
                    acceleration * Time.fixedDeltaTime);
            }

            body.gravityScale = velocity.y < 0f
                ? config.GravityScale * config.FallGravityMultiplier
                : config.GravityScale;

            if (velocity.y < -config.MaxFallSpeed)
            {
                velocity.y = -config.MaxFallSpeed;
            }

            body.linearVelocity = velocity;
        }
    }
}

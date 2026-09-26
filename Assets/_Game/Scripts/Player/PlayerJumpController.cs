using System;
using Summit.Game.Configuration;
using Summit.Game.Domain;
using Summit.Game.Input;
using UnityEngine;

namespace Summit.Game.Player
{
    [RequireComponent(typeof(PlayerMovement), typeof(PlayerGroundDetector), typeof(PlayerStateController))]
    public sealed class PlayerJumpController : MonoBehaviour
    {
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PlayerGroundDetector groundDetector;
        [SerializeField] private PlayerStateController stateController;
        [SerializeField] private PlayerMovementConfig config;

        private IPlayerInput input;
        private float chargeDuration;
        private float landingUnlockTime;
        private float lastAirborneVerticalVelocity;
        private bool wasGrounded;
        private bool jumpReleasePending;

        public event Action<float> ChargeChanged;
        public event Action<JumpDirection, float> Jumped;
        public event Action<bool, float> Landed;

        public float NormalizedCharge => config == null
            ? 0f
            : JumpMath.NormalizeCharge(chargeDuration, config.MaxChargeDuration);

        private void Awake()
        {
            if (movement == null)
            {
                movement = GetComponent<PlayerMovement>();
            }

            if (groundDetector == null)
            {
                groundDetector = GetComponent<PlayerGroundDetector>();
            }

            if (stateController == null)
            {
                stateController = GetComponent<PlayerStateController>();
            }
        }

        public void Initialize(IPlayerInput playerInput, PlayerMovementConfig movementConfig = null)
        {
            input = playerInput;
            config = movementConfig != null ? movementConfig : config;

            if (config == null)
            {
                Debug.LogError("[PlayerJumpController] Missing PlayerMovementConfig.", this);
                enabled = false;
                return;
            }

            movement.Initialize(config);
            groundDetector.Initialize(config);
            wasGrounded = groundDetector.Refresh();
            stateController.TransitionTo(wasGrounded ? PlayerState.Grounded : PlayerState.Falling);
        }

        private void Update()
        {
            if (input == null || config == null)
            {
                return;
            }

            if (input.ConsumeJumpPressed() && CanStartCharging())
            {
                chargeDuration = 0f;
                stateController.TransitionTo(PlayerState.Charging);
                ChargeChanged?.Invoke(0f);
            }

            if (stateController.CurrentState == PlayerState.Charging && input.JumpHeld)
            {
                chargeDuration = Mathf.Min(chargeDuration + Time.deltaTime, config.MaxChargeDuration);
                ChargeChanged?.Invoke(NormalizedCharge);
            }

            if (input.ConsumeJumpReleased() && stateController.CurrentState == PlayerState.Charging)
            {
                jumpReleasePending = true;
            }
        }

        private void FixedUpdate()
        {
            if (config == null)
            {
                return;
            }

            bool grounded = groundDetector.Refresh();
            // A/D is kept for the launch direction while charging, but it must
            // not move the Rigidbody across the ground during the crouch.
            movement.SetMoveInput(stateController.CurrentState == PlayerState.Charging
                ? 0f
                : input.Horizontal);
            movement.TickPhysics(grounded);

            // Ground grace must never reinterpret the first airborne frames as a landing.
            if (stateController.CurrentState == PlayerState.Jumping && movement.Velocity.y > 0f)
            {
                grounded = false;
            }

            if (!grounded)
            {
                lastAirborneVerticalVelocity = movement.Velocity.y;
            }

            if (jumpReleasePending && grounded)
            {
                ExecuteJump();
                grounded = false;
            }
            else
            {
                UpdateAirAndLandingState(grounded);
            }

            wasGrounded = grounded;
        }

        private bool CanStartCharging()
        {
            PlayerState state = stateController.CurrentState;
            return groundDetector.IsGrounded &&
                   (state == PlayerState.Grounded ||
                    state == PlayerState.Landing && Time.time >= landingUnlockTime);
        }

        private void ExecuteJump()
        {
            float charge = NormalizedCharge;
            float evaluatedCharge = config.JumpChargeCurve.Evaluate(charge);
            JumpDirection direction = JumpMath.GetDirection(input.Horizontal);
            JumpVelocity velocity = JumpMath.Calculate(evaluatedCharge, config.MinJumpVelocity,
                config.MaxJumpVelocity, config.HorizontalJumpVelocity, direction);

            jumpReleasePending = false;
            chargeDuration = 0f;
            movement.Launch(velocity);
            stateController.TransitionTo(PlayerState.Jumping);
            ChargeChanged?.Invoke(0f);
            Jumped?.Invoke(direction, charge);
        }

        private void UpdateAirAndLandingState(bool grounded)
        {
            if (grounded && !wasGrounded)
            {
                float impactSpeed = Mathf.Abs(lastAirborneVerticalVelocity);
                bool hardLanding = impactSpeed >= config.HardLandingSpeed;
                landingUnlockTime = Time.time + config.LandingLockDuration;
                stateController.TransitionTo(PlayerState.Landing);
                Landed?.Invoke(hardLanding, impactSpeed);
                return;
            }

            if (grounded)
            {
                if (stateController.CurrentState == PlayerState.Landing && Time.time >= landingUnlockTime)
                {
                    stateController.TransitionTo(PlayerState.Grounded);
                }

                return;
            }

            stateController.TransitionTo(movement.Velocity.y > 0f ? PlayerState.Jumping : PlayerState.Falling);
        }
    }
}

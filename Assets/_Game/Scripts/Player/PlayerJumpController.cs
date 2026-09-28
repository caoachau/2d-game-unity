using System;
using System.Collections.Generic;
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
        [SerializeField, Min(0.1f)] private float climbSpeed = 3f;

        private IPlayerInput input;
        private float chargeDuration;
        private float landingUnlockTime;
        private float lastAirborneVerticalVelocity;
        private bool wasGrounded;
        private bool jumpReleasePending;
        private readonly HashSet<LadderClimbZone> ladders = new();
        private bool isClimbing;

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
            PlayerLevelGeometry.Configure(this);
            wasGrounded = groundDetector.Refresh();
            stateController.TransitionTo(wasGrounded ? PlayerState.Grounded : PlayerState.Falling);
        }

        public void CancelCharge()
        {
            if (stateController.CurrentState != PlayerState.Charging) return;
            chargeDuration = 0f;
            jumpReleasePending = false;
            stateController.TransitionTo(groundDetector.IsGrounded ? PlayerState.Grounded : PlayerState.Falling);
            ChargeChanged?.Invoke(0f);
        }

        private void Update()
        {
            if (input == null || config == null || Time.timeScale <= 0f)
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
            if (config == null || input == null)
            {
                return;
            }

            ladders.RemoveWhere(zone => zone == null || !zone.isActiveAndEnabled);
            bool insideLadder = ladders.Count > 0;
            bool leavingSideways = Mathf.Abs(input.Horizontal) > .1f;
            bool canStartClimb = false;
            foreach (LadderClimbZone ladder in ladders) canStartClimb |= ladder.CanStartClimb;
            if (insideLadder && canStartClimb && !leavingSideways && input.Vertical > 0.1f && !isClimbing)
            {
                CancelCharge();
                isClimbing = true;
                chargeDuration = 0f;
                jumpReleasePending = false;
                ChargeChanged?.Invoke(0f);
            }

            if (isClimbing)
            {
                if (!insideLadder || leavingSideways)
                {
                    isClimbing = false;
                    movement.StopClimbing();
                }
                else
                {
                    movement.SetMoveInput(0f);
                    movement.SetClimbingVelocity(input != null ? input.Vertical : 0f, climbSpeed);
                    stateController.TransitionTo(PlayerState.Climbing);
                    wasGrounded = false;
                    return;
                }
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

        public void EnterLadder(LadderClimbZone zone)
        {
            if (zone != null && enabled) ladders.Add(zone);
        }

        public void ExitLadder(LadderClimbZone zone)
        {
            ladders.Remove(zone);
            if (ladders.Count == 0 && isClimbing)
            {
                isClimbing = false;
                movement.StopClimbing();
            }
        }

        private void OnDisable()
        {
            ladders.Clear();
            if (isClimbing && movement != null) movement.StopClimbing();
            isClimbing = false;
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

using UnityEngine;

namespace Summit.Game.Configuration
{
    [CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "Summit/Player Movement Config")]
    public sealed class PlayerMovementConfig : ScriptableObject
    {
        [Header("Charge Jump")]
        [SerializeField, Min(0f)] private float minJumpVelocity = 7f;
        [SerializeField, Min(0.01f)] private float maxJumpVelocity = 17f;
        [SerializeField, Min(0.01f)] private float maxChargeDuration = 1.15f;
        [SerializeField, Min(0f)] private float horizontalJumpVelocity = 6.5f;
        [SerializeField] private AnimationCurve jumpChargeCurve = new(
            new Keyframe(0f, 0f),
            new Keyframe(0.3f, 0.18f),
            new Keyframe(0.75f, 0.68f),
            new Keyframe(1f, 1f));

        [Header("Physics")]
        [SerializeField, Min(0f)] private float runSpeed = 6f;
        [SerializeField, Min(0f)] private float groundAcceleration = 45f;
        [SerializeField, Min(0f)] private float groundDeceleration = 55f;
        [SerializeField, Min(0f)] private float airAcceleration = 28f;
        [SerializeField, Min(0f)] private float gravityScale = 3.5f;
        [SerializeField, Min(1f)] private float fallGravityMultiplier = 1.45f;
        [SerializeField, Min(1f)] private float maxFallSpeed = 20f;
        [SerializeField, Range(0f, 1f)] private float airControlMultiplier;

        [Header("Grounding and Landing")]
        [SerializeField, Min(0.001f)] private float groundCheckDistance = 0.08f;
        [SerializeField, Range(0f, 1f)] private float minimumGroundNormal = 0.65f;
        [SerializeField, Min(0f)] private float groundGraceDuration = 0.04f;
        [SerializeField, Min(0f)] private float landingLockDuration = 0.08f;
        [SerializeField, Min(0f)] private float hardLandingSpeed = 12f;
        [SerializeField, Min(0f)] private float releaseBufferDuration = 0.08f;

        public float MinJumpVelocity => minJumpVelocity;
        public float MaxJumpVelocity => maxJumpVelocity;
        public float MaxChargeDuration => maxChargeDuration;
        public float HorizontalJumpVelocity => horizontalJumpVelocity;
        public AnimationCurve JumpChargeCurve => jumpChargeCurve;
        public float RunSpeed => runSpeed;
        public float GroundAcceleration => groundAcceleration;
        public float GroundDeceleration => groundDeceleration;
        public float AirAcceleration => airAcceleration;
        public float GravityScale => gravityScale;
        public float FallGravityMultiplier => fallGravityMultiplier;
        public float MaxFallSpeed => maxFallSpeed;
        public float AirControlMultiplier => airControlMultiplier;
        public float GroundCheckDistance => groundCheckDistance;
        public float MinimumGroundNormal => minimumGroundNormal;
        public float GroundGraceDuration => groundGraceDuration;
        public float LandingLockDuration => landingLockDuration;
        public float HardLandingSpeed => hardLandingSpeed;
        public float ReleaseBufferDuration => releaseBufferDuration;

        private void OnValidate()
        {
            maxJumpVelocity = Mathf.Max(minJumpVelocity, maxJumpVelocity);
            maxChargeDuration = Mathf.Max(0.01f, maxChargeDuration);
            maxFallSpeed = Mathf.Max(1f, maxFallSpeed);
        }
    }
}

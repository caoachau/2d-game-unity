namespace Summit.Game.Domain
{
    public readonly struct JumpVelocity
    {
        public JumpVelocity(float horizontal, float vertical)
        {
            Horizontal = horizontal;
            Vertical = vertical;
        }

        public float Horizontal { get; }
        public float Vertical { get; }
    }

    public static class JumpMath
    {
        public static float NormalizeCharge(float duration, float maximumDuration)
        {
            if (maximumDuration <= 0f)
            {
                return 1f;
            }

            float value = duration / maximumDuration;
            return value < 0f ? 0f : value > 1f ? 1f : value;
        }

        public static JumpVelocity Calculate(
            float evaluatedCharge,
            float minimumVerticalVelocity,
            float maximumVerticalVelocity,
            float horizontalVelocity,
            JumpDirection direction)
        {
            float charge = evaluatedCharge < 0f ? 0f : evaluatedCharge > 1f ? 1f : evaluatedCharge;
            float vertical = minimumVerticalVelocity +
                             (maximumVerticalVelocity - minimumVerticalVelocity) * charge;
            return new JumpVelocity(horizontalVelocity * (int)direction, vertical);
        }

        public static JumpDirection GetDirection(float horizontalInput, float deadZone = 0.25f)
        {
            if (horizontalInput < -deadZone)
            {
                return JumpDirection.Left;
            }

            return horizontalInput > deadZone ? JumpDirection.Right : JumpDirection.Neutral;
        }
    }
}

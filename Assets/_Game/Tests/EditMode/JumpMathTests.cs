using NUnit.Framework;
using Summit.Game.Domain;

namespace Summit.Game.Tests.EditMode
{
    public sealed class JumpMathTests
    {
        [TestCase(-1f, 1f, 0f)]
        [TestCase(0.5f, 1f, 0.5f)]
        [TestCase(2f, 1f, 1f)]
        [TestCase(1f, 0f, 1f)]
        public void NormalizeCharge_ClampsToUnitRange(float duration, float maximum, float expected)
        {
            Assert.That(JumpMath.NormalizeCharge(duration, maximum), Is.EqualTo(expected).Within(0.0001f));
        }

        [TestCase(-1f, JumpDirection.Left)]
        [TestCase(0f, JumpDirection.Neutral)]
        [TestCase(1f, JumpDirection.Right)]
        public void GetDirection_ReturnsDiscreteDirection(float input, JumpDirection expected)
        {
            Assert.That(JumpMath.GetDirection(input), Is.EqualTo(expected));
        }

        [Test]
        public void Calculate_UsesDirectionAndCharge()
        {
            JumpVelocity result = JumpMath.Calculate(0.5f, 6f, 14f, 5f, JumpDirection.Left);

            Assert.That(result.Horizontal, Is.EqualTo(-5f));
            Assert.That(result.Vertical, Is.EqualTo(10f));
        }
    }
}

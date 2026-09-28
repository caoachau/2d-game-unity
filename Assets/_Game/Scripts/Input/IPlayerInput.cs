namespace Summit.Game.Input
{
    public interface IPlayerInput
    {
        float Horizontal { get; }
        float Vertical { get; }
        bool JumpHeld { get; }
        bool ConsumeJumpPressed();
        bool ConsumeJumpReleased();
    }
}

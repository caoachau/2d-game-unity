namespace Summit.Game.Input
{
    public interface IPlayerInput
    {
        float Horizontal { get; }
        bool JumpHeld { get; }
        bool ConsumeJumpPressed();
        bool ConsumeJumpReleased();
    }
}

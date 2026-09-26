using System;

namespace Summit.Game.Services
{
    public interface IPauseService
    {
        event Action<bool> PauseChanged;
        bool IsPaused { get; }
        void Pause();
        void Resume();
        void Toggle();
    }
}

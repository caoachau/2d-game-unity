using System;
using UnityEngine;

namespace Summit.Game.Services
{
    public sealed class UnityPauseService : IPauseService
    {
        public event Action<bool> PauseChanged;

        public bool IsPaused { get; private set; }

        public void Pause()
        {
            SetPaused(true);
        }

        public void Resume()
        {
            SetPaused(false);
        }

        public void Toggle()
        {
            SetPaused(!IsPaused);
        }

        private void SetPaused(bool paused)
        {
            if (IsPaused == paused)
            {
                return;
            }

            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            AudioListener.pause = paused;
            PauseChanged?.Invoke(paused);
        }
    }
}

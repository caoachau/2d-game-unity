using System;
using Summit.Game.Domain;
using Summit.Game.Player;
using UnityEngine;

namespace Summit.Game.Application
{
    public sealed class GameRunTimer : MonoBehaviour
    {
        private PlayerJumpController jumpController;

        public event Action<float> TimeChanged;

        public float ElapsedSeconds { get; private set; }
        public bool IsRunning { get; private set; }

        public void Initialize(PlayerJumpController playerJumpController)
        {
            jumpController = playerJumpController;
            jumpController.Jumped += HandleFirstJump;
        }

        private void Update()
        {
            if (!IsRunning)
            {
                return;
            }

            ElapsedSeconds += Time.deltaTime;
            TimeChanged?.Invoke(ElapsedSeconds);
        }

        private void OnDestroy()
        {
            if (jumpController != null)
            {
                jumpController.Jumped -= HandleFirstJump;
            }
        }

        public void Stop()
        {
            IsRunning = false;
            TimeChanged?.Invoke(ElapsedSeconds);
        }

        private void HandleFirstJump(JumpDirection direction, float charge)
        {
            IsRunning = true;
            jumpController.Jumped -= HandleFirstJump;
        }
    }
}

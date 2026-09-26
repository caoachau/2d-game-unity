using System;
using Summit.Game.Domain;
using UnityEngine;

namespace Summit.Game.Player
{
    public sealed class PlayerStateController : MonoBehaviour
    {
        [SerializeField] private PlayerState currentState = PlayerState.Grounded;

        public event Action<PlayerState, PlayerState> StateChanged;

        public PlayerState CurrentState => currentState;

        public void TransitionTo(PlayerState nextState)
        {
            if (currentState == nextState)
            {
                return;
            }

            PlayerState previous = currentState;
            currentState = nextState;
            StateChanged?.Invoke(previous, nextState);
        }
    }
}

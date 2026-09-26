using System;
using UnityEngine;

namespace Summit.Game.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class GoalTrigger : MonoBehaviour
    {
        private bool reached;

        public event Action Reached;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (reached || other.attachedRigidbody == null)
            {
                return;
            }

            reached = true;
            Reached?.Invoke();
        }
    }
}

using System;
using UnityEngine;

namespace Summit.Game.Area3OldCastle
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class OldCastlePrincessGoal : MonoBehaviour
    {
        private bool completed;
        public event Action Reached;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (completed || other.attachedRigidbody == null) return;
            completed = true;
            Reached?.Invoke();
        }
    }
}

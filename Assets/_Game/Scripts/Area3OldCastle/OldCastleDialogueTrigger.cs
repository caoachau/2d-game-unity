using System;
using UnityEngine;

namespace Summit.Game.Area3OldCastle
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class OldCastleDialogueTrigger : MonoBehaviour
    {
        private bool triggered;
        public event Action Triggered;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered || other.attachedRigidbody == null) return;
            triggered = true;
            Triggered?.Invoke();
        }
    }
}

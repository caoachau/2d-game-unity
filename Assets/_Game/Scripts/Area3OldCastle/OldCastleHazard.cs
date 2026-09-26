using UnityEngine;

namespace Summit.Game.Area3OldCastle
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class OldCastleHazard : MonoBehaviour
    {
        [SerializeField] private OldCastleRespawnController respawnController;
        public void Configure(OldCastleRespawnController controller) => respawnController = controller;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (respawnController != null && other.attachedRigidbody != null)
            {
                respawnController.RespawnNow();
            }
        }
    }
}

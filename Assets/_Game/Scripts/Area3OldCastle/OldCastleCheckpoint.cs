using UnityEngine;

namespace Summit.Game.Area3OldCastle
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class OldCastleCheckpoint : MonoBehaviour
    {
        [SerializeField] private OldCastleRespawnController respawnController;
        [SerializeField] private int checkpointNumber;
        [SerializeField] private Vector3 spawnOffset = new(0f, 1.35f, 0f);
        [SerializeField] private SpriteRenderer markerRenderer;
        private bool activated;

        public void Configure(OldCastleRespawnController controller, int number, SpriteRenderer renderer)
        {
            respawnController = controller;
            checkpointNumber = number;
            markerRenderer = renderer;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (activated || respawnController == null || other.attachedRigidbody == null) return;

            activated = true;
            respawnController.SetCheckpoint(transform.position + spawnOffset);
            PlayerPrefs.SetInt("Area3OldCastle.LastCheckpoint", checkpointNumber);
            PlayerPrefs.Save();

            if (markerRenderer != null)
            {
                markerRenderer.color = new Color(0.65f, 0.90f, 1f, 1f);
            }
        }
    }
}

using UnityEngine;

namespace Summit.Game.Area2Cave
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class CaveCheckpoint : MonoBehaviour
    {
        [SerializeField] private CaveRespawnController respawnController;
        [SerializeField] private int checkpointNumber;
        [SerializeField] private Vector3 spawnOffset = new(0f, 1.2f, 0f);
        [SerializeField] private SpriteRenderer markerRenderer;
        private bool activated;

        public void Configure(CaveRespawnController controller, int number, SpriteRenderer renderer)
        {
            respawnController = controller;
            checkpointNumber = number;
            markerRenderer = renderer;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (activated || respawnController == null || !respawnController.IsPlayer(other))
            {
                return;
            }

            activated = true;
            respawnController.SetCheckpoint(transform.position + spawnOffset);
            PlayerPrefs.SetInt("Area2Cave.LastCheckpoint", checkpointNumber);
            PlayerPrefs.Save();

            if (markerRenderer != null)
            {
                markerRenderer.color = new Color(0.55f, 1f, 1f, 1f);
            }
        }
    }
}

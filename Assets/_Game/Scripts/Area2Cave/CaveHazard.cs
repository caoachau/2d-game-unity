using UnityEngine;

namespace Summit.Game.Area2Cave
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class CaveHazard : MonoBehaviour
    {
        [SerializeField] private CaveRespawnController respawnController;

        public void Configure(CaveRespawnController controller)
        {
            respawnController = controller;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (respawnController == null || !respawnController.IsPlayer(other))
            {
                return;
            }

            respawnController.RespawnNow();
        }
    }
}

using UnityEngine;

namespace Summit.Game.Area3OldCastle
{
    public sealed class OldCastleRespawnController : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private float killY = -8f;
        [SerializeField] private Vector3 currentSpawnPoint;

        private Rigidbody2D playerBody;
        public Vector3 CurrentSpawnPoint => currentSpawnPoint;

        public void Configure(Transform playerTransform, Vector3 initialSpawn, float minimumY)
        {
            player = playerTransform;
            currentSpawnPoint = initialSpawn;
            killY = minimumY;
            playerBody = player != null ? player.GetComponent<Rigidbody2D>() : null;
        }

        public void SetCheckpoint(Vector3 spawnPoint)
        {
            currentSpawnPoint = spawnPoint;
        }

        public void RespawnNow()
        {
            if (player == null) return;
            if (playerBody == null) playerBody = player.GetComponent<Rigidbody2D>();

            if (playerBody != null)
            {
                playerBody.linearVelocity = Vector2.zero;
                playerBody.angularVelocity = 0f;
                playerBody.position = currentSpawnPoint;
            }
            else
            {
                player.position = currentSpawnPoint;
            }
        }

        private void Update()
        {
            if (player != null && player.position.y < killY)
            {
                RespawnNow();
            }
        }
    }
}

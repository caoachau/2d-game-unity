using System.Collections;
using UnityEngine;

namespace Summit.Game.Area2Cave
{
    public sealed class CaveRespawnController : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private float killY = -8f;
        [SerializeField] private Vector3 currentSpawnPoint;
        [SerializeField] private Transform initialSpawnPoint;

        private Rigidbody2D playerBody;
        private Coroutine respawnRoutine;
        private bool hasCheckpoint;

        // The start marker moves with the level hierarchy. A serialized world
        // coordinate becomes stale when the designer moves the whole cave.
        public Vector3 CurrentSpawnPoint => !hasCheckpoint && initialSpawnPoint != null
            ? initialSpawnPoint.position
            : currentSpawnPoint;

        public void Configure(Transform playerTransform, Vector3 initialSpawn, float minimumY)
        {
            player = playerTransform;
            currentSpawnPoint = initialSpawn;
            killY = minimumY;
            playerBody = player != null ? player.GetComponent<Rigidbody2D>() : null;
            hasCheckpoint = false;
        }

        public void SetInitialSpawnPoint(Transform spawnPoint)
        {
            initialSpawnPoint = spawnPoint;
        }

        public void SetCheckpoint(Vector3 spawnPoint)
        {
            currentSpawnPoint = spawnPoint;
            hasCheckpoint = true;
        }

        public bool IsPlayer(Collider2D other)
        {
            return player != null && other != null && other.attachedRigidbody != null &&
                   other.attachedRigidbody.transform == player;
        }

        public void RespawnNow()
        {
            if (player == null || respawnRoutine != null)
            {
                return;
            }

            respawnRoutine = StartCoroutine(RespawnPlayer());
        }

        private IEnumerator RespawnPlayer()
        {
            Vector3 destination = CurrentSpawnPoint;
            if (playerBody == null)
            {
                playerBody = player.GetComponent<Rigidbody2D>();
            }

            if (playerBody != null)
            {
                // Suspend contacts while teleporting, then resume at the destination.
                playerBody.simulated = false;
                playerBody.linearVelocity = Vector2.zero;
                playerBody.angularVelocity = 0f;
                playerBody.position = destination;
                player.position = destination;
            }
            else
            {
                player.position = destination;
            }

            Physics2D.SyncTransforms();
            yield return new WaitForFixedUpdate();

            if (playerBody != null)
            {
                playerBody.position = destination;
                playerBody.linearVelocity = Vector2.zero;
                playerBody.angularVelocity = 0f;
                playerBody.simulated = true;
            }

            player.position = destination;
            Physics2D.SyncTransforms();
            respawnRoutine = null;
        }

        private void Update()
        {
            if (player != null && respawnRoutine == null && player.position.y < killY)
            {
                RespawnNow();
            }
        }

        private void OnDisable()
        {
            if (respawnRoutine != null)
            {
                StopCoroutine(respawnRoutine);
            }

            if (playerBody != null && !playerBody.simulated)
            {
                playerBody.simulated = true;
            }

            respawnRoutine = null;
        }
    }
}

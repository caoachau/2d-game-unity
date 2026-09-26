using System.Collections;
using NUnit.Framework;
using Summit.Game.Area2Cave;
using Summit.Game.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Summit.Game.Tests.PlayMode
{
    public sealed class CaveRespawnTests
    {
        private CaveRespawnController respawn;
        private Rigidbody2D playerBody;
        private PlayerGroundDetector ground;

        [UnitySetUp]
        public IEnumerator LoadCave()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Area2_Cave");
            yield return null;
            yield return null;
            respawn = Object.FindAnyObjectByType<CaveRespawnController>();
            var player = Object.FindAnyObjectByType<PlayerJumpController>();
            Assert.That(respawn, Is.Not.Null);
            Assert.That(player, Is.Not.Null);
            playerBody = player.GetComponent<Rigidbody2D>();
            ground = player.GetComponent<PlayerGroundDetector>();
        }

        [UnityTest]
        public IEnumerator FallingRepeatedly_ReturnsToMovedCaveAndStaysOnShelf()
        {
            Transform marker = GameObject.Find("SpawnPoint_Area2").transform;
            // The authored cave is translated by -100 X. This is the regression:
            // the old serialized respawn coordinate still pointed near X = -4.7.
            Assert.That(marker.position.x, Is.LessThan(-100f));
            Assert.That(Vector3.Distance(respawn.CurrentSpawnPoint, marker.position), Is.LessThan(0.001f));

            for (int fall = 0; fall < 3; fall++)
            {
                DropBelowCave();
                yield return AssertSettlesOnShelf(marker.position);
            }
        }

        [UnityTest]
        public IEnumerator CheckpointWorldPosition_TakesPriorityOverInitialMarker()
        {
            Vector3 checkpoint = GameObject.Find("Checkpoint_2").transform.position + Vector3.up * 1.2f;
            respawn.SetCheckpoint(checkpoint);
            DropBelowCave();
            yield return AssertSettlesOnShelf(checkpoint);
            Assert.That(Vector3.Distance(respawn.CurrentSpawnPoint, checkpoint), Is.LessThan(0.001f));
        }

        private void DropBelowCave()
        {
            Vector3 belowCave = new Vector3(playerBody.position.x, -20f, 0f);
            playerBody.position = belowCave;
            playerBody.transform.position = belowCave;
            playerBody.linearVelocity = Vector2.down * 20f;
            Physics2D.SyncTransforms();
        }

        private IEnumerator AssertSettlesOnShelf(Vector3 expectedSpawn)
        {
            yield return new WaitForSeconds(1f);
            for (int sample = 0; sample < 3; sample++)
            {
                Assert.That(playerBody.simulated, Is.True);
                Assert.That(playerBody.position.x, Is.EqualTo(expectedSpawn.x).Within(0.1f));
                Assert.That(playerBody.position.y, Is.InRange(expectedSpawn.y - 2f, expectedSpawn.y + 0.1f));
                Assert.That(ground.Refresh(), Is.True, "Respawn must land on a shelf, not keep falling in empty space.");
                Assert.That(Mathf.Abs(playerBody.linearVelocity.y), Is.LessThan(0.1f));
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Area2_Cave"));
                yield return new WaitForSeconds(0.5f);
            }
        }

        [UnityTearDown]
        public IEnumerator UnloadCave()
        {
            Scene cave = SceneManager.GetActiveScene();
            Scene empty = SceneManager.CreateScene("CaveRespawnTestCleanup");
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(cave);
        }
    }
}

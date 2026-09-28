using System.Collections;
using NUnit.Framework;
using Summit.Game.Configuration;
using Summit.Game.Input;
using Summit.Game.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Summit.Game.Tests.PlayMode
{
    public sealed class NamedLevelGoalTests
    {
        private Scene previousScene;
        private Scene scene;
        private Scene foreignScene;
        private PlayerJumpController player;
        private PlayerInputReader input;
        private PlayerMovementConfig config;
        private GameObject goalObject;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Time.timeScale = 1f;
            previousScene = SceneManager.GetActiveScene();
            scene = SceneManager.CreateScene("NamedGoalTest");
            SceneManager.SetActiveScene(scene);
            var playerObject = new GameObject("Player", typeof(Rigidbody2D), typeof(BoxCollider2D));
            playerObject.transform.position = new Vector3(0f, 10f, 0f);
            player = playerObject.AddComponent<PlayerJumpController>();
            input = new GameObject("Input").AddComponent<PlayerInputReader>();
            config = ScriptableObject.CreateInstance<PlayerMovementConfig>();
            goalObject = new GameObject("Goal_Test", typeof(BoxCollider2D));
            goalObject.transform.position = new Vector3(10f, 10f, 0f);
            goalObject.GetComponent<BoxCollider2D>().size = Vector2.one * 3f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator NamedGoal_PlayerContactCompletesSceneAndStopsInput()
        {
            player.Initialize(input, config);
            NamedLevelGoal goal = goalObject.GetComponent<NamedLevelGoal>();
            Assert.That(goal, Is.Not.Null);
            Assert.That(goalObject.GetComponent<Collider2D>().isTrigger, Is.True);
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            body.position = goalObject.transform.position;
            body.transform.position = goalObject.transform.position;
            Physics2D.SyncTransforms();
            float deadline = Time.realtimeSinceStartup + 2f;
            while (!goal.IsCompleted && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(goal.IsCompleted, Is.True);
            Assert.That(player.enabled, Is.False);
            Assert.That(input.enabled, Is.False);
            Assert.That(body.linearVelocity, Is.EqualTo(Vector2.zero));
            Assert.That(Time.timeScale, Is.EqualTo(0f));
        }

        [UnityTest]
        public IEnumerator NonPlayerRigidbody_DoesNotWin()
        {
            player.Initialize(input, config);
            var rock = new GameObject("Rock", typeof(Rigidbody2D), typeof(BoxCollider2D));
            rock.transform.position = goalObject.transform.position;
            rock.GetComponent<Rigidbody2D>().gravityScale = 0f;
            Physics2D.SyncTransforms();
            yield return new WaitForSecondsRealtime(0.15f);
            Assert.That(goalObject.GetComponent<NamedLevelGoal>().IsCompleted, Is.False);
            Assert.That(player.enabled, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        [UnityTest]
        public IEnumerator GoalInAnotherLoadedScene_IsNotAssignedToThisPlayer()
        {
            foreignScene = SceneManager.CreateScene("ForeignGoalTest");
            var foreignGoal = new GameObject("Goal_OtherScene", typeof(BoxCollider2D));
            SceneManager.MoveGameObjectToScene(foreignGoal, foreignScene);
            player.Initialize(input, config);
            yield return null;
            Assert.That(foreignGoal.GetComponent<NamedLevelGoal>(), Is.Null);
            Assert.That(foreignGoal.GetComponent<Collider2D>().isTrigger, Is.False);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1f;
            SceneManager.SetActiveScene(previousScene);
            if (foreignScene.IsValid() && foreignScene.isLoaded)
                yield return SceneManager.UnloadSceneAsync(foreignScene);
            yield return SceneManager.UnloadSceneAsync(scene);
            Object.Destroy(config);
        }
    }
}

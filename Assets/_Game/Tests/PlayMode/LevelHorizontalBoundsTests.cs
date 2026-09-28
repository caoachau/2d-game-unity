using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Summit.Game.Environment;
using Summit.Game.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Summit.Game.Tests.PlayMode
{
    public sealed class LevelHorizontalBoundsTests
    {
        private Scene previousScene;
        private Scene testScene;
        private Rigidbody2D body;
        private BoxCollider2D collider;
        private Transform backgroundParent;
        private Texture2D texture;
        private Sprite sprite;
        private float originalLeft;
        private float originalRight;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousScene = SceneManager.GetActiveScene();
            testScene = SceneManager.CreateScene("HorizontalBoundsTest");
            SceneManager.SetActiveScene(testScene);

            backgroundParent = new GameObject("CameraParent").transform;
            backgroundParent.position = new Vector3(3f, 0f, 0f);
            backgroundParent.localScale = new Vector3(5f, 2f, 1f);
            var background = new GameObject("Background", typeof(SpriteRenderer));
            background.transform.SetParent(backgroundParent, false);
            texture = new Texture2D(20, 10);
            sprite = Sprite.Create(texture, new Rect(0, 0, 20, 10), Vector2.one * 0.5f, 10f);
            SpriteRenderer renderer = background.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            originalLeft = renderer.bounds.min.x;
            originalRight = renderer.bounds.max.x;

            var player = new GameObject("Player", typeof(Rigidbody2D), typeof(BoxCollider2D));
            body = player.GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.position = new Vector2(3f, 0f);
            player.transform.localScale = new Vector3(2f, 3f, 1f);
            collider = player.GetComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            collider.offset = new Vector2(0.25f, 0f);
            player.AddComponent<PlayerMovement>();

            var systems = new GameObject("Bounds");
            systems.SetActive(false);
            LevelHorizontalBounds bounds = systems.AddComponent<LevelHorizontalBounds>();
            typeof(LevelHorizontalBounds).GetField("backgroundRenderers",
                BindingFlags.Instance | BindingFlags.NonPublic).SetValue(bounds, new[] { renderer });
            systems.SetActive(true);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FastJump_StopsEntireBodyAtRightEdge_AndCanMoveBackInside()
        {
            body.linearVelocity = new Vector2(10000f, 13f);
            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.That(collider.bounds.max.x, Is.LessThanOrEqualTo(originalRight + 0.001f));
            Assert.That(body.position.x, Is.EqualTo(originalRight - 1.5f).Within(0.01f));
            Assert.That(body.linearVelocity.y, Is.EqualTo(13f).Within(0.001f));

            for (int step = 0; step < 5; step++)
            {
                body.linearVelocity = new Vector2(100f, 13f);
                yield return new WaitForFixedUpdate();
                yield return null;
                Assert.That(body.position.x, Is.EqualTo(originalRight - 1.5f).Within(0.01f));
                Assert.That(collider.bounds.max.x, Is.LessThanOrEqualTo(originalRight + 0.001f));
            }

            float atBorder = body.position.x;
            body.linearVelocity = new Vector2(-3f, 13f);
            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.That(body.position.x, Is.LessThan(atBorder));
        }

        [UnityTest]
        public IEnumerator TeleportBeyondLeftEdge_IsContainedWithoutLimitingHeight()
        {
            body.position = new Vector2(-100f, 100f);
            body.transform.position = new Vector3(-100f, 100f, 0f);
            body.linearVelocity = new Vector2(-50f, 3f);
            Physics2D.SyncTransforms();
            yield return null;
            yield return null;
            Assert.That(collider.bounds.min.x, Is.EqualTo(originalLeft).Within(0.01f));
            Assert.That(body.linearVelocity.x, Is.EqualTo(0f));
            Assert.That(body.linearVelocity.y, Is.EqualTo(3f).Within(0.001f));
            Assert.That(body.position.y, Is.GreaterThanOrEqualTo(100f));
        }

        [UnityTest]
        public IEnumerator BackgroundFollowingCamera_DoesNotMoveAuthoredBorders()
        {
            backgroundParent.position += Vector3.right * 100f;
            body.position = new Vector2(100f, 0f);
            body.transform.position = new Vector3(100f, 0f, 0f);
            Physics2D.SyncTransforms();
            yield return null;
            yield return null;
            Assert.That(collider.bounds.max.x, Is.EqualTo(originalRight).Within(0.01f));
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            SceneManager.SetActiveScene(previousScene);
            yield return SceneManager.UnloadSceneAsync(testScene);
            Object.Destroy(sprite);
            Object.Destroy(texture);
        }
    }
}

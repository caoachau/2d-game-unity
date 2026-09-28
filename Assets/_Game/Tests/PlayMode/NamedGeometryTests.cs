using System.Collections;
using NUnit.Framework;
using Summit.Game.Configuration;
using Summit.Game.Domain;
using Summit.Game.Input;
using Summit.Game.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Summit.Game.Tests.PlayMode
{
    public sealed class NamedGeometryTests
    {
        private Scene previousScene;
        private Scene scene;
        private PlayerMovementConfig config;
        private PlayerJumpController player;
        private Rigidbody2D body;
        private BoxCollider2D playerCollider;
        private TestInput input;
        private Texture2D texture;
        private Sprite sprite;

        private sealed class TestInput : IPlayerInput
        {
            public float Horizontal { get; set; }
            public float Vertical { get; set; }
            public bool JumpHeld { get; set; }
            public bool Pressed;
            public bool ConsumeJumpPressed() { bool result = Pressed; Pressed = false; return result; }
            public bool ConsumeJumpReleased() => false;
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Time.timeScale = 1f;
            previousScene = SceneManager.GetActiveScene();
            scene = SceneManager.CreateScene("NamedGeometryTest");
            SceneManager.SetActiveScene(scene);
            config = ScriptableObject.CreateInstance<PlayerMovementConfig>();
            input = new TestInput();
            GameObject actor = new GameObject("Player", typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer));
            body = actor.GetComponent<Rigidbody2D>();
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.gravityScale = 0f;
            body.position = new Vector2(0f, 3f);
            playerCollider = actor.GetComponent<BoxCollider2D>();
            playerCollider.size = new Vector2(.8f, 1.35f);
            player = actor.AddComponent<PlayerJumpController>();
            actor.AddComponent<PlayerVisualController>();
            texture = new Texture2D(20, 10);
            sprite = Sprite.Create(texture, new Rect(0, 0, 20, 10), new Vector2(.3f, .2f), 10f);
            yield return null;
        }

        private BoxCollider2D Floor()
        {
            GameObject floor = new GameObject("Walkable_Test", typeof(BoxCollider2D));
            floor.transform.position = new Vector3(0f, -.5f, 0f);
            BoxCollider2D collider = floor.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(30f, 1f);
            return collider;
        }

        private GameObject Ladder(string name)
        {
            GameObject ladder = new GameObject(name, typeof(BoxCollider2D));
            ladder.transform.position = new Vector3(0f, 3f, 0f);
            BoxCollider2D collider = ladder.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(2f, 8f);
            collider.enabled = false;
            return ladder;
        }

        private SpriteRenderer Visual => player.transform.Find("PlayerVisual").GetComponent<SpriteRenderer>();

        private void AssertFeetOnCollider()
        {
            Assert.That(Visual.sprite, Is.Not.Null);
            Assert.That(Visual.bounds.min.y, Is.EqualTo(playerCollider.bounds.min.y).Within(.003f),
                "The rendered frame must not sink below the physical feet.");
        }

        [UnityTest]
        public IEnumerator ScaledWalkable_VisibleFeetRestOnSurface_WhenStandingRunningAndCharging()
        {
            GameObject platform = new GameObject("Walkable_Scaled", typeof(SpriteRenderer));
            platform.GetComponent<SpriteRenderer>().sprite = sprite;
            platform.transform.position = new Vector3(0f, -2f, 0f);
            platform.transform.localScale = new Vector3(8f, 2f, 1f);
            player.transform.localScale = new Vector3(2.4f, 2.96f, 1f);
            playerCollider.offset = new Vector2(.12f, .15f);
            player.Initialize(input, config);
            Physics2D.SyncTransforms();
            yield return new WaitForSeconds(.9f);
            Collider2D floor = platform.GetComponent<Collider2D>();
            Assert.That(floor, Is.Not.Null);
            Assert.That(playerCollider.bounds.min.y, Is.EqualTo(floor.bounds.max.y).Within(.04f));
            AssertFeetOnCollider();
            input.Horizontal = .5f;
            yield return new WaitForSeconds(.25f);
            Assert.That(Visual.sprite.name, Does.StartWith("run_"));
            AssertFeetOnCollider();
            input.Horizontal = 0f;
            input.Pressed = input.JumpHeld = true;
            yield return new WaitForSeconds(.2f);
            Assert.That(Visual.sprite.name, Does.StartWith("crouch_"));
            AssertFeetOnCollider();
        }

        [UnityTest]
        public IEnumerator Blocker_StopsPlayerAndReplaces3DPhysics_PreservingEdited2DColliders()
        {
            Floor();
            GameObject rock = new GameObject("Blocker_Rock", typeof(SpriteRenderer), typeof(BoxCollider));
            rock.GetComponent<SpriteRenderer>().sprite = sprite;
            rock.transform.position = new Vector3(3f, .5f, 0f);
            rock.transform.localScale = new Vector3(1.5f, 3f, 1f);
            GameObject authored = new GameObject("Blocker_Edited", typeof(BoxCollider2D), typeof(CircleCollider2D));
            authored.transform.position = new Vector3(30f, 0f, 0f);
            BoxCollider2D edited = authored.GetComponent<BoxCollider2D>();
            edited.size = new Vector2(2f, 3f);
            edited.offset = new Vector2(.3f, .4f);
            edited.enabled = false;
            edited.isTrigger = true;
            authored.GetComponent<CircleCollider2D>().isTrigger = true;
            player.Initialize(input, config);
            Assert.That(rock.GetComponent<BoxCollider>().enabled, Is.False);
            Assert.That(edited.enabled, Is.True);
            Assert.That(edited.isTrigger, Is.False);
            Assert.That(edited.size, Is.EqualTo(new Vector2(2f, 3f)));
            Assert.That(edited.offset, Is.EqualTo(new Vector2(.3f, .4f)));
            Assert.That(authored.GetComponent<CircleCollider2D>().isTrigger, Is.False);
            yield return new WaitForSeconds(.6f);
            input.Horizontal = 1f;
            yield return new WaitForSeconds(1f);
            Assert.That(playerCollider.bounds.max.x, Is.EqualTo(rock.GetComponentInChildren<Collider2D>().bounds.min.x).Within(.05f));
            input.Horizontal = -1f;
            float blockedX = body.position.x;
            yield return new WaitForSeconds(.25f);
            Assert.That(body.position.x, Is.LessThan(blockedX - .2f));
        }

        [UnityTest]
        public IEnumerator Legacy3DLadder_ClimbsHoldsDescendsAndExitsSideways_WithMatchingAnimation()
        {
            Floor();
            GameObject ladder = new GameObject("Ladder_Test", typeof(BoxCollider));
            ladder.GetComponent<BoxCollider>().size = new Vector3(2f, 8f, 1f);
            ladder.GetComponent<BoxCollider>().center = new Vector3(0f, 3f, 0f);
            player.Initialize(input, config);
            Assert.That(ladder.GetComponent<BoxCollider>().enabled, Is.False);
            Assert.That(ladder.GetComponentInChildren<Collider2D>().enabled, Is.True);
            Assert.That(ladder.GetComponentInChildren<Collider2D>().isTrigger, Is.True);
            yield return new WaitForSeconds(.6f);
            float initial = body.position.y;
            input.Vertical = 1f;
            yield return new WaitForSeconds(.3f);
            Assert.That(body.position.y, Is.GreaterThan(initial + .5f));
            Assert.That(body.gravityScale, Is.Zero);
            Assert.That(Visual.sprite.name, Does.StartWith("climb_"));
            input.Vertical = 0f;
            yield return new WaitForSeconds(.05f);
            float held = body.position.y;
            Sprite heldFrame = Visual.sprite;
            yield return new WaitForSeconds(.2f);
            Assert.That(body.position.y, Is.EqualTo(held).Within(.02f));
            Assert.That(Visual.sprite, Is.SameAs(heldFrame));
            input.Vertical = -1f;
            yield return new WaitForSeconds(.15f);
            Assert.That(body.position.y, Is.LessThan(held - .25f));
            input.Vertical = 0f;
            input.Horizontal = 1f;
            yield return new WaitForSeconds(.1f);
            Assert.That(body.gravityScale, Is.GreaterThan(0f));
            Assert.That(player.GetComponent<PlayerStateController>().CurrentState, Is.Not.EqualTo(PlayerState.Climbing));
        }

        [UnityTest]
        public IEnumerator OverlappingLadders_KeepClimbingUntilLastZoneIsDisabled()
        {
            Floor();
            GameObject first = Ladder("Ladder_First");
            GameObject second = Ladder("Ladder_Second");
            second.SetActive(false);
            player.Initialize(input, config);
            Assert.That(second.GetComponent<LadderClimbZone>(), Is.Not.Null,
                "Inactive named objects also need configuration before they are enabled.");
            second.SetActive(true);
            input.Vertical = 1f;
            yield return new WaitForSeconds(.2f);
            first.SetActive(false);
            float height = body.position.y;
            yield return new WaitForSeconds(.2f);
            Assert.That(body.gravityScale, Is.Zero);
            Assert.That(body.position.y, Is.GreaterThan(height + .3f));
            second.SetActive(false);
            Assert.That(body.gravityScale, Is.GreaterThan(0f));
            height = body.position.y;
            yield return new WaitForSeconds(.2f);
            Assert.That(body.position.y, Is.LessThan(height));
        }

        private IEnumerator ClimbThroughLanding(Vector3 scale)
        {
            Floor();
            GameObject ladder = Ladder("Ladder_WithLanding");
            ladder.transform.position = new Vector3(0f, 2f, 0f);
            ladder.GetComponent<BoxCollider2D>().size = new Vector2(2f, 4f);
            GameObject landing = new GameObject("Walkable_LadderTop", typeof(BoxCollider2D));
            landing.transform.position = new Vector3(0f, 4.1f, 0f);
            landing.GetComponent<BoxCollider2D>().size = new Vector2(8f, .2f);
            player.transform.localScale = scale;
            body.position = new Vector2(0f, 2.2f);
            player.Initialize(input, config);
            input.Vertical = 1f;
            // Keep Up held after reaching the top: the character must settle,
            // rather than repeatedly re-entering the top of the ladder.
            yield return new WaitForSeconds(2.8f);
            Assert.That(playerCollider.bounds.min.y, Is.EqualTo(landing.GetComponent<Collider2D>().bounds.max.y).Within(.05f));
            Assert.That(body.gravityScale, Is.GreaterThan(0f));
            Assert.That(player.GetComponent<PlayerStateController>().CurrentState, Is.Not.EqualTo(PlayerState.Climbing));
            AssertFeetOnCollider();
            float x = body.position.x;
            input.Horizontal = .5f;
            input.Vertical = 0f;
            yield return new WaitForSeconds(.25f);
            Assert.That(body.position.x, Is.GreaterThan(x + .2f));
            Assert.That(playerCollider.bounds.min.y, Is.EqualTo(landing.GetComponent<Collider2D>().bounds.max.y).Within(.05f));
        }

        [UnityTest]
        public IEnumerator LadderTop_WalkableAllowsClimbThroughThenSupportsPlayer()
        {
            yield return ClimbThroughLanding(Vector3.one);
        }

        [UnityTest]
        public IEnumerator LadderTop_AlsoSupportsScaledPlayerUsedInAuthoredMaps()
        {
            yield return ClimbThroughLanding(new Vector3(2.39802f, 2.96122f, 1f));
        }

        [UnityTest]
        public IEnumerator LadderTop_BlockerRemainsSolid()
        {
            Floor();
            GameObject ladder = Ladder("Ladder_UnderBlocker");
            ladder.transform.position = new Vector3(0f, 2f, 0f);
            ladder.GetComponent<BoxCollider2D>().size = new Vector2(2f, 4f);
            GameObject roof = new GameObject("Blocker_Roof", typeof(BoxCollider2D));
            roof.transform.position = new Vector3(0f, 4.1f, 0f);
            roof.GetComponent<BoxCollider2D>().size = new Vector2(8f, .2f);
            player.Initialize(input, config);
            input.Vertical = 1f;
            yield return new WaitForSeconds(1f);
            Assert.That(playerCollider.bounds.max.y, Is.LessThanOrEqualTo(roof.GetComponent<Collider2D>().bounds.min.y + .04f));
            Assert.That(roof.GetComponent<PlatformEffector2D>(), Is.Null);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1f;
            SceneManager.SetActiveScene(previousScene);
            yield return SceneManager.UnloadSceneAsync(scene);
            Object.Destroy(config);
            Object.Destroy(sprite);
            Object.Destroy(texture);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Summit.Game.Player
{
    [DisallowMultipleComponent]
    public sealed class LadderClimbZone : MonoBehaviour
    {
        private PlayerJumpController player;
        private readonly HashSet<Collider2D> contacts = new();
        private Collider2D exitPlatform;
        private Collider2D playerCollider;

        public void Configure(PlayerJumpController controller)
        {
            if (player != null && player != controller) player.ExitLadder(this);
            contacts.Clear();
            player = controller;
            playerCollider = player != null ? player.GetComponent<Collider2D>() : null;
        }

        internal bool CanStartClimb => exitPlatform == null || playerCollider == null ||
            playerCollider.bounds.min.y < exitPlatform.bounds.max.y - .03f;

        internal void ConfigureTopPlatforms()
        {
            BoxCollider2D ladder = GetComponent<BoxCollider2D>();
            if (ladder == null || !ladder.enabled || !gameObject.activeInHierarchy) return;
            Bounds ladderBounds = ladder.bounds;
            float tolerance = Mathf.Max(.3f, playerCollider != null ? playerCollider.bounds.size.y * .5f : .5f);
            float highestExit = float.NegativeInfinity;
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            foreach (Collider2D platform in root.GetComponentsInChildren<Collider2D>())
            {
                if (!platform.enabled || platform.isTrigger || !PlayerLevelGeometry.IsWalkable(platform.transform)) continue;
                Bounds bounds = platform.bounds;
                if (bounds.max.x <= ladderBounds.min.x || bounds.min.x >= ladderBounds.max.x ||
                    Mathf.Abs(bounds.max.y - ladderBounds.max.y) > tolerance) continue;

                PlatformEffector2D effector = platform.GetComponent<PlatformEffector2D>();
                if (effector == null) effector = platform.gameObject.AddComponent<PlatformEffector2D>();
                effector.enabled = true;
                effector.useOneWay = true;
                effector.useOneWayGrouping = true;
                effector.surfaceArc = 180f;
                effector.rotationalOffset = 0f;
                platform.usedByEffector = true;
                if (bounds.max.y > highestExit)
                {
                    highestExit = bounds.max.y;
                    exitPlatform = platform;
                }
            }

            // Keep ladder contact until the feet have cleared the landing surface.
            // Otherwise gravity resumes while the player's body is still below it.
            if (exitPlatform == null || Vector3.Dot(transform.up, Vector3.up) < .99f) return;
            float worldTop = Mathf.Max(ladderBounds.max.y, highestExit + .06f);
            float localTop = transform.InverseTransformPoint(new Vector3(ladderBounds.center.x,
                worldTop, ladderBounds.center.z)).y;
            float bottom = ladder.offset.y - ladder.size.y * .5f;
            ladder.size = new Vector2(ladder.size.x, localTop - bottom);
            ladder.offset = new Vector2(ladder.offset.x, (localTop + bottom) * .5f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsTargetPlayer(other))
            {
                contacts.Add(other);
                player.EnterLadder(this);
            }
        }

        private void OnTriggerStay2D(Collider2D other) => OnTriggerEnter2D(other);

        private void OnEnable()
        {
            if (player != null) ConfigureTopPlatforms();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (IsTargetPlayer(other))
            {
                contacts.Remove(other);
                if (contacts.Count == 0) player.ExitLadder(this);
            }
        }

        private void OnDisable()
        {
            contacts.Clear();
            if (player != null) player.ExitLadder(this);
        }

        private bool IsTargetPlayer(Collider2D other)
        {
            return player != null && other != null && other.attachedRigidbody != null &&
                   other.attachedRigidbody.transform == player.transform;
        }
    }
}

using UnityEngine;

namespace Summit.Game.Player
{
    /// <summary>
    /// Builds physics for Walkable_*, Blocker_*, Ladder_* and Goal_* objects.
    /// Platforms and obstacles are solid; ladders are trigger volumes.
    /// </summary>
    internal static class PlayerLevelGeometry
    {
        private const string WalkablePrefix = "Walkable_";
        private const string BlockerPrefix = "Blocker_";
        private const string LadderPrefix = "Ladder_";
        private const string GoalPrefix = "Goal_";

        public static void Configure(PlayerJumpController player)
        {
            if (player == null)
            {
                return;
            }

            GameObject[] objects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
            foreach (GameObject sceneObject in objects)
            {
                if (sceneObject.scene != player.gameObject.scene)
                    continue;

                if (sceneObject.name.StartsWith(WalkablePrefix, System.StringComparison.OrdinalIgnoreCase) ||
                    sceneObject.name.StartsWith(BlockerPrefix, System.StringComparison.OrdinalIgnoreCase))
                {
                    ConfigureSolid(sceneObject);
                }
                else if (sceneObject.name.StartsWith(LadderPrefix, System.StringComparison.OrdinalIgnoreCase))
                {
                    ConfigureLadder(sceneObject, player);
                }
                else if (sceneObject.name.StartsWith(GoalPrefix, System.StringComparison.OrdinalIgnoreCase))
                {
                    ConfigureGoal(sceneObject, player);
                }
            }

            Physics2D.SyncTransforms();
            foreach (GameObject root in player.gameObject.scene.GetRootGameObjects())
            foreach (LadderClimbZone ladder in root.GetComponentsInChildren<LadderClimbZone>(true))
                ladder.ConfigureTopPlatforms();
        }

        internal static bool IsWalkable(Transform target)
        {
            for (Transform current = target; current != null; current = current.parent)
            {
                if (current.name.StartsWith(BlockerPrefix, System.StringComparison.OrdinalIgnoreCase)) return false;
                if (current.name.StartsWith(WalkablePrefix, System.StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static void ConfigureGoal(GameObject target, PlayerJumpController player)
        {
            GameObject physicsObject = ConfigureColliders(target, true)[0].gameObject;
            NamedLevelGoal goal = physicsObject.GetComponent<NamedLevelGoal>();
            if (goal == null)
                goal = physicsObject.AddComponent<NamedLevelGoal>();

            goal.Configure(player);
        }

        private static void ConfigureSolid(GameObject target)
        {
            // The ground probe deliberately ignores triggers.
            ConfigureColliders(target, false);
        }

        private static void ConfigureLadder(GameObject target, PlayerJumpController player)
        {
            GameObject physicsObject = ConfigureColliders(target, true)[0].gameObject;
            LadderClimbZone zone = physicsObject.GetComponent<LadderClimbZone>();
            if (zone == null)
            {
                zone = physicsObject.AddComponent<LadderClimbZone>();
            }

            zone.Configure(player);
        }

        private static Collider2D[] ConfigureColliders(GameObject target, bool trigger)
        {
            // 3D colliders never collide with Rigidbody2D. Replace their role with
            // 2D physics, and keep explicitly edited 2D shapes/offsets intact.
            foreach (Collider legacy in target.GetComponents<Collider>()) legacy.enabled = false;
            Collider2D[] colliders = target.GetComponents<Collider2D>();
            Transform proxy = target.transform.Find("NamedPhysics2D");
            if (colliders.Length == 0 && proxy != null) colliders = proxy.GetComponents<Collider2D>();
            if (colliders.Length == 0) colliders = new[] { CreateSpriteSizedCollider(target) };
            foreach (Collider2D collider in colliders)
            {
                collider.enabled = true;
                collider.isTrigger = trigger;
            }
            return colliders;
        }

        private static Collider2D CreateSpriteSizedCollider(GameObject target)
        {
            SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
            GameObject physicsObject = target;
            // Unity rejects 2D and 3D colliders on the same object, even when the
            // 3D component is disabled. An identity child keeps the authored shape.
            if (target.GetComponent<Collider>() != null || target.GetComponent<Rigidbody>() != null)
            {
                physicsObject = new GameObject("NamedPhysics2D");
                physicsObject.layer = target.layer;
                physicsObject.transform.SetParent(target.transform, false);
            }
            BoxCollider2D box = physicsObject.AddComponent<BoxCollider2D>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                box.size = spriteRenderer.localBounds.size;
                box.offset = spriteRenderer.localBounds.center;
            }
            else if (target.TryGetComponent(out BoxCollider legacyBox))
            {
                box.size = new Vector2(legacyBox.size.x, legacyBox.size.y);
                box.offset = new Vector2(legacyBox.center.x, legacyBox.center.y);
            }

            return box;
        }
    }
}

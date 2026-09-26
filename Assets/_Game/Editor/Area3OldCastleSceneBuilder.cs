using System.Collections.Generic;
using System.IO;
using Summit.Game.Area3OldCastle;
using Summit.Game.Camera;
using Summit.Game.Configuration;
using Summit.Game.Input;
using Summit.Game.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Summit.Game.Editor
{
    /// <summary>
    /// Generates an editable vertical Area 3 - Old Castle scene.
    /// The layout is intentionally built from normal GameObjects so the designer
    /// can drag, duplicate, resize and rebalance platforms directly in Scene View.
    /// </summary>
    public static class Area3OldCastleSceneBuilder
    {
        public const string ScenePath = "Assets/_Game/Scenes/Area3_OldCastle.unity";
        private const string ArtRoot = "Assets/_Game/Area3_OldCastle/Art/";
        private const string PlayerPrefabPath = "Assets/_Game/Prefabs/Player/Player.prefab";
        private const string MovementConfigPath = "Assets/_Game/ScriptableObjects/Player/PlayerMovementConfig.asset";
        private const string GeneratedRootName = "Area3_OldCastle_GENERATED_v1";

        private const string PlatformWidePath = ArtRoot + "Terrain/platform_01.png";
        private const string PlatformMidPath = ArtRoot + "Terrain/platform_02.png";
        private const string PlatformSmallPath = ArtRoot + "Terrain/platform_small.png";
        private const string PlatformBrokenPath = ArtRoot + "Terrain/platform_broken.png";
        // Use the same Fallen Spires sprite as Area 2; the previous sheet could
        // render with an unwanted mask in-game.
        private const string PlayerPath = "Assets/_Game/GeneratedAssets/FallenSpires_AllAssets_From2Screens/02_Gameplay/SeparatedAssets/Character/idle_01.png";
        private const string PrincessPath = ArtRoot + "Characters/Princess/princess_idle.png";
        private const string GemPath = ArtRoot + "Items/red_gem.png";
        private const string CheckpointPath = ArtRoot + "Items/blue_crystal.png";
        private const string TorchPath = ArtRoot + "Decorations/torch_wall.png";
        private const string BannerPath = ArtRoot + "Decorations/banner_red.png";
        private const string ChandelierPath = ArtRoot + "Decorations/chandelier.png";
        private const string StatuePath = ArtRoot + "Special/suit_of_armor.png";
        private const string SpikesPath = ArtRoot + "Hazards/spikes_floor.png";
        private const string PendulumPath = ArtRoot + "Traps/pendulum_blade.png";
        private const string MovingPlatformPath = ArtRoot + "MovingPlatforms/moving_platform_01.png";
        private const string WallPath = ArtRoot + "Terrain/wall_vertical.png";
        private const string WindowPath = ArtRoot + "Architecture/stained_glass_01.png";
        private const string ArchPath = ArtRoot + "Terrain/wall_arch.png";
        private const string BgSkyPath = ArtRoot + "Background/bg_sky.png";
        private const string BgFarPath = ArtRoot + "Background/bg_castle_far.png";
        private const string BgMidPath = ArtRoot + "Background/bg_castle_mid.png";
        private const string BgNearPath = ArtRoot + "Background/bg_castle_near.png";
        private const string UiHeartFullPath = ArtRoot + "UI/heart_full.png";
        private const string UiHeartEmptyPath = ArtRoot + "UI/heart_empty.png";
        private const string UiPausePath = ArtRoot + "UI/pause_button.png";
        private const string UiSettingsPath = ArtRoot + "UI/settings_button.png";

        [MenuItem("Fallen Spires/Area 3 - Old Castle/Create or Rebuild Old Castle Scene", priority = 1)]
        public static void BuildFromMenu()
        {
            if (File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("Rebuild Area 3 - Old Castle",
                    "Area3_OldCastle.unity already exists. Rebuilding will replace that scene file. Continue?",
                    "Rebuild", "Cancel"))
            {
                return;
            }
            BuildScene(true);
        }

        [MenuItem("Fallen Spires/Area 3 - Old Castle/Open Old Castle Art Folder", priority = 20)]
        public static void OpenArtFolder()
        {
            Object folder = AssetDatabase.LoadAssetAtPath<Object>("Assets/_Game/Area3_OldCastle/Art");
            if (folder != null)
            {
                Selection.activeObject = folder;
                EditorGUIUtility.PingObject(folder);
            }
        }

        public static bool BuildScene(bool openWhenFinished)
        {
            Sprite wide = LoadSprite(PlatformWidePath);
            Sprite mid = LoadSprite(PlatformMidPath);
            Sprite small = LoadSprite(PlatformSmallPath);
            Sprite broken = LoadSprite(PlatformBrokenPath);
            Sprite playerSprite = LoadSprite(PlayerPath);
            Sprite princessSprite = LoadSprite(PrincessPath);
            Sprite checkpointSprite = LoadSprite(CheckpointPath);
            Sprite gemSprite = LoadSprite(GemPath);
            Sprite torchSprite = LoadSprite(TorchPath);
            Sprite bannerSprite = LoadSprite(BannerPath);
            Sprite chandelierSprite = LoadSprite(ChandelierPath);
            Sprite statueSprite = LoadSprite(StatuePath);
            Sprite spikesSprite = LoadSprite(SpikesPath);
            Sprite pendulumSprite = LoadSprite(PendulumPath);
            Sprite movingSprite = LoadSprite(MovingPlatformPath);
            Sprite wallSprite = LoadSprite(WallPath);
            Sprite windowSprite = LoadSprite(WindowPath);
            Sprite archSprite = LoadSprite(ArchPath);
            Sprite bgSky = LoadSprite(BgSkyPath);
            Sprite bgFar = LoadSprite(BgFarPath);
            Sprite bgMid = LoadSprite(BgMidPath);
            Sprite bgNear = LoadSprite(BgNearPath);
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            PlayerMovementConfig movementConfig = AssetDatabase.LoadAssetAtPath<PlayerMovementConfig>(MovementConfigPath);

            if (wide == null || small == null || playerSprite == null || princessSprite == null ||
                checkpointSprite == null || torchSprite == null || playerPrefab == null || movementConfig == null)
            {
                Debug.LogError("[Area3 Old Castle] Assets are still importing. Wait for Unity, then run Fallen Spires > Area 3 - Old Castle > Create or Rebuild Old Castle Scene.");
                return false;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new(GeneratedRootName);
            GameObject systemsRoot = CreateRoot("Systems", root.transform);
            GameObject levelRoot = CreateRoot("Level_Area3_OldCastle", root.transform);
            GameObject backgroundRoot = CreateRoot("Background", levelRoot.transform);
            GameObject architectureRoot = CreateRoot("Architecture", levelRoot.transform);
            GameObject platformsRoot = CreateRoot("Platforms", levelRoot.transform);
            GameObject movingPlatformsRoot = CreateRoot("MovingPlatforms", levelRoot.transform);
            GameObject checkpointsRoot = CreateRoot("Checkpoints", levelRoot.transform);
            GameObject itemsRoot = CreateRoot("Collectibles", levelRoot.transform);
            GameObject hazardsRoot = CreateRoot("Hazards", levelRoot.transform);
            GameObject decorationsRoot = CreateRoot("Decorations", levelRoot.transform);
            GameObject finaleRoot = CreateRoot("Finale_Princess", levelRoot.transform);

            // Camera: slightly wider than Area 2 to support long horizontal jumps.
            GameObject cameraObject = new("Main Camera", typeof(UnityEngine.Camera), typeof(AudioListener), typeof(VerticalCameraFollow));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(root.transform);
            UnityEngine.Camera camera = cameraObject.GetComponent<UnityEngine.Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7.6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.055f, 0.12f, 1f);
            cameraObject.transform.position = new Vector3(-4.5f, 3f, -10f);
            VerticalCameraFollow cameraFollow = cameraObject.GetComponent<VerticalCameraFollow>();
            SerializedObject cameraFollowSo = new(cameraFollow);
            cameraFollowSo.FindProperty("framingOffset").vector2Value = new Vector2(0f, 2.0f);
            cameraFollowSo.FindProperty("riseSmoothTime").floatValue = 0.16f;
            cameraFollowSo.FindProperty("fallSmoothTime").floatValue = 0.07f;
            cameraFollowSo.FindProperty("lockHorizontal").boolValue = false;
            cameraFollowSo.ApplyModifiedPropertiesWithoutUndo();

            CreateBackgroundLayer(bgSky, backgroundRoot.transform, "Sky", -120, new Color(0.85f, 0.80f, 1f, 1f), 20f, 100f, 46f);
            CreateBackgroundLayer(bgFar, backgroundRoot.transform, "FarCastle", -110, new Color(0.82f, 0.78f, 1f, 0.82f), 19f, 96f, 46f);
            CreateBackgroundLayer(bgMid, backgroundRoot.transform, "MidCastle", -100, new Color(0.72f, 0.72f, 0.92f, 0.72f), 18f, 92f, 46f);
            CreateBackgroundLayer(bgNear, backgroundRoot.transform, "NearCastle", -90, new Color(0.66f, 0.63f, 0.80f, 0.52f), 17f, 92f, 46f);
            CreateCastleSideWalls(wallSprite, windowSprite, archSprite, architectureRoot.transform);

            // Player.
            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            player.name = "Player_Area3";
            player.transform.SetParent(root.transform);
            Vector3 initialSpawn = new(-5.4f, 1.25f, 0f);
            player.transform.position = initialSpawn;
            SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();
            if (playerRenderer != null)
            {
                playerRenderer.sprite = playerSprite;
                playerRenderer.sortingOrder = 40;
                playerRenderer.color = Color.white;
                playerRenderer.maskInteraction = SpriteMaskInteraction.None;
            }
            PlayerJumpController jumpController = player.GetComponent<PlayerJumpController>();
            if (jumpController == null)
            {
                Debug.LogError("[Area3 Old Castle] Player prefab is missing PlayerJumpController.");
                return false;
            }

            PlayerInputReader inputReader = systemsRoot.AddComponent<PlayerInputReader>();
            OldCastleRespawnController respawn = systemsRoot.AddComponent<OldCastleRespawnController>();
            respawn.Configure(player.transform, initialSpawn, -7f);

            // Area 3 route: deliberately smaller platforms and repeated left-right crossings.
            PlatformSpec[] route =
            {
                new("StartShelf",        -5.3f,  0.0f, 1.75f, 0.82f, PlatformKind.Wide),
                new("P01",               -2.3f,  3.0f, 0.88f, 0.78f, PlatformKind.Small),
                new("P02",                1.4f,  5.7f, 0.80f, 0.76f, PlatformKind.Small),
                new("P03",                5.0f,  8.4f, 0.82f, 0.76f, PlatformKind.Small),
                new("P04",                1.0f, 11.2f, 0.78f, 0.74f, PlatformKind.Broken),
                new("CheckpointShelf_1", -3.6f, 14.0f, 1.45f, 0.82f, PlatformKind.Wide),
                new("P05",                0.4f, 16.8f, 0.74f, 0.74f, PlatformKind.Small),
                new("P06",                4.8f, 19.5f, 0.72f, 0.72f, PlatformKind.Small),
                new("P07",                0.5f, 22.3f, 0.78f, 0.72f, PlatformKind.Mid),
                new("P08",               -4.5f, 25.2f, 0.70f, 0.72f, PlatformKind.Small),
                new("CheckpointShelf_2", -0.5f, 28.4f, 1.45f, 0.82f, PlatformKind.Wide),
                new("P09",                4.5f, 31.4f, 0.72f, 0.72f, PlatformKind.Small),
                new("P10",                0.4f, 34.2f, 0.68f, 0.70f, PlatformKind.Small),
                new("P11",               -4.9f, 37.1f, 0.72f, 0.72f, PlatformKind.Small),
                new("P12",               -0.7f, 40.0f, 0.70f, 0.72f, PlatformKind.Broken),
                new("P13",                4.2f, 42.9f, 0.72f, 0.72f, PlatformKind.Small),
                new("P14",                0.2f, 45.8f, 0.70f, 0.70f, PlatformKind.Small),
                new("CheckpointShelf_3", -3.8f, 48.8f, 1.48f, 0.82f, PlatformKind.Wide),
                new("P15",                0.4f, 51.7f, 0.68f, 0.70f, PlatformKind.Small),
                new("P16",                4.9f, 54.7f, 0.68f, 0.70f, PlatformKind.Small),
                new("P17",                0.8f, 57.6f, 0.72f, 0.72f, PlatformKind.Broken),
                new("P18",               -4.3f, 60.5f, 0.66f, 0.70f, PlatformKind.Small),
                new("P19",                0.2f, 63.4f, 0.66f, 0.70f, PlatformKind.Small),
                new("P20",                5.0f, 66.3f, 0.70f, 0.70f, PlatformKind.Small),
                new("P21",                0.8f, 69.3f, 0.66f, 0.70f, PlatformKind.Small),
                new("RestShelf",         -3.1f, 72.3f, 1.05f, 0.78f, PlatformKind.Mid),
                new("P22",                1.1f, 75.1f, 0.64f, 0.68f, PlatformKind.Small),
                new("P23",                5.0f, 78.0f, 0.66f, 0.70f, PlatformKind.Small),
                new("P24",                0.6f, 80.9f, 0.64f, 0.68f, PlatformKind.Broken),
                new("ApproachPlatform",   1.2f, 84.0f, 1.08f, 0.78f, PlatformKind.Mid),
                new("PrincessPlatform",   5.2f, 86.4f, 1.45f, 0.82f, PlatformKind.Wide)
            };

            foreach (PlatformSpec spec in route)
            {
                Sprite selected = spec.Kind switch
                {
                    PlatformKind.Small => small,
                    PlatformKind.Broken => broken != null ? broken : small,
                    PlatformKind.Mid => mid != null ? mid : wide,
                    _ => wide
                };
                CreatePlatform(selected, platformsRoot.transform, spec);
            }

            // Moving platforms add timing checks but keep the main theme focused on horizontal precision.
            if (movingSprite != null)
            {
                CreateMovingPlatform(movingSprite, movingPlatformsRoot.transform,
                    new PlatformSpec("Moving_A", -1.9f, 18.2f, 0.86f, 0.78f, PlatformKind.Small), new Vector2(3.7f, 0f), 3.4f, 0f);
                CreateMovingPlatform(movingSprite, movingPlatformsRoot.transform,
                    new PlatformSpec("Moving_B", 2.0f, 56.1f, 0.82f, 0.76f, PlatformKind.Small), new Vector2(-4.0f, 0f), 3.8f, 0.28f);
            }

            // Checkpoints on safe shelves.
            CreateCheckpoint(checkpointSprite, checkpointsRoot.transform, respawn, 1, new Vector3(-3.6f, 15.3f, 0f));
            CreateCheckpoint(checkpointSprite, checkpointsRoot.transform, respawn, 2, new Vector3(-0.5f, 29.7f, 0f));
            CreateCheckpoint(checkpointSprite, checkpointsRoot.transform, respawn, 3, new Vector3(-3.8f, 50.1f, 0f));

            // Optional gems reward committing to the center of long gaps.
            if (gemSprite != null)
            {
                Vector3[] gemPositions =
                {
                    new(-2.3f, 4.2f, 0f), new(1.4f, 6.9f, 0f), new(5.0f, 9.6f, 0f),
                    new(4.8f, 20.7f, 0f), new(-4.5f, 26.4f, 0f), new(4.5f, 32.6f, 0f),
                    new(-4.9f, 38.3f, 0f), new(4.2f, 44.1f, 0f), new(4.9f, 55.9f, 0f),
                    new(-4.3f, 61.7f, 0f), new(5.0f, 67.5f, 0f), new(5.0f, 79.2f, 0f)
                };
                foreach (Vector3 p in gemPositions) CreateCollectible(gemSprite, itemsRoot.transform, p);
            }

            // Precision hazards. They occupy only part of a shelf so landing remains possible.
            if (spikesSprite != null)
            {
                CreateHazard(spikesSprite, hazardsRoot.transform, respawn, "Spikes_01", new Vector3(2.0f, 14.65f, 0f), new Vector3(0.55f, 0.42f, 1f));
                CreateHazard(spikesSprite, hazardsRoot.transform, respawn, "Spikes_02", new Vector3(1.3f, 29.05f, 0f), new Vector3(0.52f, 0.42f, 1f));
                CreateHazard(spikesSprite, hazardsRoot.transform, respawn, "Spikes_03", new Vector3(-2.0f, 49.45f, 0f), new Vector3(0.52f, 0.42f, 1f));
                CreateHazard(spikesSprite, hazardsRoot.transform, respawn, "Spikes_04", new Vector3(2.0f, 72.95f, 0f), new Vector3(0.45f, 0.40f, 1f));
            }

            if (pendulumSprite != null)
            {
                CreateHazard(pendulumSprite, hazardsRoot.transform, respawn, "PendulumBlade_A", new Vector3(2.0f, 41.2f, 0f), new Vector3(0.55f, 0.55f, 1f), new Vector2(1.2f, 1.2f));
                CreateHazard(pendulumSprite, hazardsRoot.transform, respawn, "PendulumBlade_B", new Vector3(-1.2f, 65.0f, 0f), new Vector3(0.52f, 0.52f, 1f), new Vector2(1.1f, 1.1f));
            }

            // Castle decorations fill both sides so the level does not feel empty.
            AddDecorations(torchSprite, bannerSprite, chandelierSprite, statueSprite, windowSprite, decorationsRoot.transform);

            // Final approach: landing here opens dialogue, but gameplay stays live.
            GameObject dialogueTriggerObject = new("FinalDialogueTrigger", typeof(BoxCollider2D), typeof(OldCastleDialogueTrigger));
            dialogueTriggerObject.transform.SetParent(finaleRoot.transform);
            dialogueTriggerObject.transform.position = new Vector3(1.2f, 85.2f, 0f);
            BoxCollider2D dialogueCollider = dialogueTriggerObject.GetComponent<BoxCollider2D>();
            dialogueCollider.isTrigger = true;
            dialogueCollider.size = new Vector2(3.2f, 2.2f);
            OldCastleDialogueTrigger dialogueTrigger = dialogueTriggerObject.GetComponent<OldCastleDialogueTrigger>();
            CreateWorldLabel(finaleRoot.transform, "ONE FINAL STEP", new Vector3(1.2f, 85.8f, 0f), new Color(1f, 0.78f, 0.45f, 1f));

            // Princess is physically separated by one last horizontal jump.
            GameObject princess = new("Princess_Elira", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(OldCastlePrincessGoal));
            princess.transform.SetParent(finaleRoot.transform);
            princess.transform.position = new Vector3(5.2f, 88.05f, 0f);
            princess.transform.localScale = Vector3.one * 0.92f;
            SpriteRenderer princessRenderer = princess.GetComponent<SpriteRenderer>();
            princessRenderer.sprite = princessSprite;
            princessRenderer.sortingOrder = 45;
            CircleCollider2D princessCollider = princess.GetComponent<CircleCollider2D>();
            princessCollider.isTrigger = true;
            princessCollider.radius = 0.65f;
            princessCollider.offset = new Vector2(0f, -0.12f);
            OldCastlePrincessGoal princessGoal = princess.GetComponent<OldCastlePrincessGoal>();

            if (bannerSprite != null)
                CreateDecorSprite(bannerSprite, finaleRoot.transform, "RoyalBanner", new Vector3(5.2f, 89.7f, 0f), Vector3.one * 1.25f, 18);
            CreateTorch(torchSprite, finaleRoot.transform, new Vector3(3.6f, 87.5f, 0f));
            CreateTorch(torchSprite, finaleRoot.transform, new Vector3(6.7f, 87.5f, 0f));

            OldCastleHudController hud = CreateHud(root.transform, player.transform, jumpController, inputReader, initialSpawn.y, princessSprite);
            Area3OldCastleBootstrapper bootstrapper = systemsRoot.AddComponent<Area3OldCastleBootstrapper>();
            bootstrapper.Configure(inputReader, jumpController, movementConfig, cameraFollow, player.transform, respawn, hud, dialogueTrigger, princessGoal);

            GameObject spawnPoint = new("SpawnPoint_Area3");
            spawnPoint.transform.SetParent(levelRoot.transform);
            spawnPoint.transform.position = initialSpawn;

            // Invisible side boundaries keep accidental full-charge jumps within the authored tower.
            CreateBoundary(levelRoot.transform, "Boundary_Left", new Vector3(-8.2f, 44f, 0f), new Vector2(1f, 98f));
            CreateBoundary(levelRoot.transform, "Boundary_Right", new Vector3(8.2f, 44f, 0f), new Vector2(1f, 98f));

            AddSceneToBuildSettings(ScenePath);
            EditorSceneManager.MarkSceneDirty(scene);
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/_Game/Scenes");
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (saved) Debug.Log("[Area3 Old Castle] Editable scene generated: " + ScenePath);
            return saved;
        }

        private static Sprite LoadSprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

        private static GameObject CreateRoot(string name, Transform parent)
        {
            GameObject go = new(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
            return go;
        }

        private static void CreateBackgroundLayer(Sprite sprite, Transform parent, string name, int order, Color tint, float width, float height, float centerY)
        {
            if (sprite == null) return;
            GameObject go = new(name, typeof(SpriteRenderer));
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(0f, centerY, 5f);
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            sr.color = tint;
            Vector2 size = sprite.bounds.size;
            go.transform.localScale = new Vector3(width / Mathf.Max(0.01f, size.x), height / Mathf.Max(0.01f, size.y), 1f);
        }

        private static void CreateCastleSideWalls(Sprite wall, Sprite window, Sprite arch, Transform parent)
        {
            if (wall == null) return;
            for (int i = 0; i < 16; i++)
            {
                float y = 2.2f + i * 5.7f;
                CreateDecorSprite(wall, parent, "Wall_Left_" + i, new Vector3(-7.15f, y, 0f), new Vector3(1.35f, 1.35f, 1f), 1);
                CreateDecorSprite(wall, parent, "Wall_Right_" + i, new Vector3(7.15f, y + 2.7f, 0f), new Vector3(1.35f, 1.35f, 1f), 1);
                if (window != null && i % 3 == 1)
                {
                    CreateDecorSprite(window, parent, "Window_Left_" + i, new Vector3(-6.65f, y + 1f, 0f), Vector3.one * 0.72f, 2);
                    CreateDecorSprite(window, parent, "Window_Right_" + i, new Vector3(6.65f, y + 3.4f, 0f), Vector3.one * 0.72f, 2);
                }
            }
            if (arch != null)
            {
                for (int i = 0; i < 7; i++)
                {
                    CreateDecorSprite(arch, parent, "Arch_" + i, new Vector3(i % 2 == 0 ? -5.9f : 5.9f, 8f + i * 12f, 0f), Vector3.one * 1.2f, 2);
                }
            }
        }

        private static GameObject CreatePlatform(Sprite sprite, Transform parent, PlatformSpec spec)
        {
            GameObject go = new(spec.Name, typeof(SpriteRenderer), typeof(BoxCollider2D));
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(spec.X, spec.Y, 0f);
            go.transform.localScale = new Vector3(spec.ScaleX, spec.ScaleY, 1f);
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 12;
            BoxCollider2D col = go.GetComponent<BoxCollider2D>();
            col.size = new Vector2(sprite.bounds.size.x * 0.92f, Mathf.Max(0.18f, sprite.bounds.size.y * 0.24f));
            col.offset = new Vector2(0f, sprite.bounds.size.y * 0.34f);
            return go;
        }

        private static void CreateMovingPlatform(Sprite sprite, Transform parent, PlatformSpec spec, Vector2 offset, float cycle, float phase)
        {
            GameObject go = CreatePlatform(sprite, parent, spec);
            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            OldCastleMovingPlatform moving = go.AddComponent<OldCastleMovingPlatform>();
            moving.Configure(offset, cycle, phase);
        }

        private static void CreateCheckpoint(Sprite sprite, Transform parent, OldCastleRespawnController respawn, int number, Vector3 position)
        {
            GameObject go = new("Checkpoint_" + number, typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(OldCastleCheckpoint));
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.68f;
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 24;
            CircleCollider2D col = go.GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.9f;
            go.GetComponent<OldCastleCheckpoint>().Configure(respawn, number, sr);
            CreateWorldLabel(parent, "CP " + number, position + Vector3.up * 1.45f, new Color(0.55f, 0.90f, 1f, 1f));
        }

        private static void CreateCollectible(Sprite sprite, Transform parent, Vector3 position)
        {
            GameObject go = new("Gem", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(OldCastleCollectible));
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.45f;
            go.GetComponent<SpriteRenderer>().sprite = sprite;
            go.GetComponent<SpriteRenderer>().sortingOrder = 25;
            CircleCollider2D col = go.GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.7f;
        }

        private static void CreateHazard(Sprite sprite, Transform parent, OldCastleRespawnController respawn, string name, Vector3 position, Vector3 scale)
        {
            CreateHazard(sprite, parent, respawn, name, position, scale, Vector2.zero);
        }

        private static void CreateHazard(Sprite sprite, Transform parent, OldCastleRespawnController respawn, string name, Vector3 position, Vector3 scale, Vector2 explicitSize)
        {
            GameObject go = new(name, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(OldCastleHazard));
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 20;
            BoxCollider2D col = go.GetComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = explicitSize == Vector2.zero
                ? new Vector2(Mathf.Max(0.35f, sprite.bounds.size.x * 0.75f), Mathf.Max(0.2f, sprite.bounds.size.y * 0.35f))
                : explicitSize;
            go.GetComponent<OldCastleHazard>().Configure(respawn);
        }

        private static void AddDecorations(Sprite torch, Sprite banner, Sprite chandelier, Sprite statue, Sprite window, Transform parent)
        {
            Vector3[] torchPositions =
            {
                new(-6.0f, 1.7f, 0f), new(6.0f, 8.8f, 0f), new(-5.8f, 14.4f, 0f), new(5.8f, 20.1f, 0f),
                new(-5.8f, 28.8f, 0f), new(5.9f, 37.4f, 0f), new(-5.9f, 49.1f, 0f), new(6.0f, 57.7f, 0f),
                new(-5.9f, 66.5f, 0f), new(5.9f, 75.0f, 0f), new(-5.8f, 84.2f, 0f)
            };
            foreach (Vector3 p in torchPositions) CreateTorch(torch, parent, p);

            if (banner != null)
            {
                Vector3[] bannerPositions = { new(-6.2f, 10f, 0f), new(6.1f, 24f, 0f), new(-6.1f, 39f, 0f), new(6.1f, 53f, 0f), new(-6.1f, 68f, 0f), new(6.0f, 81f, 0f) };
                foreach (Vector3 p in bannerPositions) CreateDecorSprite(banner, parent, "Banner", p, Vector3.one * 0.82f, 6);
            }
            if (chandelier != null)
            {
                CreateDecorSprite(chandelier, parent, "Chandelier_A", new Vector3(-0.5f, 32.0f, 0f), Vector3.one * 0.78f, 7);
                CreateDecorSprite(chandelier, parent, "Chandelier_B", new Vector3(1.2f, 70.5f, 0f), Vector3.one * 0.78f, 7);
            }
            if (statue != null)
            {
                CreateDecorSprite(statue, parent, "ArmorStatue_A", new Vector3(-6.0f, 43.0f, 0f), Vector3.one * 0.70f, 8);
                CreateDecorSprite(statue, parent, "ArmorStatue_B", new Vector3(6.0f, 62.0f, 0f), Vector3.one * 0.70f, 8);
            }
        }

        private static void CreateTorch(Sprite sprite, Transform parent, Vector3 position)
        {
            if (sprite == null) return;
            CreateDecorSprite(sprite, parent, "Torch", position, Vector3.one * 0.72f, 18);
        }

        private static void CreateDecorSprite(Sprite sprite, Transform parent, string name, Vector3 position, Vector3 scale, int sortingOrder)
        {
            if (sprite == null) return;
            GameObject go = new(name, typeof(SpriteRenderer));
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
        }

        private static void CreateBoundary(Transform parent, string name, Vector3 position, Vector2 size)
        {
            GameObject go = new(name, typeof(BoxCollider2D));
            go.transform.SetParent(parent);
            go.transform.position = position;
            BoxCollider2D col = go.GetComponent<BoxCollider2D>();
            col.size = size;
        }

        private static OldCastleHudController CreateHud(Transform parent, Transform player, PlayerJumpController jump, PlayerInputReader input, float startY, Sprite princessPortrait)
        {
            GameObject eventSystemObject = new("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystemObject.transform.SetParent(parent);

            GameObject canvasObject = new("Area3_OldCastle_UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(OldCastleHudController));
            canvasObject.transform.SetParent(parent);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Font font = GetDefaultFont();
            Color panel = new(0.035f, 0.025f, 0.055f, 0.91f);
            Color trim = new(0.82f, 0.62f, 0.32f, 1f);
            Color text = new(0.96f, 0.90f, 0.78f, 1f);

            GameObject titlePanel = CreatePanel(canvasObject.transform, "TitlePanel", new Vector2(390f, 142f), new Vector2(18f, -18f), new Vector2(0f, 1f), new Vector2(0f, 1f), panel);
            CreateText(titlePanel.transform, "GameTitle", "FALLEN SPIRES", font, 31, new Color(1f, 0.78f, 0.44f, 1f), TextAnchor.MiddleLeft,
                new Vector2(330f, 40f), new Vector2(20f, -22f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            CreateText(titlePanel.transform, "AreaTitle", "AREA 3 — OLD CASTLE", font, 19, text, TextAnchor.MiddleLeft,
                new Vector2(330f, 32f), new Vector2(20f, -60f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            Text height = CreateText(titlePanel.transform, "HeightText", "HEIGHT  000m", font, 18, text, TextAnchor.MiddleLeft,
                new Vector2(175f, 28f), new Vector2(20f, -102f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            Text best = CreateText(titlePanel.transform, "BestText", "BEST  000m", font, 18, text, TextAnchor.MiddleLeft,
                new Vector2(160f, 28f), new Vector2(200f, -102f), new Vector2(0f, 1f), new Vector2(0f, 1f));

            Sprite heartFull = LoadSprite(UiHeartFullPath);
            Sprite heartEmpty = LoadSprite(UiHeartEmptyPath);
            for (int i = 0; i < 5; i++)
            {
                Sprite h = i < 4 ? heartFull : heartEmpty;
                if (h != null) CreateUIImage(canvasObject.transform, "Heart_" + (i + 1), h, new Vector2(38f, 38f), new Vector2(430f + i * 43f, -27f), new Vector2(0f, 1f));
            }

            GameObject timerPanel = CreatePanel(canvasObject.transform, "TimerPanel", new Vector2(240f, 62f), new Vector2(0f, -18f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), panel);
            Text timer = CreateText(timerPanel.transform, "TimerText", "00:00.00", font, 30, text, TextAnchor.MiddleCenter,
                new Vector2(220f, 50f), Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

            Text gems = CreateText(canvasObject.transform, "GemText", "GEM  00", font, 20, new Color(1f, 0.46f, 0.55f, 1f), TextAnchor.MiddleRight,
                new Vector2(150f, 38f), new Vector2(-190f, -28f), new Vector2(1f, 1f), new Vector2(1f, 1f));

            Sprite pauseSprite = LoadSprite(UiPausePath);
            Sprite settingsSprite = LoadSprite(UiSettingsPath);
            Button pause = pauseSprite != null
                ? CreateIconButton(canvasObject.transform, "PauseButton", pauseSprite, new Vector2(70f, 70f), new Vector2(-110f, -20f), new Vector2(1f, 1f), panel)
                : CreateButton(canvasObject.transform, "PauseButton", "II", font, new Vector2(70f, 70f), new Vector2(-110f, -20f), new Vector2(1f, 1f), panel, trim, text);
            Button settings = settingsSprite != null
                ? CreateIconButton(canvasObject.transform, "SettingsButton", settingsSprite, new Vector2(70f, 70f), new Vector2(-28f, -20f), new Vector2(1f, 1f), panel)
                : CreateButton(canvasObject.transform, "SettingsButton", "SET", font, new Vector2(70f, 70f), new Vector2(-28f, -20f), new Vector2(1f, 1f), panel, trim, text);

            CreateText(canvasObject.transform, "ChargeLabel", "CHARGE", font, 22, text, TextAnchor.MiddleCenter,
                new Vector2(180f, 32f), new Vector2(0f, 102f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            Slider charge = CreateSlider(canvasObject.transform, "ChargeSlider", new Vector2(470f, 28f), new Vector2(0f, 68f), trim);
            CreateText(canvasObject.transform, "ControlsHint", "A / D  AIM     HOLD SPACE  CHARGE     RELEASE  JUMP", font, 18,
                new Color(0.91f, 0.87f, 0.79f, 0.94f), TextAnchor.MiddleLeft,
                new Vector2(620f, 40f), new Vector2(20f, 20f), new Vector2(0f, 0f), new Vector2(0f, 0f));

            // Dialogue overlay: appears on the penultimate platform and intentionally does not pause.
            GameObject dialoguePanel = CreatePanel(canvasObject.transform, "PrincessDialogue", new Vector2(1100f, 205f), new Vector2(0f, 115f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Color(0.025f, 0.025f, 0.045f, 0.96f));
            if (princessPortrait != null)
            {
                CreateUIImage(dialoguePanel.transform, "PrincessPortrait", princessPortrait, new Vector2(150f, 150f), new Vector2(30f, 25f), new Vector2(0f, 0f));
            }
            Text dialogueName = CreateText(dialoguePanel.transform, "Speaker", "Princess Elira", font, 27, new Color(1f, 0.77f, 0.44f, 1f), TextAnchor.MiddleLeft,
                new Vector2(790f, 42f), new Vector2(205f, 142f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            Text dialogueBody = CreateText(dialoguePanel.transform, "Dialogue", "You made it...\nOne final step, brave soul.", font, 25, text, TextAnchor.UpperLeft,
                new Vector2(790f, 104f), new Vector2(205f, 38f), new Vector2(0f, 0f), new Vector2(0f, 0f));

            GameObject pausePanel = CreateFullscreenOverlay(canvasObject.transform, "PausePanel", new Color(0.015f, 0.01f, 0.025f, 0.82f));
            CreateText(pausePanel.transform, "PauseTitle", "AREA 3 — OLD CASTLE\nPAUSED", font, 42, text, TextAnchor.MiddleCenter,
                new Vector2(600f, 120f), new Vector2(0f, 200f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            Button resume = CreateButton(pausePanel.transform, "ResumeButton", "RESUME", font, new Vector2(330f, 64f), new Vector2(0f, 55f), new Vector2(0.5f, 0.5f), panel, trim, text);
            Button restart = CreateButton(pausePanel.transform, "RestartButton", "RESTART CASTLE", font, new Vector2(330f, 64f), new Vector2(0f, -25f), new Vector2(0.5f, 0.5f), panel, trim, text);

            GameObject settingsPanel = CreateFullscreenOverlay(canvasObject.transform, "SettingsPanel", new Color(0.015f, 0.01f, 0.025f, 0.86f));
            CreateText(settingsPanel.transform, "InfoTitle", "AREA 3 — OLD CASTLE", font, 42, text, TextAnchor.MiddleCenter,
                new Vector2(650f, 70f), new Vector2(0f, 220f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            CreateText(settingsPanel.transform, "InfoBody",
                "DIFFICULTY: MEDIUM–HIGH\n\nSmaller platforms.\nRepeated horizontal crossings.\nMoving platforms and traps punish weak aim.\nReach the final ledge to hear Princess Elira, then make one last jump to touch her.",
                font, 25, new Color(0.88f, 0.84f, 1f, 1f), TextAnchor.MiddleCenter,
                new Vector2(820f, 330f), new Vector2(0f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            Button closeSettings = CreateButton(settingsPanel.transform, "CloseSettingsButton", "BACK", font, new Vector2(300f, 64f), new Vector2(0f, -230f), new Vector2(0.5f, 0.5f), panel, trim, text);

            GameObject completePanel = CreateFullscreenOverlay(canvasObject.transform, "CompletePanel", new Color(0.015f, 0.01f, 0.025f, 0.90f));
            CreateText(completePanel.transform, "CompleteTitle", "YOU REACHED PRINCESS ELIRA\nOLD CASTLE COMPLETE", font, 43,
                new Color(1f, 0.78f, 0.42f, 1f), TextAnchor.MiddleCenter,
                new Vector2(860f, 150f), new Vector2(0f, 205f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            Text completeStats = CreateText(completePanel.transform, "CompleteStats", "TIME\nHEIGHT\nGEMS", font, 28, text, TextAnchor.MiddleCenter,
                new Vector2(520f, 190f), new Vector2(0f, 15f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            Button completeRestart = CreateButton(completePanel.transform, "CompleteRestartButton", "PLAY AGAIN", font, new Vector2(320f, 64f), new Vector2(0f, -140f), new Vector2(0.5f, 0.5f), panel, trim, text);
            Button completeMenu = CreateButton(completePanel.transform, "CompleteMenuButton", "MAIN MENU", font, new Vector2(320f, 64f), new Vector2(0f, -220f), new Vector2(0.5f, 0.5f), panel, trim, text);

            OldCastleHudController hud = canvasObject.GetComponent<OldCastleHudController>();
            hud.Configure(player, jump, input, startY, height, best, timer, gems, charge,
                dialoguePanel, dialogueName, dialogueBody, pause, settings,
                pausePanel, settingsPanel, completePanel, completeStats,
                resume, restart, closeSettings, completeRestart, completeMenu);

            dialoguePanel.SetActive(false);
            pausePanel.SetActive(false);
            settingsPanel.SetActive(false);
            completePanel.SetActive(false);
            return hud;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 size, Vector2 anchoredPosition, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin == anchorMax ? anchorMin : new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static GameObject CreateFullscreenOverlay(Transform parent, string name, Color color)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static Text CreateText(Transform parent, string name, string value, Font font, int fontSize, Color color,
            TextAnchor alignment, Vector2 size, Vector2 anchoredPosition, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin == anchorMax ? anchorMin : new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            Text t = go.GetComponent<Text>();
            t.text = value;
            t.font = font;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = alignment;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private static Image CreateUIImage(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 anchoredPosition, Vector2 anchor)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        private static Button CreateIconButton(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 anchoredPosition, Vector2 anchor, Color background)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.color = Color.white;
            Button button = go.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.85f, 0.62f, 1f);
            colors.pressedColor = new Color(0.72f, 0.55f, 0.38f, 1f);
            button.colors = colors;
            return button;
        }

        private static Button CreateButton(Transform parent, string name, string label, Font font, Vector2 size,
            Vector2 anchoredPosition, Vector2 anchor, Color background, Color trim, Color textColor)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            Image image = go.GetComponent<Image>();
            image.color = background;
            Button button = go.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = background;
            colors.highlightedColor = new Color(Mathf.Min(1f, background.r + 0.12f), Mathf.Min(1f, background.g + 0.10f), Mathf.Min(1f, background.b + 0.06f), background.a);
            colors.pressedColor = new Color(trim.r * 0.55f, trim.g * 0.45f, trim.b * 0.35f, 1f);
            button.colors = colors;
            Text t = CreateText(go.transform, "Label", label, font, Mathf.RoundToInt(Mathf.Clamp(size.y * 0.36f, 18f, 30f)), textColor,
                TextAnchor.MiddleCenter, size, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            t.raycastTarget = false;
            return button;
        }

        private static Slider CreateSlider(Transform parent, string name, Vector2 size, Vector2 anchoredPosition, Color trim)
        {
            GameObject root = new(name, typeof(RectTransform), typeof(Slider));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            GameObject bg = new("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bg.transform.SetParent(root.transform, false);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0.035f, 0.025f, 0.055f, 0.96f);

            GameObject fillArea = new("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(root.transform, false);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(5f, 5f);
            fillAreaRect.offsetMax = new Vector2(-5f, -5f);

            GameObject fill = new("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fill.GetComponent<Image>().color = new Color(1f, 0.53f, 0.13f, 1f);

            Slider slider = root.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            slider.fillRect = fillRect;
            slider.targetGraphic = fill.GetComponent<Image>();
            slider.interactable = false;
            return slider;
        }

        private static Font GetDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return font;
        }

        private static void CreateWorldLabel(Transform parent, string label, Vector3 position, Color color)
        {
            GameObject go = new(label.Replace(' ', '_'), typeof(TextMesh));
            go.transform.SetParent(parent);
            go.transform.position = position;
            TextMesh text = go.GetComponent<TextMesh>();
            text.text = label;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = 0.11f;
            text.fontSize = 46;
            text.color = color;
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.sortingOrder = 30;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = new(EditorBuildSettings.scenes);
            if (!scenes.Exists(s => s.path == scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        private enum PlatformKind { Wide, Mid, Small, Broken }

        private readonly struct PlatformSpec
        {
            public readonly string Name;
            public readonly float X;
            public readonly float Y;
            public readonly float ScaleX;
            public readonly float ScaleY;
            public readonly PlatformKind Kind;

            public PlatformSpec(string name, float x, float y, float scaleX, float scaleY, PlatformKind kind)
            {
                Name = name;
                X = x;
                Y = y;
                ScaleX = scaleX;
                ScaleY = scaleY;
                Kind = kind;
            }
        }
    }

    [InitializeOnLoad]
    internal static class Area3OldCastleAutoInstaller
    {
        static Area3OldCastleAutoInstaller()
        {
            EditorApplication.delayCall += TryInstall;
        }

        private static void TryInstall()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (File.Exists(Area3OldCastleSceneBuilder.ScenePath)) return;
            if (AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Area3_OldCastle/Art/Terrain/platform_01.png") == null) return;
            Area3OldCastleSceneBuilder.BuildScene(false);
        }
    }
}

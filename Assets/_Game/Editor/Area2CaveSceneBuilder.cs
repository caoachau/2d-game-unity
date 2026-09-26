using System.Collections.Generic;
using System.IO;
using Summit.Game.Area2Cave;
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
    /// Generates an editable vertical Area 2 - Cave scene using the cave assets
    /// shipped with this package. The scene is intentionally built from ordinary
    /// GameObjects, SpriteRenderers, Collider2D components and UI objects so the
    /// designer can move/duplicate everything in the Unity Editor afterward.
    /// </summary>
    public static class Area2CaveSceneBuilder
    {
        public const string ScenePath = "Assets/_Game/Scenes/Area2_Cave.unity";
        private const string ArtRoot = "Assets/_Game/Area2_Cave/Art/";
        private const string PlayerPrefabPath = "Assets/_Game/Prefabs/Player/Player.prefab";
        private const string MovementConfigPath = "Assets/_Game/ScriptableObjects/Player/PlayerMovementConfig.asset";
        private const string GeneratedRootName = "Area2_Cave_GENERATED_v1";

        private const string PlatformPath = ArtRoot + "platform1.png";
        // Keep every area on the same Fallen Spires player sprite.
        // The old Cave image could render with an unwanted masked look in-game.
        private const string PlayerPath = "Assets/_Game/GeneratedAssets/FallenSpires_AllAssets_From2Screens/02_Gameplay/SeparatedAssets/Character/idle_01.png";
        private const string CheckpointPath = ArtRoot + "checkpoint.png";
        private const string CrystalPath = ArtRoot + "crystal.png";
        private const string TorchPath = ArtRoot + "torch.png";
        private const string BackdropPath = ArtRoot + "cave_backdrop.png";

        private const string SpikesPath = "Assets/_Game/GeneratedAssets/FallenSpires_AllAssets_From2Screens/02_Gameplay/SeparatedAssets/Environment/Special/spikes_long.png";
        private const string HangingPlatformPath = "Assets/_Game/GeneratedAssets/FallenSpires_AllAssets_From2Screens/02_Gameplay/SeparatedAssets/Environment/Platforms/hanging_platform.png";
        private const string CagePath = "Assets/_Game/GeneratedAssets/FallenSpires_AllAssets_From2Screens/02_Gameplay/SeparatedAssets/Environment/Special/cage.png";
        private const string LanternPath = "Assets/_Game/GeneratedAssets/FallenSpires_AllAssets_From2Screens/02_Gameplay/SeparatedAssets/Props/lantern_hanging_01.png";

        [MenuItem("Fallen Spires/Area 2 - Cave/Create or Rebuild Cave Scene", priority = 1)]
        public static void BuildFromMenu()
        {
            if (File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("Rebuild Area 2 - Cave",
                    "Area2_Cave.unity already exists. Rebuilding will replace that scene file. Continue?",
                    "Rebuild", "Cancel"))
            {
                return;
            }

            BuildScene(true);
        }

        [MenuItem("Fallen Spires/Area 2 - Cave/Open Cave Art Folder", priority = 20)]
        public static void OpenArtFolder()
        {
            Object folder = AssetDatabase.LoadAssetAtPath<Object>("Assets/_Game/Area2_Cave/Art");
            if (folder != null)
            {
                Selection.activeObject = folder;
                EditorGUIUtility.PingObject(folder);
            }
        }

        public static bool BuildScene(bool openWhenFinished)
        {
            Sprite platformSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PlatformPath);
            Sprite playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PlayerPath);
            Sprite checkpointSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CheckpointPath);
            Sprite crystalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CrystalPath);
            Sprite torchSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TorchPath);
            Sprite backdropSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackdropPath);
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            PlayerMovementConfig movementConfig = AssetDatabase.LoadAssetAtPath<PlayerMovementConfig>(MovementConfigPath);

            if (platformSprite == null || playerSprite == null || checkpointSprite == null ||
                crystalSprite == null || torchSprite == null || backdropSprite == null ||
                playerPrefab == null || movementConfig == null)
            {
                Debug.LogError("[Area2 Cave] Required assets have not finished importing. Wait for Unity to finish importing, then run Fallen Spires > Area 2 - Cave > Create or Rebuild Cave Scene.");
                return false;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject generatedRoot = new(GeneratedRootName);
            GameObject systemsRoot = CreateRoot("Systems", generatedRoot.transform);
            GameObject levelRoot = CreateRoot("Level_Area2_Cave", generatedRoot.transform);
            GameObject backgroundRoot = CreateRoot("Background", levelRoot.transform);
            GameObject platformsRoot = CreateRoot("Platforms", levelRoot.transform);
            GameObject movingPlatformsRoot = CreateRoot("MovingPlatforms", levelRoot.transform);
            GameObject checkpointsRoot = CreateRoot("Checkpoints", levelRoot.transform);
            GameObject crystalsRoot = CreateRoot("Crystals", levelRoot.transform);
            GameObject decorationsRoot = CreateRoot("Decorations", levelRoot.transform);
            GameObject hazardsRoot = CreateRoot("Hazards", levelRoot.transform);
            GameObject goalRoot = CreateRoot("Goal", levelRoot.transform);

            // Camera + dark cave backdrop.
            GameObject cameraObject = new("Main Camera", typeof(UnityEngine.Camera), typeof(AudioListener), typeof(VerticalCameraFollow));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(generatedRoot.transform);
            UnityEngine.Camera camera = cameraObject.GetComponent<UnityEngine.Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7.8f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.04f, 0.075f, 1f);
            cameraObject.transform.position = new Vector3(-3f, 3f, -10f);
            VerticalCameraFollow cameraFollow = cameraObject.GetComponent<VerticalCameraFollow>();
            SerializedObject cameraFollowSo = new(cameraFollow);
            cameraFollowSo.FindProperty("framingOffset").vector2Value = new Vector2(0f, 2.1f);
            cameraFollowSo.FindProperty("riseSmoothTime").floatValue = 0.18f;
            cameraFollowSo.FindProperty("fallSmoothTime").floatValue = 0.08f;
            cameraFollowSo.FindProperty("lockHorizontal").boolValue = false;
            cameraFollowSo.ApplyModifiedPropertiesWithoutUndo();

            CreateBackdrop(backdropSprite, backgroundRoot.transform);
            CreateFarRockSilhouettes(platformSprite, backgroundRoot.transform);

            // Player.
            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            player.name = "Player_Area2";
            player.transform.SetParent(generatedRoot.transform);
            // The player collider must start fully above the shelf so respawning
            // cannot resolve the overlap by pushing the player off its edge.
            Vector3 initialSpawn = new(-4.7f, 1.85f, 0f);
            player.transform.position = initialSpawn;
            SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();
            if (playerRenderer != null)
            {
                playerRenderer.sprite = playerSprite;
                playerRenderer.sortingOrder = 30;
                playerRenderer.color = Color.white;
                playerRenderer.maskInteraction = SpriteMaskInteraction.None;
            }

            PlayerJumpController jumpController = player.GetComponent<PlayerJumpController>();
            if (jumpController == null)
            {
                Debug.LogError("[Area2 Cave] Player prefab is missing PlayerJumpController.");
                return false;
            }

            PlayerInputReader inputReader = systemsRoot.AddComponent<PlayerInputReader>();
            CaveRespawnController respawn = systemsRoot.AddComponent<CaveRespawnController>();
            respawn.Configure(player.transform, initialSpawn, -7f);

            // Main fixed route. Larger gaps than Area 1; three safe checkpoint shelves.
            PlatformSpec[] fixedPlatforms =
            {
                new("StartShelf",        -4.7f,  0.0f, 2.35f, 1.00f),
                new("P01",               -1.8f,  2.7f, 1.15f, 0.88f),
                new("P02",                2.1f,  5.7f, 1.35f, 0.88f),
                new("P03",                4.7f,  8.8f, 1.05f, 0.86f),
                new("P04",                0.7f, 11.7f, 1.45f, 0.90f),
                new("CheckpointShelf_1", -3.4f, 14.6f, 2.35f, 0.95f),
                new("P05",                0.2f, 17.7f, 1.00f, 0.86f),
                new("P06",               -3.8f, 23.8f, 1.20f, 0.86f),
                new("P07",                0.3f, 26.8f, 1.05f, 0.84f),
                new("CheckpointShelf_2", -2.2f, 29.8f, 2.35f, 0.95f),
                new("P08",                2.5f, 33.0f, 1.10f, 0.84f),
                new("P09",                5.0f, 36.0f, 1.15f, 0.86f),
                new("P10",               -2.1f, 42.0f, 1.15f, 0.84f),
                new("P11",               -5.0f, 45.0f, 1.20f, 0.88f),
                new("CheckpointShelf_3", -1.2f, 48.0f, 2.45f, 0.95f),
                new("P12",                3.0f, 51.2f, 1.10f, 0.84f),
                new("P13",                5.1f, 54.2f, 1.00f, 0.82f),
                new("P14",                1.5f, 57.0f, 1.35f, 0.88f),
                new("ExitShelf",         -2.1f, 60.0f, 2.70f, 1.00f)
            };

            foreach (PlatformSpec spec in fixedPlatforms)
            {
                CreatePlatform(platformSprite, platformsRoot.transform, spec);
            }

            // Two moving platforms create the main medium-difficulty timing checks.
            CreateMovingPlatform(platformSprite, movingPlatformsRoot.transform,
                new PlatformSpec("Moving_A", 3.4f, 20.8f, 1.25f, 0.82f), new Vector2(-3.0f, 0f), 3.6f, 0f);
            CreateMovingPlatform(platformSprite, movingPlatformsRoot.transform,
                new PlatformSpec("Moving_B", 1.7f, 39.0f, 1.20f, 0.82f), new Vector2(-3.2f, 0f), 4.2f, 0.35f);

            // Alternate optional hanging platform visuals from the generated pack when available.
            Sprite hangingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(HangingPlatformPath);
            if (hangingSprite != null)
            {
                CreateDecorSprite(hangingSprite, decorationsRoot.transform, "HangingBridge_Optional", new Vector3(4.8f, 28.1f, 0f), new Vector3(1.15f, 1.15f, 1f), 3);
                CreateDecorSprite(hangingSprite, decorationsRoot.transform, "HangingBridge_Optional_2", new Vector3(-4.7f, 52.4f, 0f), new Vector3(1.1f, 1.1f, 1f), 3);
            }

            // Checkpoints placed on broad safe shelves.
            CreateCheckpoint(checkpointSprite, checkpointsRoot.transform, respawn, 1, new Vector3(-3.4f, 15.9f, 0f));
            CreateCheckpoint(checkpointSprite, checkpointsRoot.transform, respawn, 2, new Vector3(-2.2f, 31.1f, 0f));
            CreateCheckpoint(checkpointSprite, checkpointsRoot.transform, respawn, 3, new Vector3(-1.2f, 49.3f, 0f));

            // Collectibles encourage deliberate long jumps but are not required.
            Vector3[] crystalPositions =
            {
                new(-1.8f, 4.0f, 0f), new(2.1f, 7.0f, 0f), new(4.7f, 10.1f, 0f),
                new(0.2f, 19.0f, 0f), new(-3.8f, 25.1f, 0f), new(2.5f, 34.3f, 0f),
                new(5.0f, 37.3f, 0f), new(-2.1f, 43.3f, 0f), new(3.0f, 52.5f, 0f),
                new(1.5f, 58.3f, 0f)
            };
            foreach (Vector3 position in crystalPositions)
            {
                CreateCrystal(crystalSprite, crystalsRoot.transform, position);
            }

            // Torches and cages make the cave readable without cluttering collision geometry.
            Vector3[] torchPositions =
            {
                new(-6.6f, 1.7f, 0f), new(6.4f, 8.6f, 0f), new(-5.8f, 14.9f, 0f),
                new(5.9f, 22.4f, 0f), new(-5.9f, 30.0f, 0f), new(6.0f, 37.0f, 0f),
                new(-6.2f, 46.1f, 0f), new(5.9f, 54.6f, 0f), new(-4.5f, 60.8f, 0f)
            };
            foreach (Vector3 position in torchPositions)
            {
                CreateTorch(torchSprite, decorationsRoot.transform, position);
            }

            Sprite cageSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CagePath);
            Sprite lanternSprite = AssetDatabase.LoadAssetAtPath<Sprite>(LanternPath);
            if (cageSprite != null)
            {
                CreateDecorSprite(cageSprite, decorationsRoot.transform, "HangingCage_A", new Vector3(5.9f, 18.4f, 0f), Vector3.one * 0.8f, 5);
                CreateDecorSprite(cageSprite, decorationsRoot.transform, "HangingCage_B", new Vector3(-5.5f, 38.0f, 0f), Vector3.one * 0.85f, 5);
            }
            if (lanternSprite != null)
            {
                CreateDecorSprite(lanternSprite, decorationsRoot.transform, "Lantern_A", new Vector3(-5.8f, 34.5f, 0f), Vector3.one * 0.85f, 6);
                CreateDecorSprite(lanternSprite, decorationsRoot.transform, "Lantern_B", new Vector3(5.8f, 47.0f, 0f), Vector3.one * 0.85f, 6);
            }

            // A few spike hazards introduce precision while preserving generous recovery below.
            Sprite spikesSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpikesPath);
            if (spikesSprite != null)
            {
                CreateHazard(spikesSprite, hazardsRoot.transform, respawn, "Spikes_01", new Vector3(1.0f, 15.45f, 0f), new Vector3(0.85f, 0.7f, 1f));
                CreateHazard(spikesSprite, hazardsRoot.transform, respawn, "Spikes_02", new Vector3(3.7f, 36.9f, 0f), new Vector3(0.75f, 0.65f, 1f));
                CreateHazard(spikesSprite, hazardsRoot.transform, respawn, "Spikes_03", new Vector3(-3.3f, 48.85f, 0f), new Vector3(0.75f, 0.65f, 1f));
            }

            // Exit trigger on the final shelf.
            GameObject exitObject = new("Exit_To_Area3", typeof(BoxCollider2D), typeof(CaveExitGoal));
            exitObject.transform.SetParent(goalRoot.transform);
            exitObject.transform.position = new Vector3(-2.1f, 61.6f, 0f);
            BoxCollider2D exitCollider = exitObject.GetComponent<BoxCollider2D>();
            exitCollider.isTrigger = true;
            exitCollider.size = new Vector2(4.5f, 3.2f);
            CaveExitGoal exitGoal = exitObject.GetComponent<CaveExitGoal>();
            CreateWorldLabel(goalRoot.transform, "EXIT  •  AREA 3", new Vector3(-2.1f, 62.7f, 0f));
            CreateTorch(torchSprite, decorationsRoot.transform, new Vector3(-4.2f, 61.0f, 0f));
            CreateTorch(torchSprite, decorationsRoot.transform, new Vector3(0.0f, 61.0f, 0f));

            CaveHudController hud = CreateHud(generatedRoot.transform, player.transform, jumpController, inputReader, initialSpawn.y);

            Area2CaveBootstrapper bootstrapper = systemsRoot.AddComponent<Area2CaveBootstrapper>();
            bootstrapper.Configure(inputReader, jumpController, movementConfig, cameraFollow, player.transform, respawn, hud, exitGoal);

            // Use the marker's world position so respawning follows level edits.
            GameObject spawnPoint = new("SpawnPoint_Area2");
            spawnPoint.transform.SetParent(levelRoot.transform);
            spawnPoint.transform.position = initialSpawn;
            respawn.SetInitialSpawnPoint(spawnPoint.transform);

            AddSceneToBuildSettings(ScenePath);
            EditorSceneManager.MarkSceneDirty(scene);
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/_Game/Scenes");
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (saved)
            {
                Debug.Log("[Area2 Cave] Editable scene generated: " + ScenePath);
            }

            if (!openWhenFinished)
            {
                // Keep the generated scene open: it lets the designer immediately inspect the result
                // after the first project import. This intentionally avoids silently restoring another scene.
            }

            return saved;
        }

        private static GameObject CreateRoot(string name, Transform parent)
        {
            GameObject go = new(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
            return go;
        }

        private static void CreateBackdrop(Sprite sprite, Transform parent)
        {
            GameObject go = new("CaveBackdrop", typeof(SpriteRenderer));
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(0f, 30f, 5f);
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -100;
            renderer.color = new Color(0.70f, 0.82f, 1f, 1f);
            Vector2 size = sprite.bounds.size;
            go.transform.localScale = new Vector3(17.5f / Mathf.Max(0.01f, size.x), 72f / Mathf.Max(0.01f, size.y), 1f);
        }

        private static void CreateFarRockSilhouettes(Sprite platformSprite, Transform parent)
        {
            for (int i = 0; i < 11; i++)
            {
                float y = i * 6.2f + 1.5f;
                CreateTintedRock(platformSprite, parent, "FarRock_L_" + i, new Vector3(-7.7f, y, 0f), new Vector3(2.8f, 2.2f, 1f));
                CreateTintedRock(platformSprite, parent, "FarRock_R_" + i, new Vector3(7.7f, y + 2.4f, 0f), new Vector3(2.6f, 2.0f, 1f));
            }
        }

        private static void CreateTintedRock(Sprite sprite, Transform parent, string name, Vector3 position, Vector3 scale)
        {
            GameObject go = new(name, typeof(SpriteRenderer));
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -80;
            renderer.color = new Color(0.12f, 0.18f, 0.28f, 0.65f);
        }

        private static GameObject CreatePlatform(Sprite sprite, Transform parent, PlatformSpec spec)
        {
            GameObject go = new(spec.Name, typeof(SpriteRenderer), typeof(BoxCollider2D));
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(spec.X, spec.Y, 0f);
            go.transform.localScale = new Vector3(spec.ScaleX, spec.ScaleY, 1f);

            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 10;
            renderer.color = new Color(0.92f, 0.96f, 1f, 1f);

            BoxCollider2D collider = go.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(sprite.bounds.size.x * 0.92f, sprite.bounds.size.y * 0.48f);
            collider.offset = new Vector2(0f, sprite.bounds.size.y * 0.22f);
            return go;
        }

        private static void CreateMovingPlatform(Sprite sprite, Transform parent, PlatformSpec spec, Vector2 offset, float cycle, float phase)
        {
            GameObject go = CreatePlatform(sprite, parent, spec);
            go.name = spec.Name;
            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            CaveMovingPlatform moving = go.AddComponent<CaveMovingPlatform>();
            moving.Configure(offset, cycle, phase);
        }

        private static void CreateCheckpoint(Sprite sprite, Transform parent, CaveRespawnController respawn, int number, Vector3 position)
        {
            GameObject go = new("Checkpoint_" + number, typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(CaveCheckpoint));
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.82f;
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 20;
            CircleCollider2D trigger = go.GetComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.72f;
            go.GetComponent<CaveCheckpoint>().Configure(respawn, number, renderer);
        }

        private static void CreateCrystal(Sprite sprite, Transform parent, Vector3 position)
        {
            GameObject go = new("Crystal", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(CaveCrystalPickup));
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.42f;
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 22;
            CircleCollider2D trigger = go.GetComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.65f;
        }

        private static void CreateTorch(Sprite sprite, Transform parent, Vector3 position)
        {
            GameObject go = new("Torch", typeof(SpriteRenderer));
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.55f;
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 18;
        }

        private static void CreateDecorSprite(Sprite sprite, Transform parent, string name, Vector3 position, Vector3 scale, int sortingOrder)
        {
            GameObject go = new(name, typeof(SpriteRenderer));
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
        }

        private static void CreateHazard(Sprite sprite, Transform parent, CaveRespawnController respawn, string name, Vector3 position, Vector3 scale)
        {
            GameObject go = new(name, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(CaveHazard));
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 15;
            BoxCollider2D trigger = go.GetComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(Mathf.Max(0.4f, sprite.bounds.size.x * 0.8f), Mathf.Max(0.2f, sprite.bounds.size.y * 0.35f));
            go.GetComponent<CaveHazard>().Configure(respawn);
        }

        private static CaveHudController CreateHud(Transform parent, Transform player, PlayerJumpController jump, PlayerInputReader input, float startY)
        {
            GameObject eventSystemObject = new("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystemObject.transform.SetParent(parent);

            GameObject canvasObject = new("Area2_Cave_UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CaveHudController));
            canvasObject.transform.SetParent(parent);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Font font = GetDefaultFont();
            Color panelColor = new(0.025f, 0.035f, 0.055f, 0.90f);
            Color trim = new(0.82f, 0.68f, 0.39f, 1f);
            Color textColor = new(0.96f, 0.92f, 0.80f, 1f);

            GameObject statsPanel = CreatePanel(canvasObject.transform, "StatsPanel", new Vector2(330f, 112f), new Vector2(18f, -18f), new Vector2(0f, 1f), new Vector2(0f, 1f), panelColor);
            Text height = CreateText(statsPanel.transform, "HeightText", "HEIGHT  000m", font, 28, textColor, TextAnchor.MiddleLeft,
                new Vector2(295f, 42f), new Vector2(18f, -28f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            Text best = CreateText(statsPanel.transform, "BestText", "BEST    000m", font, 24, textColor, TextAnchor.MiddleLeft,
                new Vector2(295f, 36f), new Vector2(18f, -75f), new Vector2(0f, 1f), new Vector2(0f, 1f));

            GameObject timerPanel = CreatePanel(canvasObject.transform, "TimerPanel", new Vector2(245f, 64f), new Vector2(0f, -18f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), panelColor);
            Text timer = CreateText(timerPanel.transform, "TimerText", "00:00.00", font, 31, textColor, TextAnchor.MiddleCenter,
                new Vector2(220f, 50f), Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

            GameObject crystalPanel = CreatePanel(canvasObject.transform, "CrystalPanel", new Vector2(190f, 52f), new Vector2(-175f, -25f), new Vector2(1f, 1f), new Vector2(1f, 1f), panelColor);
            Text crystals = CreateText(crystalPanel.transform, "CrystalText", "CRYSTAL  00", font, 21, new Color(0.55f, 0.90f, 1f, 1f), TextAnchor.MiddleCenter,
                new Vector2(175f, 42f), Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

            Button pause = CreateButton(canvasObject.transform, "PauseButton", "II", font, new Vector2(72f, 72f), new Vector2(-112f, -25f), new Vector2(1f, 1f), panelColor, trim, textColor);
            Button settings = CreateButton(canvasObject.transform, "SettingsButton", "⚙", font, new Vector2(72f, 72f), new Vector2(-28f, -25f), new Vector2(1f, 1f), panelColor, trim, textColor);

            Text chargeLabel = CreateText(canvasObject.transform, "ChargeLabel", "CHARGE", font, 23, textColor, TextAnchor.MiddleCenter,
                new Vector2(180f, 34f), new Vector2(0f, 104f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            Slider charge = CreateSlider(canvasObject.transform, "ChargeSlider", new Vector2(470f, 30f), new Vector2(0f, 70f), trim);
            chargeLabel.transform.SetAsLastSibling();

            CreateText(canvasObject.transform, "ControlsHint", "A / D   AIM     •     HOLD SPACE   CHARGE     •     RELEASE   JUMP", font, 19,
                new Color(0.90f, 0.88f, 0.80f, 0.92f), TextAnchor.MiddleLeft,
                new Vector2(650f, 42f), new Vector2(22f, 22f), new Vector2(0f, 0f), new Vector2(0f, 0f));

            GameObject pausePanel = CreateFullscreenOverlay(canvasObject.transform, "PausePanel", new Color(0.01f, 0.015f, 0.025f, 0.78f));
            CreateText(pausePanel.transform, "PauseTitle", "AREA 2 — CAVE\nPAUSED", font, 42, textColor, TextAnchor.MiddleCenter,
                new Vector2(520f, 130f), new Vector2(0f, 220f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            Button resume = CreateButton(pausePanel.transform, "ResumeButton", "RESUME", font, new Vector2(330f, 64f), new Vector2(0f, 60f), new Vector2(0.5f, 0.5f), panelColor, trim, textColor);
            Button restart = CreateButton(pausePanel.transform, "RestartButton", "RESTART CAVE", font, new Vector2(330f, 64f), new Vector2(0f, -20f), new Vector2(0.5f, 0.5f), panelColor, trim, textColor);
            Button pauseMenu = CreateButton(pausePanel.transform, "MainMenuButton", "MAIN MENU", font, new Vector2(330f, 64f), new Vector2(0f, -100f), new Vector2(0.5f, 0.5f), panelColor, trim, textColor);

            GameObject settingsPanel = CreateFullscreenOverlay(canvasObject.transform, "SettingsPanel", new Color(0.01f, 0.015f, 0.025f, 0.84f));
            CreateText(settingsPanel.transform, "InfoTitle", "AREA 2 — CAVE", font, 42, textColor, TextAnchor.MiddleCenter,
                new Vector2(600f, 70f), new Vector2(0f, 230f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            CreateText(settingsPanel.transform, "InfoBody",
                "DIFFICULTY: MEDIUM\n\nPlatforms are farther apart.\nChoose the jump direction before charging.\nLong gaps require stronger charge.\nThree checkpoints protect progress through the cave.",
                font, 25, new Color(0.84f, 0.90f, 1f, 1f), TextAnchor.MiddleCenter,
                new Vector2(760f, 320f), new Vector2(0f, 15f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            Button closeSettings = CreateButton(settingsPanel.transform, "CloseSettingsButton", "BACK", font, new Vector2(300f, 64f), new Vector2(0f, -220f), new Vector2(0.5f, 0.5f), panelColor, trim, textColor);

            GameObject completePanel = CreateFullscreenOverlay(canvasObject.transform, "CompletePanel", new Color(0.01f, 0.015f, 0.025f, 0.88f));
            CreateText(completePanel.transform, "CompleteTitle", "AREA 2 COMPLETE\nTHE CAVE IS CLEARED", font, 44,
                new Color(1f, 0.82f, 0.42f, 1f), TextAnchor.MiddleCenter,
                new Vector2(760f, 150f), new Vector2(0f, 220f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            Text completeStats = CreateText(completePanel.transform, "CompleteStats", "TIME\nHEIGHT\nCRYSTALS", font, 28, textColor, TextAnchor.MiddleCenter,
                new Vector2(520f, 190f), new Vector2(0f, 20f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            Button completeRestart = CreateButton(completePanel.transform, "CompleteRestartButton", "PLAY AGAIN", font, new Vector2(320f, 64f), new Vector2(0f, -135f), new Vector2(0.5f, 0.5f), panelColor, trim, textColor);
            Button completeMenu = CreateButton(completePanel.transform, "CompleteMenuButton", "MAIN MENU", font, new Vector2(320f, 64f), new Vector2(0f, -215f), new Vector2(0.5f, 0.5f), panelColor, trim, textColor);

            CaveHudController hud = canvasObject.GetComponent<CaveHudController>();
            hud.Configure(player, jump, input, startY, height, best, timer, crystals, charge, pause, settings,
                pausePanel, settingsPanel, completePanel, completeStats,
                resume, restart, pauseMenu, closeSettings, completeRestart, completeMenu);
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
            Image image = go.GetComponent<Image>();
            image.color = color;
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
            Image image = go.GetComponent<Image>();
            image.color = color;
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
            Text text = go.GetComponent<Text>();
            text.text = value;
            text.font = font;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
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
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            GameObject border = new("Border", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            border.transform.SetParent(go.transform, false);
            RectTransform borderRect = border.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(2f, 2f);
            borderRect.offsetMax = new Vector2(-2f, -2f);
            border.GetComponent<Image>().color = new Color(trim.r, trim.g, trim.b, 0.18f);
            border.transform.SetAsFirstSibling();

            Text text = CreateText(go.transform, "Label", label, font, Mathf.RoundToInt(Mathf.Clamp(size.y * 0.36f, 18f, 30f)),
                textColor, TextAnchor.MiddleCenter, size, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            text.raycastTarget = false;
            return button;
        }

        private static Slider CreateSlider(Transform parent, string name, Vector2 size, Vector2 anchoredPosition, Color gold)
        {
            GameObject root = new(name, typeof(RectTransform), typeof(Slider));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            GameObject background = new("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            background.transform.SetParent(root.transform, false);
            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            background.GetComponent<Image>().color = new Color(0.035f, 0.045f, 0.07f, 0.95f);

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
            fill.GetComponent<Image>().color = new Color(1f, 0.65f, 0.12f, 1f);

            Slider slider = root.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            slider.fillRect = fillRect;
            slider.targetGraphic = fill.GetComponent<Image>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.interactable = false;
            return slider;
        }

        private static Font GetDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            return font;
        }

        private static void CreateWorldLabel(Transform parent, string label, Vector3 position)
        {
            GameObject go = new("ExitLabel", typeof(TextMesh));
            go.transform.SetParent(parent);
            go.transform.position = position;
            TextMesh text = go.GetComponent<TextMesh>();
            text.text = label;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = 0.12f;
            text.fontSize = 48;
            text.color = new Color(1f, 0.82f, 0.45f, 1f);
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 25;
            }
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = new(EditorBuildSettings.scenes);
            if (scenes.Exists(s => s.path == scenePath))
            {
                return;
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private readonly struct PlatformSpec
        {
            public readonly string Name;
            public readonly float X;
            public readonly float Y;
            public readonly float ScaleX;
            public readonly float ScaleY;

            public PlatformSpec(string name, float x, float y, float scaleX, float scaleY)
            {
                Name = name;
                X = x;
                Y = y;
                ScaleX = scaleX;
                ScaleY = scaleY;
            }
        }
    }

    [InitializeOnLoad]
    internal static class Area2CaveAutoInstaller
    {
        static Area2CaveAutoInstaller()
        {
            EditorApplication.delayCall += TryInstall;
        }

        private static void TryInstall()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (File.Exists(Area2CaveSceneBuilder.ScenePath))
            {
                return;
            }

            // Only run when the essential cave sprites have been imported.
            if (AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Area2_Cave/Art/platform1.png") == null)
            {
                return;
            }

            Area2CaveSceneBuilder.BuildScene(false);
        }
    }
}

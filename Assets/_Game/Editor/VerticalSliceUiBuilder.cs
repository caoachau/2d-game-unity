using System.Collections.Generic;
using System.IO;
using Summit.Game.Application;
using Summit.Game.Camera;
using Summit.Game.Configuration;
using Summit.Game.Environment;
using Summit.Game.Input;
using Summit.Game.Player;
using Summit.Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Summit.Game.Editor
{
    public static class VerticalSliceUiBuilder
    {
        private const string SceneFolder = "Assets/_Game/Scenes";
        private const string ConfigFolder = "Assets/_Game/ScriptableObjects/Player";
        private const string PrefabFolder = "Assets/_Game/Prefabs/Player";

        private static readonly Color Ink = Hex("11131C");
        private static readonly Color InkSoft = Hex("202331");
        private static readonly Color Gold = Hex("DCC47C");
        private static readonly Color Cream = Hex("F4E7C5");
        private static readonly Color Red = Hex("67251F");
        private static readonly Color RedBright = Hex("91342A");
        private static readonly Color Slate = Hex("406C91");

        [MenuItem("Summit/Build Vertical Slice UI")]
        public static void BuildAll()
        {
            Directory.CreateDirectory(SceneFolder);
            Directory.CreateDirectory(ConfigFolder);
            Directory.CreateDirectory(PrefabFolder);

            ConfigureKitTextures();
            PlayerMovementConfig config = GetOrCreateMovementConfig();
            AssetDatabase.SaveAssets();
            BuildBootScene();
            BuildMainMenuScene();
            AssetDatabase.ImportAsset($"{ConfigFolder}/PlayerMovementConfig.asset", ImportAssetOptions.ForceUpdate);
            config = AssetDatabase.LoadAssetAtPath<PlayerMovementConfig>($"{ConfigFolder}/PlayerMovementConfig.asset");
            if (config == null)
            {
                throw new InvalidDataException("PlayerMovementConfig asset could not be loaded before scene authoring.");
            }
            BuildGameScene(config);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{SceneFolder}/Boot.unity", true),
                new EditorBuildSettingsScene($"{SceneFolder}/MainMenu.unity", true),
                new EditorBuildSettingsScene($"{SceneFolder}/Game.unity", true)
            };

            PlayerSettings.companyName = "Summit Studio";
            PlayerSettings.productName = "Fallen Spires";
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[VerticalSliceUiBuilder] Boot, MainMenu and Game UI scenes created.");
        }

        private static void ConfigureKitTextures()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Game/ThirdParty" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                bool dirty = importer.textureType != TextureImporterType.Sprite ||
                             importer.spritePixelsPerUnit != 16f || importer.filterMode != FilterMode.Point ||
                             importer.mipmapEnabled || importer.textureCompression != TextureImporterCompression.Uncompressed;
                if (!dirty)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 16f;
                TextureImporterSettings settings = new();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static PlayerMovementConfig GetOrCreateMovementConfig()
        {
            string path = $"{ConfigFolder}/PlayerMovementConfig.asset";
            PlayerMovementConfig config = AssetDatabase.LoadAssetAtPath<PlayerMovementConfig>(path);
            if (config != null)
            {
                return config;
            }

            config = ScriptableObject.CreateInstance<PlayerMovementConfig>();
            AssetDatabase.CreateAsset(config, path);
            return config;
        }

        private static void BuildBootScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("BootLoader").AddComponent<BootLoader>();
            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/Boot.unity");
        }

        private static void BuildMainMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Canvas canvas = CreateCanvas("MainMenuCanvas");
            Sprite background = LoadSprite("Assets/_Game/ThirdParty/FantasyForest/Mockups/MockUpWithBG.png");
            CreateStretchImage(canvas.transform, "Background", background, Color.white);
            CreateStretchImage(canvas.transform, "Atmosphere", null, new Color(0.04f, 0.04f, 0.1f, 0.34f));

            Text logo = CreateText(canvas.transform, "Logo", "FALLEN\nSPIRES", 82, Gold, TextAnchor.MiddleCenter,
                new Vector2(0.22f, 0.76f), new Vector2(0f, 50f), new Vector2(650f, 270f), FontStyle.Bold);
            logo.horizontalOverflow = HorizontalWrapMode.Overflow;
            CreateText(canvas.transform, "Tagline", "CLIMB BEYOND YESTERDAY", 25, Cream, TextAnchor.MiddleCenter,
                new Vector2(0.22f, 0.64f), Vector2.zero, new Vector2(520f, 52f));

            Button play = CreateButton(canvas.transform, "StartJourneyButton", "START JOURNEY", null,
                new Vector2(0.25f, 0.47f), Vector2.zero, new Vector2(450f, 78f), true);
            Button settings = CreateButton(canvas.transform, "SettingsButton", "SETTINGS", LoadIcon(3),
                new Vector2(0.25f, 0.37f), Vector2.zero, new Vector2(410f, 68f));
            Button quit = CreateButton(canvas.transform, "QuitButton", "QUIT", LoadIcon(2),
                new Vector2(0.25f, 0.28f), Vector2.zero, new Vector2(410f, 68f));
            Button cornerSettings = CreateIconButton(canvas.transform, "CornerSettings", LoadIcon(3),
                new Vector2(1f, 1f), new Vector2(-122f, -62f));
            Button cornerQuit = CreateIconButton(canvas.transform, "CornerQuit", LoadIcon(2),
                new Vector2(1f, 1f), new Vector2(-48f, -62f));

            CreateText(canvas.transform, "Motto", "HIGHER THINGS AWAIT", 22, Cream, TextAnchor.MiddleCenter,
                new Vector2(0.89f, 0.30f), Vector2.zero, new Vector2(210f, 120f));

            SettingsPanelView settingsPanel = CreateSettingsPanel(canvas.transform);
            settingsPanel.gameObject.SetActive(false);

            MainMenuView view = canvas.gameObject.AddComponent<MainMenuView>();
            Set(view, "playButton", play);
            Set(view, "settingsButton", settings);
            Set(view, "quitButton", quit);
            Set(view, "cornerSettingsButton", cornerSettings);
            Set(view, "cornerQuitButton", cornerQuit);
            Set(view, "settingsPanel", settingsPanel);

            GameObject bootstrap = new("MainMenuBootstrapper");
            MainMenuBootstrapper bootstrapper = bootstrap.AddComponent<MainMenuBootstrapper>();
            Set(bootstrapper, "menuView", view);
            CreateEventSystem();
            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/MainMenu.unity");
        }

        private static void BuildGameScene(PlayerMovementConfig config)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            UnityEngine.Camera camera = CreateGameCamera();
            Transform start = new GameObject("PlayerSpawn").transform;
            start.position = new Vector3(0f, 1.05f, 0f);
            GameObject player = CreatePlayer(start.position, config);
            VerticalCameraFollow cameraFollow = camera.gameObject.AddComponent<VerticalCameraFollow>();
            CreateParallaxBackground(camera.transform);

            List<Vector3> platforms = CreatePrototypeLevel();
            Vector3 goalPosition = platforms[^1] + new Vector3(0f, 1.3f, 0f);
            GoalTrigger goal = CreateGoal(goalPosition);
            CreateRespawnZone(start, player.GetComponent<Rigidbody2D>());

            GameObject systems = new("GameSystems");
            PlayerInputReader input = systems.AddComponent<PlayerInputReader>();
            GameProgressTracker progress = systems.AddComponent<GameProgressTracker>();
            GameRunTimer timer = systems.AddComponent<GameRunTimer>();

            GameUiController gameUi = CreateGameUi();
            CreateEventSystem();

            GameSceneBootstrapper bootstrapper = systems.AddComponent<GameSceneBootstrapper>();
            PlayerJumpController jump = player.GetComponent<PlayerJumpController>();
            Set(bootstrapper, "inputReader", input);
            Set(bootstrapper, "jumpController", jump);
            Set(bootstrapper, "movementConfig", config);
            Set(bootstrapper, "cameraFollow", cameraFollow);
            Set(bootstrapper, "playerTransform", player.transform);
            Set(bootstrapper, "progressTracker", progress);
            Set(bootstrapper, "runTimer", timer);
            Set(bootstrapper, "goalTrigger", goal);
            Set(bootstrapper, "gameUi", gameUi);
            Set(bootstrapper, "levelStart", start);

            PrefabUtility.SaveAsPrefabAsset(player, $"{PrefabFolder}/Player.prefab");
            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/Game.unity");
        }

        private static UnityEngine.Camera CreateGameCamera()
        {
            GameObject cameraObject = new("Main Camera");
            cameraObject.tag = "MainCamera";
            UnityEngine.Camera camera = cameraObject.AddComponent<UnityEngine.Camera>();
            cameraObject.AddComponent<AudioListener>();
            camera.orthographic = true;
            camera.orthographicSize = 7.5f;
            camera.backgroundColor = Hex("101522");
            camera.transform.position = new Vector3(0f, 3.25f, -10f);
            return camera;
        }

        private static void CreateParallaxBackground(Transform camera)
        {
            for (int i = 1; i <= 4; i++)
            {
                Sprite sprite = LoadSprite($"Assets/_Game/ThirdParty/FantasyForest/Background/BG-{i}.png");
                GameObject layer = new($"Background_{i}");
                layer.transform.SetParent(camera, false);
                layer.transform.localPosition = new Vector3(0f, 0f, 10f);
                layer.transform.localScale = Vector3.one * 0.667f;
                SpriteRenderer renderer = layer.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = -110 + i;
            }
        }

        private static GameObject CreatePlayer(Vector3 position, PlayerMovementConfig config)
        {
            GameObject player = new("Player");
            player.transform.position = position;
            SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadSprite("Assets/_Game/ThirdParty/PixelPlatformer/Character/Sprite/idle-01.png");
            renderer.sortingOrder = 10;
            renderer.color = Color.white;
            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.8f, 1.35f);
            player.AddComponent<PlayerMovement>();
            player.AddComponent<PlayerGroundDetector>();
            player.AddComponent<PlayerStateController>();
            PlayerJumpController jump = player.AddComponent<PlayerJumpController>();
            Set(jump, "config", config);
            return player;
        }

        private static List<Vector3> CreatePrototypeLevel()
        {
            (float x, float y, float width)[] layout =
            {
                (0,0,7), (3.5f,3,3.5f), (-1,6,3.2f), (-4,9.2f,3.6f), (0,12.4f,4.4f),
                (4,15.8f,3.2f), (1,19.1f,2.8f), (-3.6f,22.4f,3.1f), (0.5f,25.8f,4.6f),
                (4.4f,29.2f,2.8f), (1.2f,32.6f,2.6f), (-3.4f,36f,3.5f), (0,39.4f,4.8f),
                (4.2f,42.8f,2.6f), (1.1f,46.1f,2.4f), (-3.9f,49.5f,2.8f), (-0.4f,52.9f,3.6f),
                (3.8f,56.3f,2.5f), (0.5f,59.8f,2.2f), (-4.1f,63.1f,2.5f), (-1,66.5f,3.3f),
                (3.9f,69.9f,2.2f), (0.7f,73.3f,2.1f), (-3.6f,76.7f,2.3f), (0,80.2f,6f)
            };

            Sprite block = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            List<Vector3> positions = new();
            for (int i = 0; i < layout.Length; i++)
            {
                (float x, float y, float width) = layout[i];
                GameObject platform = new($"Platform_{i + 1:00}");
                platform.transform.position = new Vector3(x, y, 0f);
                GameObject bodyVisual = new("Body");
                bodyVisual.transform.SetParent(platform.transform, false);
                bodyVisual.transform.localScale = new Vector3(width, 0.55f, 1f);
                SpriteRenderer renderer = bodyVisual.AddComponent<SpriteRenderer>();
                renderer.sprite = block;
                renderer.color = i < 8 ? Hex("334C3E") : i < 17 ? Hex("3B3A4A") : Hex("403436");
                BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(width, 0.55f);

                GameObject trim = new("GoldTrim");
                trim.transform.SetParent(platform.transform, false);
                trim.transform.localPosition = new Vector3(0f, 0.31f, 0f);
                trim.transform.localScale = new Vector3(width, 0.08f, 1f);
                SpriteRenderer trimRenderer = trim.AddComponent<SpriteRenderer>();
                trimRenderer.sprite = block;
                trimRenderer.color = Gold;
                trimRenderer.sortingOrder = 1;
                positions.Add(platform.transform.position);
            }

            return positions;
        }

        private static GoalTrigger CreateGoal(Vector3 position)
        {
            GameObject goal = new("GoalTrigger");
            goal.transform.position = position;
            BoxCollider2D collider = goal.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(5f, 2.5f);
            SpriteRenderer renderer = goal.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadIcon3(55);
            renderer.sortingOrder = 5;
            renderer.transform.localScale = Vector3.one * 1.5f;
            return goal.AddComponent<GoalTrigger>();
        }

        private static void CreateRespawnZone(Transform spawn, Rigidbody2D playerBody)
        {
            GameObject zone = new("WorldBottomRespawn");
            zone.transform.position = new Vector3(0f, -12f, 0f);
            BoxCollider2D collider = zone.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(80f, 4f);
            RespawnZone respawn = zone.AddComponent<RespawnZone>();
            respawn.Initialize(spawn, playerBody);
        }

        private static GameUiController CreateGameUi()
        {
            Canvas canvas = CreateCanvas("GameCanvas");
            GameUiController ui = canvas.gameObject.AddComponent<GameUiController>();

            GameObject heightPanel = CreatePanel(canvas.transform, "HeightPanel", new Vector2(0f, 1f),
                new Vector2(190f, -70f), new Vector2(350f, 110f), new Color(Ink.r, Ink.g, Ink.b, 0.94f));
            Text height = CreateText(heightPanel.transform, "Height", "HEIGHT   000m", 27, Cream,
                TextAnchor.MiddleLeft, new Vector2(0.5f, 0.67f), new Vector2(25f, 0f), new Vector2(290f, 38f), FontStyle.Bold);
            Text best = CreateText(heightPanel.transform, "Best", "BEST       000m", 23, Gold,
                TextAnchor.MiddleLeft, new Vector2(0.5f, 0.30f), new Vector2(25f, 0f), new Vector2(290f, 36f));

            GameObject timerPanel = CreatePanel(canvas.transform, "TimerPanel", new Vector2(0.5f, 1f),
                new Vector2(0f, -48f), new Vector2(250f, 70f), new Color(Ink.r, Ink.g, Ink.b, 0.94f));
            Text timer = CreateText(timerPanel.transform, "Timer", "00:00.00", 31, Cream, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220f, 50f), FontStyle.Bold);

            Button pause = CreateIconButton(canvas.transform, "PauseButton", LoadIcon(6), new Vector2(1f, 1f),
                new Vector2(-122f, -62f));
            Button hudSettings = CreateIconButton(canvas.transform, "HudSettingsButton", LoadIcon(3),
                new Vector2(1f, 1f), new Vector2(-48f, -62f));

            GameObject chargeRoot = CreatePanel(canvas.transform, "ChargeRoot", new Vector2(0.5f, 0f),
                new Vector2(0f, 58f), new Vector2(420f, 76f), new Color(Ink.r, Ink.g, Ink.b, 0.9f));
            CreateText(chargeRoot.transform, "ChargeLabel", "CHARGE", 21, Cream, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.77f), Vector2.zero, new Vector2(180f, 28f), FontStyle.Bold);
            Slider charge = CreateSlider(chargeRoot.transform, "ChargeSlider", new Vector2(0.5f, 0.30f),
                Vector2.zero, new Vector2(350f, 22f));

            GameObject tutorial = CreatePanel(canvas.transform, "TutorialHint", new Vector2(0f, 0f),
                new Vector2(300f, 55f), new Vector2(560f, 62f), new Color(Ink.r, Ink.g, Ink.b, 0.82f));
            CreateText(tutorial.transform, "HintText", "A / D   AIM     •     HOLD SPACE   CHARGE     •     RELEASE   JUMP",
                18, Cream, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(530f, 40f));

            GameObject pausePanel = CreatePausePanel(canvas.transform, out Button resume, out Button restart,
                out Button pauseSettings, out Button mainMenu);
            SettingsPanelView settingsPanel = CreateSettingsPanel(canvas.transform);
            GameObject cutscene = CreateCutscenePanel(canvas.transform, out Text dialogueName, out Text dialogueBody,
                out Button dialogueNext, out Button dialogueSkip);
            GameObject victory = CreateVictoryPanel(canvas.transform, out Text victoryTime, out Text victoryHeight,
                out Button victoryRestart, out Button victoryMainMenu);

            pausePanel.SetActive(false);
            settingsPanel.gameObject.SetActive(false);
            cutscene.SetActive(false);
            victory.SetActive(false);

            Set(ui, "currentHeightText", height); Set(ui, "bestHeightText", best); Set(ui, "timerText", timer);
            Set(ui, "chargeSlider", charge); Set(ui, "chargeRoot", chargeRoot); Set(ui, "tutorialHint", tutorial);
            Set(ui, "pauseButton", pause); Set(ui, "hudSettingsButton", hudSettings);
            Set(ui, "pausePanel", pausePanel); Set(ui, "resumeButton", resume); Set(ui, "restartButton", restart);
            Set(ui, "pauseSettingsButton", pauseSettings); Set(ui, "mainMenuButton", mainMenu);
            Set(ui, "settingsPanel", settingsPanel); Set(ui, "cutscenePanel", cutscene);
            Set(ui, "dialogueNameText", dialogueName); Set(ui, "dialogueBodyText", dialogueBody);
            Set(ui, "dialogueNextButton", dialogueNext); Set(ui, "dialogueSkipButton", dialogueSkip);
            Set(ui, "victoryPanel", victory); Set(ui, "victoryTimeText", victoryTime);
            Set(ui, "victoryHeightText", victoryHeight); Set(ui, "victoryRestartButton", victoryRestart);
            Set(ui, "victoryMainMenuButton", victoryMainMenu);
            return ui;
        }

        private static GameObject CreatePausePanel(Transform parent, out Button resume, out Button restart,
            out Button settings, out Button mainMenu)
        {
            GameObject shade = CreateStretchImage(parent, "PausePanel", null, new Color(0f, 0f, 0f, 0.62f));
            GameObject panel = CreatePanel(shade.transform, "PauseCard", new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(560f, 600f), new Color(Ink.r, Ink.g, Ink.b, 0.98f));
            CreateText(panel.transform, "Title", "JOURNEY PAUSED", 43, Gold, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.84f), Vector2.zero, new Vector2(480f, 70f), FontStyle.Bold);
            resume = CreateButton(panel.transform, "ResumeButton", "RESUME", LoadIcon(1), new Vector2(0.5f, 0.64f),
                Vector2.zero, new Vector2(390f, 68f), true);
            restart = CreateButton(panel.transform, "RestartButton", "RESTART", LoadIcon(5), new Vector2(0.5f, 0.49f),
                Vector2.zero, new Vector2(390f, 68f));
            settings = CreateButton(panel.transform, "SettingsButton", "SETTINGS", LoadIcon(3), new Vector2(0.5f, 0.34f),
                Vector2.zero, new Vector2(390f, 68f));
            mainMenu = CreateButton(panel.transform, "MainMenuButton", "MAIN MENU", LoadIcon2(25), new Vector2(0.5f, 0.19f),
                Vector2.zero, new Vector2(390f, 68f));
            return shade;
        }

        private static SettingsPanelView CreateSettingsPanel(Transform parent)
        {
            GameObject shade = CreateStretchImage(parent, "SettingsPanel", null, new Color(0f, 0f, 0f, 0.72f));
            GameObject panel = CreatePanel(shade.transform, "SettingsCard", new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(720f, 680f), new Color(Ink.r, Ink.g, Ink.b, 0.99f));
            CreateText(panel.transform, "Title", "SETTINGS", 46, Gold, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.88f), Vector2.zero, new Vector2(600f, 70f), FontStyle.Bold);
            CreateText(panel.transform, "VolumeLabel", "MASTER VOLUME", 23, Cream, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 0.70f), new Vector2(-170f, 0f), new Vector2(280f, 40f));
            Slider volume = CreateSlider(panel.transform, "MasterVolume", new Vector2(0.5f, 0.62f), Vector2.zero,
                new Vector2(520f, 28f));
            Toggle fullscreen = CreateToggle(panel.transform, "Fullscreen", "FULLSCREEN", new Vector2(0.5f, 0.48f));
            Toggle shake = CreateToggle(panel.transform, "CameraShake", "CAMERA SHAKE", new Vector2(0.5f, 0.37f));
            Toggle charge = CreateToggle(panel.transform, "ChargeIndicator", "SHOW CHARGE INDICATOR", new Vector2(0.5f, 0.26f));
            Button close = CreateButton(panel.transform, "CloseButton", "CLOSE", LoadIcon(2), new Vector2(0.5f, 0.10f),
                Vector2.zero, new Vector2(310f, 64f), true);
            SettingsPanelView view = shade.AddComponent<SettingsPanelView>();
            Set(view, "masterVolumeSlider", volume); Set(view, "fullscreenToggle", fullscreen);
            Set(view, "cameraShakeToggle", shake); Set(view, "chargeIndicatorToggle", charge); Set(view, "closeButton", close);
            return view;
        }

        private static GameObject CreateCutscenePanel(Transform parent, out Text nameText, out Text bodyText,
            out Button next, out Button skip)
        {
            GameObject root = CreateStretchImage(parent, "CutscenePanel", null, new Color(0.02f, 0.02f, 0.05f, 0.26f));
            skip = CreateButton(root.transform, "SkipButton", "ESC  SKIP", LoadIcon(2), new Vector2(1f, 1f),
                new Vector2(-105f, -55f), new Vector2(170f, 58f));
            GameObject dialogue = CreatePanel(root.transform, "DialogueBox", new Vector2(0.5f, 0f),
                new Vector2(0f, 145f), new Vector2(1180f, 220f), new Color(Ink.r, Ink.g, Ink.b, 0.97f));
            Image portrait = CreateImage(dialogue.transform, "Portrait", LoadIcon3(55), InkSoft,
                new Vector2(0f, 0.5f), new Vector2(105f, 0f), new Vector2(160f, 160f));
            portrait.preserveAspect = true;
            nameText = CreateText(dialogue.transform, "Speaker", "Princess Elira", 30, Gold, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 0.74f), new Vector2(35f, 0f), new Vector2(760f, 45f), FontStyle.Bold);
            bodyText = CreateText(dialogue.transform, "Dialogue", "You made it...", 25, Cream, TextAnchor.UpperLeft,
                new Vector2(0.5f, 0.39f), new Vector2(35f, 0f), new Vector2(760f, 92f));
            next = CreateButton(dialogue.transform, "NextButton", "NEXT", null, new Vector2(0.89f, 0.18f),
                Vector2.zero, new Vector2(170f, 52f), true);
            CreateText(root.transform, "StoryMotto", "AT LAST, YOU ARE HERE", 25, Gold, TextAnchor.MiddleCenter,
                new Vector2(0.86f, 0.73f), Vector2.zero, new Vector2(300f, 90f));
            return root;
        }

        private static GameObject CreateVictoryPanel(Transform parent, out Text timeText, out Text heightText,
            out Button restart, out Button mainMenu)
        {
            GameObject root = CreateStretchImage(parent, "VictoryPanel", null, new Color(0.025f, 0.025f, 0.06f, 0.82f));
            CreateText(root.transform, "Crown", "♛", 76, Gold, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.88f),
                Vector2.zero, new Vector2(140f, 90f));
            CreateText(root.transform, "Title", "CONGRATULATIONS!", 72, Gold, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.76f), Vector2.zero, new Vector2(1050f, 100f), FontStyle.Bold);
            CreateText(root.transform, "Subtitle", "YOU HAVE REACHED THE TOP", 29, Cream, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.68f), Vector2.zero, new Vector2(700f, 55f));

            GameObject stats = CreatePanel(root.transform, "Stats", new Vector2(0.25f, 0.40f), Vector2.zero,
                new Vector2(560f, 380f), new Color(Ink.r, Ink.g, Ink.b, 0.96f));
            timeText = CreateText(stats.transform, "Time", "Completion Time    00:00.00", 25, Cream, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 0.70f), Vector2.zero, new Vector2(480f, 48f));
            heightText = CreateText(stats.transform, "Height", "Highest Reached    000 m", 25, Cream, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 0.52f), Vector2.zero, new Vector2(480f, 48f));
            CreateText(stats.transform, "Quote", "“You did not just reach the top,\nyou proved what you are made of.”", 22,
                Gold, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.25f), Vector2.zero, new Vector2(480f, 100f), FontStyle.Italic);

            GameObject actions = CreatePanel(root.transform, "Actions", new Vector2(0.77f, 0.39f), Vector2.zero,
                new Vector2(470f, 320f), new Color(Ink.r, Ink.g, Ink.b, 0.72f));
            restart = CreateButton(actions.transform, "ContinueButton", "CLIMB AGAIN", LoadIcon(1),
                new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(370f, 72f), true);
            mainMenu = CreateButton(actions.transform, "MainMenuButton", "MAIN MENU", LoadIcon2(25),
                new Vector2(0.5f, 0.36f), Vector2.zero, new Vector2(370f, 72f));
            CreateText(root.transform, "Footer", "SOME PLACES CHANGE YOU JUST BY REACHING THEM", 19, Cream,
                TextAnchor.MiddleCenter, new Vector2(0.75f, 0.12f), Vector2.zero, new Vector2(600f, 50f));
            return root;
        }

        private static Canvas CreateCanvas(string name)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void CreateEventSystem()
        {
            GameObject go = new("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static GameObject CreateStretchImage(Transform parent, string name, Sprite sprite, Color color)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = sprite == null && color.a > 0.5f;
            return go;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 position,
            Vector2 size, Color color)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = go.GetComponent<Image>();
            image.color = color;
            Outline outline = go.GetComponent<Outline>();
            outline.effectColor = Gold;
            outline.effectDistance = new Vector2(2f, -2f);
            return go;
        }

        private static Image CreateImage(Transform parent, string name, Sprite sprite, Color color, Vector2 anchor,
            Vector2 position, Vector2 size)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            return image;
        }

        private static Text CreateText(Transform parent, string name, string value, int size, Color color,
            TextAnchor alignment, Vector2 anchor, Vector2 position, Vector2 dimensions, FontStyle style = FontStyle.Normal)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Shadow));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = dimensions;
            Text text = go.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            Shadow shadow = go.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            shadow.effectDistance = new Vector2(2f, -2f);
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Sprite icon, Vector2 anchor,
            Vector2 position, Vector2 size, bool primary = false)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(Outline));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = go.GetComponent<Image>();
            image.color = primary ? Red : InkSoft;
            Outline outline = go.GetComponent<Outline>();
            outline.effectColor = primary ? Gold : new Color(Gold.r, Gold.g, Gold.b, 0.7f);
            outline.effectDistance = new Vector2(2f, -2f);
            Button button = go.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = primary ? Red : InkSoft;
            colors.highlightedColor = primary ? RedBright : Slate;
            colors.pressedColor = Hex("4B1C19");
            colors.selectedColor = colors.highlightedColor;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            if (icon != null)
            {
                Image iconImage = CreateImage(go.transform, "Icon", icon, Color.white, new Vector2(0f, 0.5f),
                    new Vector2(42f, 0f), new Vector2(40f, 40f));
                iconImage.preserveAspect = true;
                CreateText(go.transform, "Label", label, 25, Cream, TextAnchor.MiddleCenter, new Vector2(0.57f, 0.5f),
                    Vector2.zero, new Vector2(size.x - 105f, size.y - 12f), FontStyle.Bold);
            }
            else
            {
                CreateText(go.transform, "Label", label, 25, Cream, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f),
                    Vector2.zero, new Vector2(size.x - 30f, size.y - 12f), FontStyle.Bold);
            }
            return button;
        }

        private static Button CreateIconButton(Transform parent, string name, Sprite icon, Vector2 anchor, Vector2 position)
        {
            Button button = CreateButton(parent, name, string.Empty, null, anchor, position, new Vector2(64f, 64f));
            Image iconImage = CreateImage(button.transform, "Icon", icon, Color.white, new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(44f, 44f));
            iconImage.preserveAspect = true;
            return button;
        }

        private static Slider CreateSlider(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
        {
            GameObject root = new(name, typeof(RectTransform), typeof(Slider));
            root.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)root.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image background = CreateImage(root.transform, "Background", null, Hex("090B10"), new Vector2(0.5f, 0.5f),
                Vector2.zero, size);
            Outline outline = background.gameObject.AddComponent<Outline>();
            outline.effectColor = Gold;
            GameObject fillArea = new("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(root.transform, false);
            RectTransform fillAreaRect = (RectTransform)fillArea.transform;
            fillAreaRect.anchorMin = new Vector2(0f, 0.2f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.8f);
            fillAreaRect.offsetMin = new Vector2(5f, 0f);
            fillAreaRect.offsetMax = new Vector2(-5f, 0f);
            Image fill = CreateStretchImage(fillArea.transform, "Fill", null, Gold).GetComponent<Image>();
            RectTransform fillRect = fill.rectTransform;
            GameObject handle = CreateImage(root.transform, "Handle", null, Cream, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(16f, size.y + 10f)).gameObject;
            Slider slider = root.GetComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = (RectTransform)handle.transform;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.8f;
            return slider;
        }

        private static Toggle CreateToggle(Transform parent, string name, string label, Vector2 anchor)
        {
            GameObject root = new(name, typeof(RectTransform), typeof(Toggle));
            root.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)root.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(520f, 55f);
            Image background = CreateImage(root.transform, "Background", null, InkSoft, new Vector2(0f, 0.5f),
                new Vector2(24f, 0f), new Vector2(38f, 38f));
            Outline outline = background.gameObject.AddComponent<Outline>();
            outline.effectColor = Gold;
            Image check = CreateImage(background.transform, "Checkmark", LoadIcon(7), Color.white,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30f, 30f));
            CreateText(root.transform, "Label", label, 23, Cream, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f),
                new Vector2(45f, 0f), new Vector2(420f, 46f));
            Toggle toggle = root.GetComponent<Toggle>();
            toggle.targetGraphic = background;
            toggle.graphic = check;
            return toggle;
        }

        private static Sprite LoadIcon(int index) => LoadSprite($"Assets/_Game/ThirdParty/MedievalFantasyUI/UI Icons/icon{index}.png");
        private static Sprite LoadIcon2(int index) => LoadSprite($"Assets/_Game/ThirdParty/MedievalFantasyUI/UI Icons 2/icon{index}.png");
        private static Sprite LoadIcon3(int index) => LoadSprite($"Assets/_Game/ThirdParty/MedievalFantasyUI/UI Icons 3/icon{index}.png");

        private static Sprite LoadSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogError($"[VerticalSliceUiBuilder] Missing sprite: {path}");
            }
            return sprite;
        }

        private static void Set(Object target, string propertyName, Object value)
        {
            SerializedObject serialized = new(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"Missing serialized property {target.GetType().Name}.{propertyName}");
                return;
            }
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString($"#{value}", out Color color);
            return color;
        }
    }
}

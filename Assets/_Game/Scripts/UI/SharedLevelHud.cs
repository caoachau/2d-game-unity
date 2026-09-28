using System.Collections;
using Summit.Game.Application;
using Summit.Game.Camera;
using Summit.Game.Environment;
using Summit.Game.Input;
using Summit.Game.Player;
using Summit.Game.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Summit.Game.UI
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class SharedLevelHud : MonoBehaviour
    {
        private const string HudScene = "HUD";
        private PlayerJumpController player;
        private PlayerInputReader input;
        private GameProgressTracker progress;
        private GameRunTimer timer;
        private GameUiController ui;
        private UnityPauseService pause;
        private ISettingsService settings;
        private GoalTrigger legacyGoal;
        private bool completed;
        private bool showStory;
        private bool loading;
        private bool loadFailed;
        private Scene importedScene;

        public bool IsCompleted => completed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallInOpenScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                Ensure(SceneManager.GetSceneAt(i));
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode) => Ensure(scene);

        private static bool IsGameplayScene(string name) => name == "Area_1_Forest" ||
            name == "Area_2_Cave" || name == "Area_3_OldCastle" || name == HudScene;

        public static SharedLevelHud Ensure(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded || !IsGameplayScene(scene.name)) return null;
            SharedLevelHud existing = FindInScene<SharedLevelHud>(scene);
            if (existing != null) return existing;
            GameObject host = new GameObject("SharedLevelHUD");
            SceneManager.MoveGameObjectToScene(host, scene);
            return host.AddComponent<SharedLevelHud>();
        }

        public static T FindInScene<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>();
                if (component != null) return component;
            }
            return null;
        }

        private IEnumerator Start()
        {
            player = FindInScene<PlayerJumpController>(gameObject.scene);
            input = FindInScene<PlayerInputReader>(gameObject.scene);
            if (player == null || input == null)
            {
                loadFailed = true;
                Debug.LogError("[SharedLevelHud] This level needs a Player and PlayerInputReader.", this);
                yield break;
            }

            ISaveService save = new PlayerPrefsSaveService();
            pause = new UnityPauseService();
            settings = new UnitySettingsService(save);
            settings.SettingsChanged += HandleSettingsChanged;
            progress = gameObject.AddComponent<GameProgressTracker>();
            timer = gameObject.AddComponent<GameRunTimer>();
            float goalHeight = player.transform.position.y + 1f;
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            foreach (Transform item in root.GetComponentsInChildren<Transform>())
                if (item.name.StartsWith("Goal_", System.StringComparison.OrdinalIgnoreCase))
                    goalHeight = Mathf.Max(goalHeight, item.position.y);
            progress.Initialize(player.transform, save, player.transform.position.y, goalHeight, gameObject.scene.name);
            timer.Initialize(player);
            input.PausePressed += HandlePause;
            pause.PauseChanged += HandlePauseChanged;
            player.Landed += HandleLanded;
            legacyGoal = FindInScene<GoalTrigger>(gameObject.scene);
            if (legacyGoal != null) legacyGoal.Reached += HandleLegacyGoal;

            ui = FindInScene<GameUiController>(gameObject.scene);
            if (ui == null)
            {
                if (!UnityEngine.Application.CanStreamedLevelBeLoaded(HudScene))
                {
                    loadFailed = true;
                    Debug.LogError("[SharedLevelHud] Add Assets/_Game/Scenes/HUD.unity to Build Settings.", this);
                    yield break;
                }
                loading = true;
                SceneManager.sceneLoaded += ImportHud;
                AsyncOperation operation = SceneManager.LoadSceneAsync(HudScene, LoadSceneMode.Additive);
                if (operation != null) yield return operation;
                SceneManager.sceneLoaded -= ImportHud;
                loading = false;
            }
            if (ui == null)
            {
                loadFailed = true;
                if (importedScene.IsValid() && importedScene.isLoaded)
                    SceneManager.UnloadSceneAsync(importedScene);
                Debug.LogError("[SharedLevelHud] HUD scene needs GameCanvas with GameUiController.", this);
                yield break;
            }
            ui.Initialize(player, progress, timer, pause, new UnitySceneService(gameObject.scene.name), settings);
            ui.gameObject.SetActive(true);
            if (completed) PresentCompletion();
            if (importedScene.IsValid() && importedScene.isLoaded)
                SceneManager.UnloadSceneAsync(importedScene);
        }

        private void ImportHud(Scene source, LoadSceneMode mode)
        {
            if (!loading || source.name != HudScene || !gameObject.scene.isLoaded) return;
            // sceneLoaded runs before Start: prevent the design scene's sample player,
            // camera and bootstrapper from participating in the level.
            foreach (GameObject root in source.GetRootGameObjects())
            {
                if (root.GetComponentInChildren<GameUiController>(true) != null)
                    ui = root.GetComponentInChildren<GameUiController>(true);
                else root.SetActive(false);
            }
            if (ui != null)
            {
                ui.BindAuthoredLayout();
                GameObject canvasRoot = ui.transform.root.gameObject;
                canvasRoot.SetActive(false);
                SceneManager.MoveGameObjectToScene(canvasRoot, gameObject.scene);
            }
            if (FindInScene<EventSystem>(gameObject.scene) == null)
            {
                foreach (GameObject root in source.GetRootGameObjects())
                {
                    if (root.GetComponentInChildren<EventSystem>(true) == null) continue;
                    SceneManager.MoveGameObjectToScene(root, gameObject.scene);
                    root.SetActive(true);
                    break;
                }
            }
            importedScene = source;
        }

        private void HandlePause()
        {
            if (completed) return;
            if (ui != null && ui.IsInitialized) ui.TogglePause();
            else pause.Toggle();
        }

        private void HandlePauseChanged(bool paused)
        {
            input?.SetGameplayEnabled(!paused && !completed);
            if (paused && player != null) player.CancelCharge();
        }

        private void HandleSettingsChanged()
        {
            if (settings.CameraShake) return;
            VerticalCameraFollow camera = FindInScene<VerticalCameraFollow>(gameObject.scene);
            if (camera != null) camera.CancelShake();
        }

        private void HandleLanded(bool hardLanding, float impact)
        {
            if (!hardLanding || !settings.CameraShake) return;
            VerticalCameraFollow camera = FindInScene<VerticalCameraFollow>(gameObject.scene);
            if (camera != null) camera.Shake(Mathf.Clamp(impact * .008f, .04f, .15f));
        }

        private void HandleLegacyGoal() => CompleteLevel(gameObject.scene.name == "Area_3_OldCastle");

        public bool CompleteLevel(bool playStory = false)
        {
            if (loadFailed || player == null || timer == null || pause == null) return false;
            if (completed) return true;
            completed = true;
            showStory = playStory;
            player.enabled = false;
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            if (body != null) body.linearVelocity = Vector2.zero;
            input.SetGameplayEnabled(false);
            progress.CaptureCurrentHeight();
            timer.Stop();
            pause.Pause();
            if (ui != null && ui.IsInitialized) PresentCompletion();
            return true;
        }

        private void PresentCompletion()
        {
            if (showStory) ui.ShowCutscene();
            else ui.ShowVictory();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= ImportHud;
            if (input != null)
            {
                input.PausePressed -= HandlePause;
                input.SetGameplayEnabled(true);
            }
            if (player != null) player.Landed -= HandleLanded;
            if (settings != null) settings.SettingsChanged -= HandleSettingsChanged;
            if (legacyGoal != null) legacyGoal.Reached -= HandleLegacyGoal;
            if (pause != null)
            {
                pause.PauseChanged -= HandlePauseChanged;
                if (pause.IsPaused) pause.Resume();
            }
        }
    }
}

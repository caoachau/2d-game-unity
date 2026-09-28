using Summit.Game.Camera;
using Summit.Game.Configuration;
using Summit.Game.Input;
using Summit.Game.Environment;
using Summit.Game.Player;
using Summit.Game.Services;
using Summit.Game.UI;
using UnityEngine;

namespace Summit.Game.Application
{
    public sealed class GameSceneBootstrapper : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerJumpController jumpController;
        [SerializeField] private PlayerMovementConfig movementConfig;
        [SerializeField] private VerticalCameraFollow cameraFollow;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private GameProgressTracker progressTracker;
        [SerializeField] private GameRunTimer runTimer;
        [SerializeField] private GoalTrigger goalTrigger;
        [SerializeField] private GameUiController gameUi;
        [SerializeField] private Transform levelStart;

        private IPauseService pauseService;
        private ISceneService sceneService;
        private ISettingsService settingsService;

        private void Start()
        {
            bool sharedHud = SharedLevelHud.Ensure(gameObject.scene) != null;
            // Scene-authored maps can add the Player prefab without manually
            // wiring every reference on this scene bootstrapper.
            if (inputReader == null)
            {
                inputReader = SharedLevelHud.FindInScene<PlayerInputReader>(gameObject.scene);
            }

            if (jumpController == null)
            {
                jumpController = SharedLevelHud.FindInScene<PlayerJumpController>(gameObject.scene);
            }

            if (playerTransform == null && jumpController != null)
            {
                playerTransform = jumpController.transform;
            }

            if (inputReader == null || jumpController == null)
            {
                Debug.LogError("[GameSceneBootstrapper] Required scene dependency is missing.", this);
                enabled = false;
                return;
            }

            if (movementConfig == null)
            {
                movementConfig = ScriptableObject.CreateInstance<PlayerMovementConfig>();
                Debug.LogWarning("[GameSceneBootstrapper] PlayerMovementConfig reference was missing; using safe runtime defaults.", this);
            }

            ISaveService saveService = new PlayerPrefsSaveService();
            pauseService = new UnityPauseService();
            sceneService = new UnitySceneService();
            settingsService = new UnitySettingsService(saveService);
            jumpController.Initialize(inputReader, movementConfig);

            if (jumpController.GetComponent<PlayerVisualController>() == null)
            {
                jumpController.gameObject.AddComponent<PlayerVisualController>();
            }

            if (cameraFollow != null && playerTransform != null)
            {
                cameraFollow.Initialize(playerTransform);
            }

            if (!sharedHud && progressTracker != null && runTimer != null && goalTrigger != null && gameUi != null)
            {
                float startY = levelStart != null ? levelStart.position.y : playerTransform.position.y;
                progressTracker.Initialize(playerTransform, saveService, startY, goalTrigger.transform.position.y);
                runTimer.Initialize(jumpController);
                gameUi.Initialize(jumpController, progressTracker, runTimer, pauseService, sceneService, settingsService);
                goalTrigger.Reached += HandleGoalReached;
            }

            if (!sharedHud) inputReader.PausePressed += HandlePausePressed;
        }

        private void OnDestroy()
        {
            if (inputReader != null)
            {
                inputReader.PausePressed -= HandlePausePressed;
            }


            if (goalTrigger != null)
            {
                goalTrigger.Reached -= HandleGoalReached;
            }

            if (pauseService != null && pauseService.IsPaused)
            {
                pauseService.Resume();
            }
        }

        private void HandlePausePressed()
        {
            pauseService.Toggle();
        }

        private void HandleGoalReached()
        {
            inputReader.enabled = false;
            runTimer.Stop();
            gameUi.ShowCutscene();
        }
    }
}

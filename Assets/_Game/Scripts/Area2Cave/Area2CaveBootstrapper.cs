using Summit.Game.Camera;
using Summit.Game.Configuration;
using Summit.Game.Input;
using Summit.Game.Player;
using UnityEngine;

namespace Summit.Game.Area2Cave
{
    public sealed class Area2CaveBootstrapper : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerJumpController jumpController;
        [SerializeField] private PlayerMovementConfig movementConfig;
        [SerializeField] private VerticalCameraFollow cameraFollow;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private CaveRespawnController respawnController;
        [SerializeField] private CaveHudController hudController;
        [SerializeField] private CaveExitGoal exitGoal;

        public void Configure(
            PlayerInputReader input,
            PlayerJumpController jump,
            PlayerMovementConfig config,
            VerticalCameraFollow camera,
            Transform player,
            CaveRespawnController respawn,
            CaveHudController hud,
            CaveExitGoal goal)
        {
            inputReader = input;
            jumpController = jump;
            movementConfig = config;
            cameraFollow = camera;
            playerTransform = player;
            respawnController = respawn;
            hudController = hud;
            exitGoal = goal;
        }

        private void Start()
        {
            if (inputReader == null || jumpController == null || movementConfig == null || playerTransform == null)
            {
                Debug.LogError("[Area2CaveBootstrapper] Missing required dependency.", this);
                enabled = false;
                return;
            }

            jumpController.Initialize(inputReader, movementConfig);
            if (jumpController.GetComponent<PlayerVisualController>() == null)
            {
                jumpController.gameObject.AddComponent<PlayerVisualController>();
            }
            if (cameraFollow != null)
            {
                cameraFollow.Initialize(playerTransform);
            }

            hudController?.Initialize();

            if (exitGoal != null && hudController != null)
            {
                exitGoal.Reached += hudController.ShowComplete;
            }
        }

        private void OnDestroy()
        {
            if (exitGoal != null && hudController != null)
            {
                exitGoal.Reached -= hudController.ShowComplete;
            }
        }
    }
}

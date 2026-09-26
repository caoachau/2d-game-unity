using Summit.Game.Camera;
using Summit.Game.Configuration;
using Summit.Game.Input;
using Summit.Game.Player;
using UnityEngine;

namespace Summit.Game.Area3OldCastle
{
    public sealed class Area3OldCastleBootstrapper : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerJumpController jumpController;
        [SerializeField] private PlayerMovementConfig movementConfig;
        [SerializeField] private VerticalCameraFollow cameraFollow;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private OldCastleRespawnController respawnController;
        [SerializeField] private OldCastleHudController hudController;
        [SerializeField] private OldCastleDialogueTrigger finalDialogueTrigger;
        [SerializeField] private OldCastlePrincessGoal princessGoal;

        public void Configure(PlayerInputReader input, PlayerJumpController jump, PlayerMovementConfig config,
            VerticalCameraFollow camera, Transform player, OldCastleRespawnController respawn,
            OldCastleHudController hud, OldCastleDialogueTrigger dialogueTrigger, OldCastlePrincessGoal goal)
        {
            inputReader = input;
            jumpController = jump;
            movementConfig = config;
            cameraFollow = camera;
            playerTransform = player;
            respawnController = respawn;
            hudController = hud;
            finalDialogueTrigger = dialogueTrigger;
            princessGoal = goal;
        }

        private void Start()
        {
            if (inputReader == null || jumpController == null || movementConfig == null || playerTransform == null)
            {
                Debug.LogError("[Area3OldCastleBootstrapper] Missing required dependency.", this);
                enabled = false;
                return;
            }

            jumpController.Initialize(inputReader, movementConfig);
            PlayerVisualController playerVisual = jumpController.GetComponent<PlayerVisualController>();
            if (playerVisual == null)
            {
                playerVisual = jumpController.gameObject.AddComponent<PlayerVisualController>();
            }
            // Area 3 platforms already have their collider close to the artwork;
            // unlike Area 2 they need no extra visual drop.
            playerVisual.SetVisualFloorDrop(0f);
            cameraFollow?.Initialize(playerTransform);
            hudController?.Initialize();

            if (finalDialogueTrigger != null && hudController != null)
                finalDialogueTrigger.Triggered += hudController.ShowPrincessDialogue;
            if (princessGoal != null && hudController != null)
                princessGoal.Reached += hudController.ShowComplete;
        }

        private void OnDestroy()
        {
            if (finalDialogueTrigger != null && hudController != null)
                finalDialogueTrigger.Triggered -= hudController.ShowPrincessDialogue;
            if (princessGoal != null && hudController != null)
                princessGoal.Reached -= hudController.ShowComplete;
        }
    }
}

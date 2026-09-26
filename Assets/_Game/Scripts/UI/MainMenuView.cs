using Summit.Game.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Summit.Game.UI
{
    public sealed class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button cornerSettingsButton;
        [SerializeField] private Button cornerQuitButton;
        [SerializeField] private SettingsPanelView settingsPanel;

        private ISceneService sceneService;

        public void Initialize(ISceneService scenes, ISettingsService settings)
        {
            sceneService = scenes;
            settingsPanel.Initialize(settings);
            playButton.onClick.AddListener(sceneService.StartGame);
            settingsButton.onClick.AddListener(settingsPanel.Show);
            quitButton.onClick.AddListener(sceneService.Quit);
            cornerSettingsButton.onClick.AddListener(settingsPanel.Show);
            cornerQuitButton.onClick.AddListener(sceneService.Quit);
        }

        private void OnDestroy()
        {
            if (sceneService == null)
            {
                return;
            }

            playButton.onClick.RemoveListener(sceneService.StartGame);
            settingsButton.onClick.RemoveListener(settingsPanel.Show);
            quitButton.onClick.RemoveListener(sceneService.Quit);
            cornerSettingsButton.onClick.RemoveListener(settingsPanel.Show);
            cornerQuitButton.onClick.RemoveListener(sceneService.Quit);
        }
    }
}

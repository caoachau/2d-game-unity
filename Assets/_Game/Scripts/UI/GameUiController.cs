using Summit.Game.Application;
using Summit.Game.Domain;
using Summit.Game.Player;
using Summit.Game.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Summit.Game.UI
{
    public sealed class GameUiController : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private Text currentHeightText;
        [SerializeField] private Text bestHeightText;
        [SerializeField] private Text timerText;
        [SerializeField] private Slider chargeSlider;
        [SerializeField] private GameObject chargeRoot;
        [SerializeField] private GameObject tutorialHint;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button hudSettingsButton;

        [Header("Pause")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button pauseSettingsButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Settings")]
        [SerializeField] private SettingsPanelView settingsPanel;

        [Header("Story")]
        [SerializeField] private GameObject cutscenePanel;
        [SerializeField] private Text dialogueNameText;
        [SerializeField] private Text dialogueBodyText;
        [SerializeField] private Button dialogueNextButton;
        [SerializeField] private Button dialogueSkipButton;

        [Header("Victory")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private Text victoryTimeText;
        [SerializeField] private Text victoryHeightText;
        [SerializeField] private Button victoryRestartButton;
        [SerializeField] private Button victoryMainMenuButton;

        private PlayerJumpController jumpController;
        private GameProgressTracker progressTracker;
        private GameRunTimer runTimer;
        private IPauseService pauseService;
        private ISceneService sceneService;
        private ISettingsService settingsService;
        private int successfulJumps;
        private int dialogueStep;
        private bool storyMode;

        public void Initialize(PlayerJumpController jump, GameProgressTracker progress, GameRunTimer timer,
            IPauseService pause, ISceneService scenes, ISettingsService settings)
        {
            jumpController = jump;
            progressTracker = progress;
            runTimer = timer;
            pauseService = pause;
            sceneService = scenes;
            settingsService = settings;

            jumpController.ChargeChanged += HandleChargeChanged;
            jumpController.Jumped += HandleJumped;
            progressTracker.ProgressChanged += HandleProgressChanged;
            runTimer.TimeChanged += HandleTimeChanged;
            pauseService.PauseChanged += HandlePauseChanged;
            settingsService.SettingsChanged += HandleSettingsChanged;

            pauseButton.onClick.AddListener(pauseService.Toggle);
            hudSettingsButton.onClick.AddListener(OpenSettingsFromHud);
            resumeButton.onClick.AddListener(pauseService.Resume);
            restartButton.onClick.AddListener(sceneService.RestartGame);
            pauseSettingsButton.onClick.AddListener(OpenSettingsFromPause);
            mainMenuButton.onClick.AddListener(sceneService.LoadMainMenu);
            dialogueNextButton.onClick.AddListener(AdvanceDialogue);
            dialogueSkipButton.onClick.AddListener(ShowVictory);
            victoryRestartButton.onClick.AddListener(sceneService.RestartGame);
            victoryMainMenuButton.onClick.AddListener(sceneService.LoadMainMenu);
            settingsPanel.Initialize(settingsService, HandleSettingsClosed);

            pausePanel.SetActive(false);
            cutscenePanel.SetActive(false);
            victoryPanel.SetActive(false);
            settingsPanel.gameObject.SetActive(false);
            chargeSlider.value = 0f;
            HandleProgressChanged(progress.CurrentHeight, progress.HighestHeight, progress.BestHeight);
            HandleTimeChanged(0f);
            HandleSettingsChanged();
        }

        private void OnDestroy()
        {
            if (jumpController != null)
            {
                jumpController.ChargeChanged -= HandleChargeChanged;
                jumpController.Jumped -= HandleJumped;
            }

            if (progressTracker != null) progressTracker.ProgressChanged -= HandleProgressChanged;
            if (runTimer != null) runTimer.TimeChanged -= HandleTimeChanged;
            if (pauseService != null) pauseService.PauseChanged -= HandlePauseChanged;
            if (settingsService != null) settingsService.SettingsChanged -= HandleSettingsChanged;
        }

        public void ShowCutscene()
        {
            storyMode = true;
            dialogueStep = 0;
            dialogueNameText.text = "Princess Elira";
            dialogueBodyText.text = "You made it...\nI knew you would. Thank you for coming all this way.";
            pausePanel.SetActive(false);
            settingsPanel.gameObject.SetActive(false);
            victoryPanel.SetActive(false);
            cutscenePanel.SetActive(true);
            pauseService.Pause();
        }

        private void HandleChargeChanged(float charge)
        {
            chargeSlider.value = charge;
        }

        private void HandleJumped(JumpDirection direction, float charge)
        {
            successfulJumps++;
            if (successfulJumps >= 3)
            {
                tutorialHint.SetActive(false);
            }
        }

        private void HandleProgressChanged(float current, float highest, float best)
        {
            currentHeightText.text = $"HEIGHT   {Mathf.RoundToInt(current * 10f):000}m";
            bestHeightText.text = $"BEST       {Mathf.RoundToInt(best * 10f):000}m";
        }

        private void HandleTimeChanged(float seconds)
        {
            int totalHundredths = Mathf.FloorToInt(seconds * 100f);
            int minutes = totalHundredths / 6000;
            int remainingSeconds = totalHundredths / 100 % 60;
            int hundredths = totalHundredths % 100;
            timerText.text = $"{minutes:00}:{remainingSeconds:00}.{hundredths:00}";
        }

        private void HandlePauseChanged(bool paused)
        {
            if (!storyMode)
            {
                pausePanel.SetActive(paused && !settingsPanel.gameObject.activeSelf);
            }
        }

        private void HandleSettingsChanged()
        {
            chargeRoot.SetActive(settingsService.ShowChargeIndicator);
        }

        private void OpenSettingsFromHud()
        {
            if (!pauseService.IsPaused)
            {
                pauseService.Pause();
            }

            pausePanel.SetActive(false);
            settingsPanel.Show();
        }

        private void OpenSettingsFromPause()
        {
            pausePanel.SetActive(false);
            settingsPanel.Show();
        }

        private void HandleSettingsClosed()
        {
            if (pauseService.IsPaused && !storyMode)
            {
                pausePanel.SetActive(true);
            }
        }

        private void AdvanceDialogue()
        {
            if (dialogueStep == 0)
            {
                dialogueStep = 1;
                dialogueBodyText.text = "The summit remembers every fall.\nToday, it will remember that you rose again.";
                return;
            }

            ShowVictory();
        }

        private void ShowVictory()
        {
            cutscenePanel.SetActive(false);
            victoryTimeText.text = $"Completion Time    {timerText.text}";
            victoryHeightText.text = $"Highest Reached    {Mathf.RoundToInt(progressTracker.HighestHeight * 10f):000} m";
            victoryPanel.SetActive(true);
        }
    }
}

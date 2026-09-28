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
        private bool initialized;
        private bool layoutBound;
        private bool authoredHeightLabels;
        private Image moveHintImage;
        private Image jumpHintImage;
        private Color moveHintColor;
        private Color jumpHintColor;
        private Input.PlayerInputReader hintInput;

        public bool IsInitialized => initialized;
        public bool IsFinished => storyMode;

        public void BindAuthoredLayout()
        {
            if (layoutBound) return;
            layoutBound = true;
            Canvas.ForceUpdateCanvases();
            RectTransform height = AuthoredHudLayout.FindPanel(this, "01_height_best_panel");
            if (height != null)
            {
                currentHeightText = AuthoredHudLayout.AddText(height, "CurrentHeightValue",
                    new Vector2(.68f, .53f), new Vector2(.93f, .88f));
                bestHeightText = AuthoredHudLayout.AddText(height, "BestHeightValue",
                    new Vector2(.68f, .13f), new Vector2(.93f, .48f));
                authoredHeightLabels = true;
            }
            RectTransform time = AuthoredHudLayout.FindPanel(this, "02_timer_panel");
            if (time != null)
                timerText = AuthoredHudLayout.AddText(time, "TimerValue",
                    new Vector2(.28f, .16f), new Vector2(.91f, .84f), TextAnchor.MiddleCenter);
            RectTransform charge = AuthoredHudLayout.FindPanel(this, "05_charge", "ui_015");
            if (charge != null)
            {
                chargeRoot = charge.gameObject;
                chargeSlider = AuthoredHudLayout.AddChargeSlider(charge);
            }
            RectTransform move = AuthoredHudLayout.FindPanel(this, "03_move_panel");
            RectTransform jump = AuthoredHudLayout.FindPanel(this, "04_jump_panel");
            if (move != null)
            {
                moveHintImage = move.GetComponent<Image>();
                moveHintColor = moveHintImage.color;
            }
            if (jump != null)
            {
                jumpHintImage = jump.GetComponent<Image>();
                jumpHintColor = jumpHintImage.color;
            }
        }

        public void Initialize(PlayerJumpController jump, GameProgressTracker progress, GameRunTimer timer,
            IPauseService pause, ISceneService scenes, ISettingsService settings)
        {
            if (initialized) return;
            BindAuthoredLayout();
            initialized = true;
            jumpController = jump;
            progressTracker = progress;
            runTimer = timer;
            pauseService = pause;
            sceneService = scenes;
            settingsService = settings;
            hintInput = SharedLevelHud.FindInScene<Input.PlayerInputReader>(gameObject.scene);

            jumpController.ChargeChanged += HandleChargeChanged;
            jumpController.Jumped += HandleJumped;
            progressTracker.ProgressChanged += HandleProgressChanged;
            runTimer.TimeChanged += HandleTimeChanged;
            pauseService.PauseChanged += HandlePauseChanged;
            settingsService.SettingsChanged += HandleSettingsChanged;

            pauseButton.onClick.AddListener(TogglePause);
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
            HandleChargeChanged(jump.NormalizedCharge);
            HandleProgressChanged(progress.CurrentHeight, progress.HighestHeight, progress.BestHeight);
            HandleTimeChanged(timer.ElapsedSeconds);
            HandleSettingsChanged();
            HandlePauseChanged(pause.IsPaused);
        }

        private void Update()
        {
            if (!initialized) return;
            Input.PlayerInputReader input = hintInput;
            if (moveHintImage != null)
                moveHintImage.color = input != null && !pauseService.IsPaused &&
                    (Mathf.Abs(input.Horizontal) > .1f || Mathf.Abs(input.Vertical) > .1f)
                    ? moveHintColor * new Color(1f, 1f, .65f, 1f) : moveHintColor;
            if (jumpHintImage != null)
                jumpHintImage.color = input != null && !pauseService.IsPaused && input.JumpHeld
                    ? jumpHintColor * new Color(1f, 1f, .65f, 1f) : jumpHintColor;
        }

        public void TogglePause()
        {
            if (!initialized || storyMode) return;
            if (settingsPanel.gameObject.activeSelf)
            {
                settingsPanel.Hide();
                pausePanel.SetActive(false);
                pauseService.Resume();
                return;
            }
            pauseService.Toggle();
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
            if (!initialized) return;
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
            if (chargeSlider != null) chargeSlider.value = charge;
        }

        private void HandleJumped(JumpDirection direction, float charge)
        {
            successfulJumps++;
            if (successfulJumps >= 3 && tutorialHint != null)
            {
                tutorialHint.SetActive(false);
            }
        }

        private void HandleProgressChanged(float current, float highest, float best)
        {
            if (currentHeightText != null) currentHeightText.text =
                (authoredHeightLabels ? "" : "HEIGHT   ") + $"{Mathf.RoundToInt(current * 10f):000}m";
            if (bestHeightText != null) bestHeightText.text =
                (authoredHeightLabels ? "" : "BEST       ") + $"{Mathf.RoundToInt(best * 10f):000}m";
        }

        private void HandleTimeChanged(float seconds)
        {
            int totalHundredths = Mathf.FloorToInt(seconds * 100f);
            int minutes = totalHundredths / 6000;
            int remainingSeconds = totalHundredths / 100 % 60;
            int hundredths = totalHundredths % 100;
            if (timerText != null) timerText.text = $"{minutes:00}:{remainingSeconds:00}.{hundredths:00}";
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
            if (chargeRoot != null) chargeRoot.SetActive(settingsService.ShowChargeIndicator);
        }

        private void OpenSettingsFromHud()
        {
            if (storyMode) return;
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

        public void ShowVictory()
        {
            if (!initialized) return;
            storyMode = true;
            runTimer.Stop();
            pauseService.Pause();
            pausePanel.SetActive(false);
            settingsPanel.gameObject.SetActive(false);
            cutscenePanel.SetActive(false);
            HandleTimeChanged(runTimer.ElapsedSeconds);
            victoryTimeText.text = $"Completion Time    {(timerText != null ? timerText.text : runTimer.ElapsedSeconds.ToString("0.00") + " s")}";
            victoryHeightText.text = $"Highest Reached    {Mathf.RoundToInt(progressTracker.HighestHeight * 10f):000} m";
            victoryPanel.SetActive(true);
        }
    }
}

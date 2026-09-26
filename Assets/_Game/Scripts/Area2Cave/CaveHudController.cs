using Summit.Game.Input;
using Summit.Game.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Summit.Game.Area2Cave
{
    public sealed class CaveHudController : MonoBehaviour
    {
        private const string BestHeightKey = "Area2Cave.BestHeight";

        [Header("Runtime")]
        [SerializeField] private Transform player;
        [SerializeField] private PlayerJumpController jumpController;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private float startY;

        [Header("HUD")]
        [SerializeField] private Text heightText;
        [SerializeField] private Text bestText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text crystalText;
        [SerializeField] private Slider chargeSlider;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button settingsButton;

        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject completePanel;
        [SerializeField] private Text completeStatsText;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button pauseMainMenuButton;
        [SerializeField] private Button closeSettingsButton;
        [SerializeField] private Button completeRestartButton;
        [SerializeField] private Button completeMainMenuButton;

        private float elapsedSeconds;
        private float bestHeight;
        private bool timerRunning;
        private bool completed;
        private bool initialized;

        public void Configure(
            Transform playerTransform,
            PlayerJumpController jump,
            PlayerInputReader input,
            float levelStartY,
            Text currentHeight,
            Text bestHeightLabel,
            Text timeLabel,
            Text crystalsLabel,
            Slider charge,
            Button pause,
            Button settings,
            GameObject pauseRoot,
            GameObject settingsRoot,
            GameObject completeRoot,
            Text completeStats,
            Button resume,
            Button restart,
            Button pauseMenu,
            Button closeSettings,
            Button completeRestart,
            Button completeMenu)
        {
            player = playerTransform;
            jumpController = jump;
            inputReader = input;
            startY = levelStartY;
            heightText = currentHeight;
            bestText = bestHeightLabel;
            timerText = timeLabel;
            crystalText = crystalsLabel;
            chargeSlider = charge;
            pauseButton = pause;
            settingsButton = settings;
            pausePanel = pauseRoot;
            settingsPanel = settingsRoot;
            completePanel = completeRoot;
            completeStatsText = completeStats;
            resumeButton = resume;
            restartButton = restart;
            pauseMainMenuButton = pauseMenu;
            closeSettingsButton = closeSettings;
            completeRestartButton = completeRestart;
            completeMainMenuButton = completeMenu;
        }

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            bestHeight = PlayerPrefs.GetFloat(BestHeightKey, 0f);

            if (jumpController != null)
            {
                jumpController.ChargeChanged += HandleChargeChanged;
                jumpController.Jumped += HandleJumped;
            }

            if (inputReader != null)
            {
                inputReader.PausePressed += TogglePause;
            }

            if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (restartButton != null) restartButton.onClick.AddListener(Restart);
            if (pauseMainMenuButton != null) pauseMainMenuButton.onClick.AddListener(MainMenu);
            if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettings);
            if (completeRestartButton != null) completeRestartButton.onClick.AddListener(Restart);
            if (completeMainMenuButton != null) completeMainMenuButton.onClick.AddListener(MainMenu);

            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (completePanel != null) completePanel.SetActive(false);
            if (chargeSlider != null) chargeSlider.value = 0f;
            RefreshLabels();
        }

        private void Update()
        {
            if (!initialized || completed)
            {
                return;
            }

            if (timerRunning && Time.timeScale > 0f)
            {
                elapsedSeconds += Time.unscaledDeltaTime;
            }

            float currentHeight = player != null ? Mathf.Max(0f, player.position.y - startY) : 0f;
            if (currentHeight > bestHeight)
            {
                bestHeight = currentHeight;
                PlayerPrefs.SetFloat(BestHeightKey, bestHeight);
            }

            RefreshLabels();
        }

        private void OnDestroy()
        {
            if (jumpController != null)
            {
                jumpController.ChargeChanged -= HandleChargeChanged;
                jumpController.Jumped -= HandleJumped;
            }

            if (inputReader != null)
            {
                inputReader.PausePressed -= TogglePause;
            }

            if (Time.timeScale == 0f)
            {
                Time.timeScale = 1f;
            }
        }

        public void ShowComplete()
        {
            if (completed)
            {
                return;
            }

            completed = true;
            timerRunning = false;
            float currentHeight = player != null ? Mathf.Max(0f, player.position.y - startY) : 0f;
            PlayerPrefs.SetFloat(BestHeightKey, Mathf.Max(bestHeight, currentHeight));
            PlayerPrefs.Save();

            if (completeStatsText != null)
            {
                completeStatsText.text = $"TIME   {FormatTime(elapsedSeconds)}\nHEIGHT {Mathf.RoundToInt(currentHeight * 10f):000}m\nCRYSTALS {PlayerPrefs.GetInt("Area2Cave.Crystals", 0):00}";
            }

            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (completePanel != null) completePanel.SetActive(true);
            Time.timeScale = 0f;
        }

        private void HandleChargeChanged(float normalizedCharge)
        {
            if (chargeSlider != null)
            {
                chargeSlider.value = normalizedCharge;
            }
        }

        private void HandleJumped(Summit.Game.Domain.JumpDirection direction, float charge)
        {
            timerRunning = true;
        }

        private void RefreshLabels()
        {
            float currentHeight = player != null ? Mathf.Max(0f, player.position.y - startY) : 0f;
            if (heightText != null) heightText.text = $"HEIGHT  {Mathf.RoundToInt(currentHeight * 10f):000}m";
            if (bestText != null) bestText.text = $"BEST    {Mathf.RoundToInt(bestHeight * 10f):000}m";
            if (timerText != null) timerText.text = FormatTime(elapsedSeconds);
            if (crystalText != null) crystalText.text = $"CRYSTAL  {PlayerPrefs.GetInt("Area2Cave.Crystals", 0):00}";
        }

        private static string FormatTime(float seconds)
        {
            int hundredths = Mathf.FloorToInt(seconds * 100f);
            int minutes = hundredths / 6000;
            int secs = hundredths / 100 % 60;
            int hs = hundredths % 100;
            return $"{minutes:00}:{secs:00}.{hs:00}";
        }

        private void TogglePause()
        {
            if (completed)
            {
                return;
            }

            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
                return;
            }

            if (Time.timeScale > 0f)
            {
                Time.timeScale = 0f;
                if (pausePanel != null) pausePanel.SetActive(true);
            }
            else
            {
                Resume();
            }
        }

        private void Resume()
        {
            if (completed)
            {
                return;
            }

            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void OpenSettings()
        {
            if (completed)
            {
                return;
            }

            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        private void CloseSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        private static void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private static void MainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}

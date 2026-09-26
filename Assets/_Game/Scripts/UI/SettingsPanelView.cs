using System;
using Summit.Game.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Summit.Game.UI
{
    public sealed class SettingsPanelView : MonoBehaviour
    {
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Toggle cameraShakeToggle;
        [SerializeField] private Toggle chargeIndicatorToggle;
        [SerializeField] private Button closeButton;

        private ISettingsService settingsService;
        private Action closeAction;
        private bool updatingControls;

        public void Initialize(ISettingsService service, Action onClosed = null)
        {
            settingsService = service;
            closeAction = onClosed;

            masterVolumeSlider.onValueChanged.AddListener(HandleMasterVolume);
            fullscreenToggle.onValueChanged.AddListener(HandleFullscreen);
            cameraShakeToggle.onValueChanged.AddListener(HandleCameraShake);
            chargeIndicatorToggle.onValueChanged.AddListener(HandleChargeIndicator);
            closeButton.onClick.AddListener(Hide);
            settingsService.SettingsChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (settingsService != null)
            {
                settingsService.SettingsChanged -= Refresh;
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            closeAction?.Invoke();
        }

        private void Refresh()
        {
            if (settingsService == null)
            {
                return;
            }

            updatingControls = true;
            masterVolumeSlider.SetValueWithoutNotify(settingsService.MasterVolume);
            fullscreenToggle.SetIsOnWithoutNotify(settingsService.Fullscreen);
            cameraShakeToggle.SetIsOnWithoutNotify(settingsService.CameraShake);
            chargeIndicatorToggle.SetIsOnWithoutNotify(settingsService.ShowChargeIndicator);
            updatingControls = false;
        }

        private void HandleMasterVolume(float value)
        {
            if (!updatingControls) settingsService.SetMasterVolume(value);
        }

        private void HandleFullscreen(bool value)
        {
            if (!updatingControls) settingsService.SetFullscreen(value);
        }

        private void HandleCameraShake(bool value)
        {
            if (!updatingControls) settingsService.SetCameraShake(value);
        }

        private void HandleChargeIndicator(bool value)
        {
            if (!updatingControls) settingsService.SetShowChargeIndicator(value);
        }
    }
}

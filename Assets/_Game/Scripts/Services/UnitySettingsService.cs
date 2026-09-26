using System;
using UnityEngine;

namespace Summit.Game.Services
{
    public sealed class UnitySettingsService : ISettingsService
    {
        private const string MasterVolumeKey = "Settings.MasterVolume";
        private const string FullscreenKey = "Settings.Fullscreen";
        private const string CameraShakeKey = "Settings.CameraShake";
        private const string ChargeIndicatorKey = "Settings.ChargeIndicator";

        private readonly ISaveService saveService;

        public UnitySettingsService(ISaveService persistence)
        {
            saveService = persistence;
            MasterVolume = saveService.GetFloat(MasterVolumeKey, 0.8f);
            Fullscreen = saveService.GetFloat(FullscreenKey, Screen.fullScreen ? 1f : 0f) > 0.5f;
            CameraShake = saveService.GetFloat(CameraShakeKey, 1f) > 0.5f;
            ShowChargeIndicator = saveService.GetFloat(ChargeIndicatorKey, 1f) > 0.5f;
            Apply();
        }

        public event Action SettingsChanged;

        public float MasterVolume { get; private set; }
        public bool Fullscreen { get; private set; }
        public bool CameraShake { get; private set; }
        public bool ShowChargeIndicator { get; private set; }

        public void SetMasterVolume(float value)
        {
            MasterVolume = Mathf.Clamp01(value);
            saveService.SetFloat(MasterVolumeKey, MasterVolume);
            ApplyAndNotify();
        }

        public void SetFullscreen(bool value)
        {
            Fullscreen = value;
            saveService.SetFloat(FullscreenKey, value ? 1f : 0f);
            ApplyAndNotify();
        }

        public void SetCameraShake(bool value)
        {
            CameraShake = value;
            saveService.SetFloat(CameraShakeKey, value ? 1f : 0f);
            SettingsChanged?.Invoke();
        }

        public void SetShowChargeIndicator(bool value)
        {
            ShowChargeIndicator = value;
            saveService.SetFloat(ChargeIndicatorKey, value ? 1f : 0f);
            SettingsChanged?.Invoke();
        }

        private void ApplyAndNotify()
        {
            Apply();
            SettingsChanged?.Invoke();
        }

        private void Apply()
        {
            AudioListener.volume = MasterVolume;
            Screen.fullScreen = Fullscreen;
        }
    }
}

using System;

namespace Summit.Game.Services
{
    public interface ISettingsService
    {
        event Action SettingsChanged;
        float MasterVolume { get; }
        bool Fullscreen { get; }
        bool CameraShake { get; }
        bool ShowChargeIndicator { get; }
        void SetMasterVolume(float value);
        void SetFullscreen(bool value);
        void SetCameraShake(bool value);
        void SetShowChargeIndicator(bool value);
    }
}

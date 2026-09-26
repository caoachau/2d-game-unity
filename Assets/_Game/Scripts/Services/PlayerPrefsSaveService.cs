using UnityEngine;

namespace Summit.Game.Services
{
    public sealed class PlayerPrefsSaveService : ISaveService
    {
        public float GetFloat(string key, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Summit.Game.Services
{
    public sealed class UnitySceneService : ISceneService
    {
        public void StartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Area2_Cave");
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        public void Quit()
        {
#if UNITY_EDITOR
            Debug.Log("Quit requested. The Editor keeps running.");
#else
            Application.Quit();
#endif
        }
    }
}

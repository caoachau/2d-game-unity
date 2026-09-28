using UnityEngine;
using UnityEngine.SceneManagement;

namespace Summit.Game.Services
{
    public sealed class UnitySceneService : ISceneService
    {
        private readonly string gameplayScene;

        public UnitySceneService(string levelScene = null)
        {
            gameplayScene = levelScene;
        }

        public void StartGame()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            SceneManager.LoadScene("Area_1_Forest");
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            SceneManager.LoadScene(gameplayScene ?? SceneManager.GetActiveScene().name);
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
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

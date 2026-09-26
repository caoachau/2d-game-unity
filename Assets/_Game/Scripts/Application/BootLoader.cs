using UnityEngine;
using UnityEngine.SceneManagement;

namespace Summit.Game.Application
{
    public sealed class BootLoader : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}

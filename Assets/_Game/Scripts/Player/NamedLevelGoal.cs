using Summit.Game.Application;
using Summit.Game.Input;
using Summit.Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Summit.Game.Player
{
    [DisallowMultipleComponent]
    public sealed class NamedLevelGoal : MonoBehaviour
    {
        private PlayerJumpController player;
        private Rigidbody2D playerBody;
        private bool completed;
        private bool presentedByHud;

        public bool IsCompleted => completed;

        public void Configure(PlayerJumpController targetPlayer)
        {
            player = targetPlayer;
            playerBody = player != null ? player.GetComponent<Rigidbody2D>() : null;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (completed || player == null || !player.enabled || playerBody == null ||
                other == null || other.attachedRigidbody != playerBody)
                return;

            completed = true;
            player.enabled = false;
            playerBody.linearVelocity = Vector2.zero;
            playerBody.angularVelocity = 0f;

            // Input and timers may live on a separate scene Systems object.
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            {
                foreach (PlayerInputReader input in root.GetComponentsInChildren<PlayerInputReader>())
                    input.enabled = false;
                foreach (GameRunTimer timer in root.GetComponentsInChildren<GameRunTimer>())
                    timer.Stop();
            }

            SharedLevelHud hud = SharedLevelHud.FindInScene<SharedLevelHud>(gameObject.scene);
            if (hud != null)
                presentedByHud = hud.CompleteLevel(gameObject.scene.name == "Area_3_OldCastle");
            Time.timeScale = 0f;
        }

        private void OnGUI()
        {
            if (!completed || presentedByHud)
                return;

            float width = Mathf.Min(440f, Screen.width * 0.9f);
            Rect panel = new Rect((Screen.width - width) * 0.5f,
                (Screen.height - 170f) * 0.5f, width, 170f);
            GUI.Box(panel, GUIContent.none);
            GUIStyle title = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            GUI.Label(new Rect(panel.x + 15f, panel.y + 25f, width - 30f, 60f),
                "HOÀN THÀNH MÀN!", title);

            if (GUI.Button(new Rect(panel.center.x - 80f, panel.y + 110f, 160f, 40f), "CHƠI LẠI"))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(gameObject.scene.name);
            }
        }

        private void OnDestroy()
        {
            if (completed)
                Time.timeScale = 1f;
        }
    }
}

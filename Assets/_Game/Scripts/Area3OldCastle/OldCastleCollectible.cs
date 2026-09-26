using UnityEngine;

namespace Summit.Game.Area3OldCastle
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class OldCastleCollectible : MonoBehaviour
    {
        [SerializeField] private int value = 1;
        [SerializeField] private float bobAmplitude = 0.10f;
        [SerializeField] private float bobSpeed = 2.2f;
        private Vector3 startPosition;
        private bool collected;

        private void Awake() => startPosition = transform.position;

        private void Update()
        {
            if (!collected)
            {
                transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobAmplitude);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected || other.attachedRigidbody == null) return;
            collected = true;
            int total = PlayerPrefs.GetInt("Area3OldCastle.Gems", 0) + value;
            PlayerPrefs.SetInt("Area3OldCastle.Gems", total);
            PlayerPrefs.Save();
            gameObject.SetActive(false);
        }
    }
}

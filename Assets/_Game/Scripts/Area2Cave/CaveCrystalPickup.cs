using UnityEngine;

namespace Summit.Game.Area2Cave
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class CaveCrystalPickup : MonoBehaviour
    {
        [SerializeField] private int value = 1;
        [SerializeField] private float bobAmplitude = 0.12f;
        [SerializeField] private float bobSpeed = 2f;

        private Vector3 startPosition;
        private bool collected;

        private void Awake()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            if (!collected)
            {
                transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobAmplitude);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected || other.attachedRigidbody == null)
            {
                return;
            }

            collected = true;
            int total = PlayerPrefs.GetInt("Area2Cave.Crystals", 0) + value;
            PlayerPrefs.SetInt("Area2Cave.Crystals", total);
            PlayerPrefs.Save();
            gameObject.SetActive(false);
        }
    }
}

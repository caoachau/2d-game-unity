using System;
using Summit.Game.Services;
using UnityEngine;

namespace Summit.Game.Application
{
    public sealed class GameProgressTracker : MonoBehaviour
    {
        private const string BestHeightKey = "Progress.BestHeight";

        private Transform player;
        private ISaveService saveService;
        private float startHeight;
        private float goalHeight;
        private float lastReportedHeight = float.NaN;

        public event Action<float, float, float> ProgressChanged;

        public float CurrentHeight { get; private set; }
        public float HighestHeight { get; private set; }
        public float BestHeight { get; private set; }
        public float NormalizedProgress { get; private set; }

        public void Initialize(Transform playerTransform, ISaveService persistence, float levelStartHeight,
            float levelGoalHeight)
        {
            player = playerTransform;
            saveService = persistence;
            startHeight = levelStartHeight;
            goalHeight = Mathf.Max(startHeight + 1f, levelGoalHeight);
            BestHeight = saveService.GetFloat(BestHeightKey);
            Refresh(true);
        }

        private void Update()
        {
            Refresh(false);
        }

        private void Refresh(bool force)
        {
            if (player == null)
            {
                return;
            }

            CurrentHeight = Mathf.Max(0f, player.position.y - startHeight);
            if (CurrentHeight > HighestHeight)
            {
                HighestHeight = CurrentHeight;
            }

            if (HighestHeight > BestHeight)
            {
                BestHeight = HighestHeight;
                saveService?.SetFloat(BestHeightKey, BestHeight);
            }

            NormalizedProgress = Mathf.InverseLerp(startHeight, goalHeight, player.position.y);
            if (force || Mathf.Abs(CurrentHeight - lastReportedHeight) >= 0.05f)
            {
                lastReportedHeight = CurrentHeight;
                ProgressChanged?.Invoke(CurrentHeight, HighestHeight, BestHeight);
            }
        }
    }
}

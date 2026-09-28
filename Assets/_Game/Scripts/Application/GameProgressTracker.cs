using System;
using Summit.Game.Services;
using UnityEngine;

namespace Summit.Game.Application
{
    public sealed class GameProgressTracker : MonoBehaviour
    {
        private string bestHeightKey = "Progress.BestHeight";

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
            float levelGoalHeight, string levelId = null)
        {
            player = playerTransform;
            saveService = persistence;
            startHeight = levelStartHeight;
            goalHeight = Mathf.Max(startHeight + 1f, levelGoalHeight);
            bestHeightKey = string.IsNullOrEmpty(levelId) ? "Progress.BestHeight" : "Progress.BestHeight." + levelId;
            HighestHeight = 0f;
            lastReportedHeight = float.NaN;
            BestHeight = saveService.GetFloat(bestHeightKey);
            Refresh(true);
        }

        private void Update()
        {
            Refresh(false);
        }

        public void CaptureCurrentHeight()
        {
            Refresh(true);
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
                saveService?.SetFloat(bestHeightKey, BestHeight);
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

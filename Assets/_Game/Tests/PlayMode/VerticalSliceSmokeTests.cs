using System.Collections;
using NUnit.Framework;
using Summit.Game.Application;
using Summit.Game.Environment;
using Summit.Game.Player;
using Summit.Game.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Summit.Game.Tests.PlayMode
{
    public sealed class VerticalSliceSmokeTests
    {
        [UnityTest]
        public IEnumerator MainMenu_LoadsWithInteractiveUi()
        {
            SceneManager.LoadScene("MainMenu");
            yield return null;

            Assert.That(UnityEngine.Object.FindAnyObjectByType<MainMenuView>(), Is.Not.Null);
            Assert.That(UnityEngine.Object.FindAnyObjectByType<MainMenuBootstrapper>(), Is.Not.Null);
            Assert.That(UnityEngine.Object.FindAnyObjectByType<EventSystem>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator Game_LoadsWithPlayerHudAndGoal()
        {
            SceneManager.LoadScene("Game");
            yield return null;
            yield return null;

            Assert.That(UnityEngine.Object.FindAnyObjectByType<GameSceneBootstrapper>(), Is.Not.Null);
            Assert.That(UnityEngine.Object.FindAnyObjectByType<PlayerJumpController>(), Is.Not.Null);
            Assert.That(UnityEngine.Object.FindAnyObjectByType<GameUiController>(), Is.Not.Null);
            Assert.That(UnityEngine.Object.FindAnyObjectByType<GoalTrigger>(), Is.Not.Null);
        }
    }
}

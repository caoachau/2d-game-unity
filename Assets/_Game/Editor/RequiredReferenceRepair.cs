using Summit.Game.Application;
using Summit.Game.Configuration;
using Summit.Game.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Summit.Game.Editor
{
    [InitializeOnLoad]
    public static class RequiredReferenceRepair
    {
        private const string ConfigPath = "Assets/_Game/ScriptableObjects/Player/PlayerMovementConfig.asset";

        static RequiredReferenceRepair()
        {
            EditorApplication.delayCall += RepairLoadedGameScene;
        }

        private static void RepairLoadedGameScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            GameSceneBootstrapper bootstrapper = Object.FindAnyObjectByType<GameSceneBootstrapper>();
            if (bootstrapper == null)
            {
                return;
            }

            PlayerMovementConfig config = AssetDatabase.LoadAssetAtPath<PlayerMovementConfig>(ConfigPath);
            if (config == null)
            {
                Debug.LogError($"[RequiredReferenceRepair] Cannot load {ConfigPath}.");
                return;
            }

            Scene scene = bootstrapper.gameObject.scene;
            bool sceneWasDirty = scene.isDirty;
            bool repaired = AssignIfMissing(bootstrapper, "movementConfig", config);

            PlayerJumpController jumpController = Object.FindAnyObjectByType<PlayerJumpController>();
            if (jumpController != null)
            {
                repaired |= AssignIfMissing(jumpController, "config", config);
            }

            if (!repaired)
            {
                return;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if (!sceneWasDirty && !string.IsNullOrEmpty(scene.path))
            {
                EditorSceneManager.SaveScene(scene);
            }

            Debug.Log("[RequiredReferenceRepair] Restored PlayerMovementConfig references in the loaded Game scene.");
        }

        private static bool AssignIfMissing(Object target, string fieldName, Object value)
        {
            SerializedObject serialized = new(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property == null || property.objectReferenceValue != null)
            {
                return false;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }
    }
}

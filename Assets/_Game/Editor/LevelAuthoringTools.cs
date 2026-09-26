using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Summit.Game.Editor
{
    /// <summary>
    /// Small editor-only helpers for a drag/drop level-design workflow.
    /// These tools intentionally use native Unity Scene/Prefab/Tilemap objects
    /// so the project stays editable without custom runtime dependencies.
    /// </summary>
    public static class LevelAuthoringTools
    {
        private const string AuthoringRoot = "Assets/_Game/AuthoringLibrary";
        private const string AuthoringPrefabRoot = "Assets/_Game/Prefabs/Authoring";

        [MenuItem("Fallen Spires/Authoring/Open Authoring Library", priority = 0)]
        public static void OpenAuthoringLibrary()
        {
            Object folder = AssetDatabase.LoadAssetAtPath<Object>(AuthoringRoot);
            if (folder == null)
            {
                Debug.LogError($"[Authoring] Missing folder: {AuthoringRoot}");
                return;
            }

            Selection.activeObject = folder;
            EditorGUIUtility.PingObject(folder);
        }

        [MenuItem("Fallen Spires/Authoring/Create Standard Level Hierarchy", priority = 20)]
        public static void CreateStandardLevelHierarchy()
        {
            GameObject level = FindOrCreateRoot("Level");
            string[] zones =
            {
                "Zone_01_Forest",
                "Zone_02_Cave",
                "Zone_03_Castle",
                "Zone_04_Tower",
                "Zone_05_Summit"
            };

            Undo.RegisterCreatedObjectUndo(level, "Create Level Hierarchy");
            foreach (string zoneName in zones)
            {
                Transform zone = FindOrCreateChild(level.transform, zoneName);
                FindOrCreateChild(zone, "Ground");
                FindOrCreateChild(zone, "Platforms");
                FindOrCreateChild(zone, "Decoration");
                FindOrCreateChild(zone, "Background");
                FindOrCreateChild(zone, "Triggers");
            }

            Selection.activeGameObject = level;
            MarkSceneDirty(level);
            Debug.Log("[Authoring] Standard five-zone hierarchy is ready.");
        }

        [MenuItem("Fallen Spires/Authoring/Create Ground Tilemap", priority = 21)]
        public static void CreateGroundTilemap()
        {
            Transform parent = GetSceneParent();

            GameObject gridObject = new("Grid", typeof(Grid));
            Undo.RegisterCreatedObjectUndo(gridObject, "Create Ground Tilemap");
            Undo.SetTransformParent(gridObject.transform, parent, "Parent Grid");

            GameObject tilemapObject = new("GroundTilemap", typeof(Tilemap), typeof(TilemapRenderer), typeof(TilemapCollider2D));
            Undo.SetTransformParent(tilemapObject.transform, gridObject.transform, "Parent Ground Tilemap");

            TilemapRenderer renderer = tilemapObject.GetComponent<TilemapRenderer>();
            renderer.sortingOrder = 0;

            Selection.activeGameObject = tilemapObject;
            MarkSceneDirty(tilemapObject);
            Debug.Log("[Authoring] Ground Tilemap created. Open Window > 2D > Tile Palette and drag sprites from AuthoringLibrary/World into a palette.");
        }

        [MenuItem("Fallen Spires/Authoring/Create Platform From Selected Sprite", priority = 40)]
        public static void CreatePlatformFromSelectedSprite()
        {
            Sprite sprite = GetSelectedSprite();
            if (sprite == null)
            {
                return;
            }

            GameObject go = new($"Platform_{SanitizeName(sprite.name)}", typeof(SpriteRenderer), typeof(BoxCollider2D));
            Undo.RegisterCreatedObjectUndo(go, "Create Platform From Sprite");
            Undo.SetTransformParent(go.transform, GetSceneParent(), "Parent Platform");

            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 0;

            BoxCollider2D collider = go.GetComponent<BoxCollider2D>();
            collider.size = sprite.bounds.size;
            collider.offset = sprite.bounds.center;

            PlaceAtSceneViewCenter(go.transform);
            Selection.activeGameObject = go;
            MarkSceneDirty(go);
        }

        [MenuItem("Fallen Spires/Authoring/Create Platform From Selected Sprite", true)]
        private static bool ValidateCreatePlatform() => GetSelectedSprite() != null;

        [MenuItem("Fallen Spires/Authoring/Create Decoration From Selected Sprite", priority = 41)]
        public static void CreateDecorationFromSelectedSprite()
        {
            Sprite sprite = GetSelectedSprite();
            if (sprite == null)
            {
                return;
            }

            GameObject go = new($"Decor_{SanitizeName(sprite.name)}", typeof(SpriteRenderer));
            Undo.RegisterCreatedObjectUndo(go, "Create Decoration From Sprite");
            Undo.SetTransformParent(go.transform, GetSceneParent(), "Parent Decoration");
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 5;
            PlaceAtSceneViewCenter(go.transform);
            Selection.activeGameObject = go;
            MarkSceneDirty(go);
        }

        [MenuItem("Fallen Spires/Authoring/Create Decoration From Selected Sprite", true)]
        private static bool ValidateCreateDecoration() => GetSelectedSprite() != null;

        [MenuItem("Fallen Spires/Authoring/Create Background From Selected Sprite", priority = 42)]
        public static void CreateBackgroundFromSelectedSprite()
        {
            Sprite sprite = GetSelectedSprite();
            if (sprite == null)
            {
                return;
            }

            GameObject go = new($"Background_{SanitizeName(sprite.name)}", typeof(SpriteRenderer));
            Undo.RegisterCreatedObjectUndo(go, "Create Background From Sprite");
            Undo.SetTransformParent(go.transform, GetSceneParent(), "Parent Background");
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -100;
            PlaceAtSceneViewCenter(go.transform);
            Selection.activeGameObject = go;
            MarkSceneDirty(go);
        }

        [MenuItem("Fallen Spires/Authoring/Create Background From Selected Sprite", true)]
        private static bool ValidateCreateBackground() => GetSelectedSprite() != null;

        [MenuItem("Fallen Spires/Authoring/Create UI Image From Selected Sprite", priority = 43)]
        public static void CreateUiImageFromSelectedSprite()
        {
            Sprite sprite = GetSelectedSprite();
            if (sprite == null)
            {
                return;
            }

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new("AuthoringCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Undo.RegisterCreatedObjectUndo(canvasObject, "Create Authoring Canvas");
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            GameObject imageObject = new($"Image_{SanitizeName(sprite.name)}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            Undo.RegisterCreatedObjectUndo(imageObject, "Create UI Image From Sprite");
            Undo.SetTransformParent(imageObject.transform, canvas.transform, "Parent UI Image");
            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(Mathf.Max(32f, sprite.rect.width * 2f), Mathf.Max(32f, sprite.rect.height * 2f));
            Selection.activeGameObject = imageObject;
            MarkSceneDirty(imageObject);
        }

        [MenuItem("Fallen Spires/Authoring/Create UI Image From Selected Sprite", true)]
        private static bool ValidateCreateUiImage() => GetSelectedSprite() != null;

        [MenuItem("Fallen Spires/Authoring/Save Selected Scene Object As Prefab", priority = 60)]
        public static void SaveSelectedAsPrefab()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null || EditorUtility.IsPersistent(selected))
            {
                return;
            }

            EnsureAssetFolder(AuthoringPrefabRoot);
            string filename = SanitizeName(selected.name) + ".prefab";
            string path = AssetDatabase.GenerateUniqueAssetPath($"{AuthoringPrefabRoot}/{filename}");
            GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(selected, path, InteractionMode.UserAction);
            if (prefab != null)
            {
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
                Debug.Log($"[Authoring] Prefab saved: {path}");
            }
        }

        [MenuItem("Fallen Spires/Authoring/Save Selected Scene Object As Prefab", true)]
        private static bool ValidateSaveSelectedAsPrefab()
        {
            GameObject selected = Selection.activeGameObject;
            return selected != null && !EditorUtility.IsPersistent(selected);
        }

        [MenuItem("Fallen Spires/Authoring/Validate Authoring Setup", priority = 80)]
        public static void ValidateAuthoringSetup()
        {
            string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { AuthoringRoot });
            bool hasGameScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/_Game/Scenes/Game.unity") != null;
            bool hasPlayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Player/Player.prefab") != null;
            bool hasLibrary = AssetDatabase.IsValidFolder(AuthoringRoot);

            string status = $"[Authoring] Library={(hasLibrary ? "OK" : "MISSING")}, Sprites={spriteGuids.Length}, GameScene={(hasGameScene ? "OK" : "MISSING")}, PlayerPrefab={(hasPlayerPrefab ? "OK" : "MISSING")}";
            if (hasLibrary && hasGameScene && hasPlayerPrefab && spriteGuids.Length > 0)
            {
                Debug.Log(status);
            }
            else
            {
                Debug.LogWarning(status);
            }
        }

        private static Sprite GetSelectedSprite()
        {
            if (Selection.activeObject is Sprite directSprite)
            {
                return directSprite;
            }

            if (Selection.activeObject == null)
            {
                return null;
            }

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Transform GetSceneParent()
        {
            GameObject active = Selection.activeGameObject;
            if (active != null && !EditorUtility.IsPersistent(active))
            {
                return active.transform;
            }

            GameObject level = GameObject.Find("Level");
            return level != null ? level.transform : null;
        }

        private static GameObject FindOrCreateRoot(string name)
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null)
            {
                return existing;
            }

            GameObject created = new(name);
            Undo.RegisterCreatedObjectUndo(created, $"Create {name}");
            return created;
        }

        private static Transform FindOrCreateChild(Transform parent, string childName)
        {
            Transform existing = parent.Find(childName);
            if (existing != null)
            {
                return existing;
            }

            GameObject child = new(childName);
            Undo.RegisterCreatedObjectUndo(child, $"Create {childName}");
            Undo.SetTransformParent(child.transform, parent, $"Parent {childName}");
            return child.transform;
        }

        private static void PlaceAtSceneViewCenter(Transform transform)
        {
            SceneView view = SceneView.lastActiveSceneView;
            if (view == null || view.camera == null)
            {
                transform.position = Vector3.zero;
                return;
            }

            Vector3 center = view.pivot;
            transform.position = new Vector3(center.x, center.y, 0f);
        }

        private static string SanitizeName(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
            }

            return value.Replace(' ', '_');
        }

        private static void EnsureAssetFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

        private static void MarkSceneDirty(GameObject gameObject)
        {
            if (gameObject != null && gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }
    }
}

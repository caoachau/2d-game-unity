using Summit.Game.Services;
using Summit.Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Summit.Game.Application
{
    public sealed class MainMenuBootstrapper : MonoBehaviour
    {
        [SerializeField] private MainMenuView menuView;

        private void Awake()
        {
            ISaveService saveService = new PlayerPrefsSaveService();
            ISettingsService settingsService = new UnitySettingsService(saveService);
            ISceneService sceneService = new UnitySceneService();
            menuView.Initialize(sceneService, settingsService);
            ApplyButtonArtworkMaterial("StartJourneyButton");
            ApplyButtonArtworkMaterial("SettingsButton");
            ApplyButtonArtworkMaterial("QuitButton");
            CropButtonArtwork("StartJourneyButton", new Rect(82f, 187f, 1997f, 383f), new Rect(30f, 153f, 2111f, 433f));
            CropButtonArtwork("SettingsButton", new Rect(52f, 187f, 2067f, 365f), new Rect(34f, 161f, 2103f, 425f));
            CropButtonArtwork("QuitButton", new Rect(58f, 78f, 2389f, 417f), new Rect(40f, 167f, 2089f, 409f));
        }

        private static void ApplyButtonArtworkMaterial(string objectName)
        {
            GameObject buttonObject = GameObject.Find(objectName);
            Image image = buttonObject != null ? buttonObject.GetComponent<Image>() : null;
            Shader shader = Shader.Find("Summit/UI/Black Key");
            if (image != null && shader != null)
                image.material = new Material(shader);
        }

        private static void CropButtonArtwork(string objectName, Rect normalRect, Rect hoverRect)
        {
            GameObject buttonObject = GameObject.Find(objectName);
            Image image = buttonObject != null ? buttonObject.GetComponent<Image>() : null;
            Button button = buttonObject != null ? buttonObject.GetComponent<Button>() : null;
            if (image == null || button == null || image.sprite == null)
                return;

            image.sprite = CropSprite(image.sprite, normalRect);
            SpriteState state = button.spriteState;
            if (state.highlightedSprite != null)
            {
                Sprite hover = CropSprite(state.highlightedSprite, hoverRect);
                state.highlightedSprite = hover;
                state.pressedSprite = hover;
                state.selectedSprite = hover;
                button.spriteState = state;
            }
        }

        private static Sprite CropSprite(Sprite source, Rect rect)
        {
            return Sprite.Create(source.texture, rect, new Vector2(0.5f, 0.5f), source.pixelsPerUnit);
        }
    }
}

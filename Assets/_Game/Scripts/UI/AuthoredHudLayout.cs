using System;
using UnityEngine;
using UnityEngine.UI;

namespace Summit.Game.UI
{
    // The design scene can contain either UI Images or sprites dragged into the scene.
    // Convert sprites to canvas graphics at runtime so the artwork follows the screen.
    internal static class AuthoredHudLayout
    {
        public static RectTransform FindPanel(GameUiController ui, string prefix, string fallback = null)
        {
            foreach (GameObject root in ui.gameObject.scene.GetRootGameObjects())
            foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
            {
                if (!item.name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                    (fallback == null || item.name != fallback)) continue;

                if (item is RectTransform rect && item.GetComponent<Image>() != null) return rect;
                SpriteRenderer source = item.GetComponent<SpriteRenderer>();
                if (source == null || source.sprite == null) continue;

                GameObject graphic = new GameObject(item.name + "_UI", typeof(RectTransform), typeof(Image));
                RectTransform panel = (RectTransform)graphic.transform;
                panel.SetParent(ui.transform, false);
                RectTransform canvasRect = (RectTransform)ui.transform;
                panel.anchorMin = panel.anchorMax = canvasRect.pivot;
                panel.pivot = source.sprite.pivot / source.sprite.rect.size;
                Vector3 canvasScale = ui.transform.lossyScale;
                Vector3 scale = new Vector3(item.lossyScale.x / Mathf.Max(.0001f, Mathf.Abs(canvasScale.x)),
                    item.lossyScale.y / Mathf.Max(.0001f, Mathf.Abs(canvasScale.y)), 1f);
                panel.sizeDelta = new Vector2(source.sprite.rect.width / source.sprite.pixelsPerUnit * Mathf.Abs(scale.x),
                    source.sprite.rect.height / source.sprite.pixelsPerUnit * Mathf.Abs(scale.y));
                panel.anchoredPosition = ui.transform.InverseTransformPoint(item.position);
                panel.localRotation = Quaternion.Inverse(ui.transform.rotation) * item.rotation;
                Image image = graphic.GetComponent<Image>();
                image.sprite = source.sprite;
                image.color = source.color;
                image.raycastTarget = false;
                source.enabled = false;
                // HUD stays behind the existing modal panels and buttons.
                panel.SetAsFirstSibling();
                return panel;
            }
            return null;
        }

        public static Text AddText(RectTransform parent, string name, Vector2 min, Vector2 max,
            TextAnchor alignment = TextAnchor.MiddleRight)
        {
            Transform existing = parent.Find(name);
            if (existing != null && existing.TryGetComponent(out Text label)) return label;
            GameObject child = new GameObject(name, typeof(RectTransform), typeof(Text));
            RectTransform rect = (RectTransform)child.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            Text text = child.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = new Color(1f, .94f, .76f);
            text.alignment = alignment;
            text.fontStyle = FontStyle.Bold;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 8;
            text.resizeTextMaxSize = Mathf.Clamp(Mathf.RoundToInt(parent.rect.height * .27f), 16, 38);
            text.raycastTarget = false;
            return text;
        }

        public static Slider AddChargeSlider(RectTransform parent)
        {
            Slider existing = parent.GetComponentInChildren<Slider>(true);
            if (existing != null) return existing;
            GameObject child = new GameObject("ChargeSlider", typeof(RectTransform), typeof(Slider));
            RectTransform track = (RectTransform)child.transform;
            track.SetParent(parent, false);
            track.anchorMin = new Vector2(.12f, .32f);
            track.anchorMax = new Vector2(.88f, .68f);
            track.offsetMin = track.offsetMax = Vector2.zero;
            GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            RectTransform fill = (RectTransform)fillObject.transform;
            fill.SetParent(track, false);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            Image image = fillObject.GetComponent<Image>();
            image.color = new Color(1f, .67f, .12f);
            image.raycastTarget = false;
            Slider slider = child.GetComponent<Slider>();
            slider.fillRect = fill;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.interactable = false;
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            slider.value = 0f;
            return slider;
        }
    }
}

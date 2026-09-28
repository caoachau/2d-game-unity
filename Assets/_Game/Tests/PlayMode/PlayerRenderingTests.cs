using System.Collections;
using System.IO;
using NUnit.Framework;
using Summit.Game.Player;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Summit.Game.Tests.PlayMode
{
    public sealed class PlayerRenderingTests
    {
        private Scene previousScene;
        private Scene scene;
        private UnityEngine.Camera camera;
        private SpriteRenderer visual;
        private RenderTexture target;
        private Texture2D readback;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                Assert.Ignore("Rendering verification requires a graphics device; run without -nographics.");
            previousScene = SceneManager.GetActiveScene();
            scene = SceneManager.CreateScene("PlayerRenderingTest");
            SceneManager.SetActiveScene(scene);
            GameObject actor = new GameObject("Player", typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer));
            actor.GetComponent<Rigidbody2D>().gravityScale = 0f;
            actor.GetComponent<BoxCollider2D>().size = new Vector2(.8f, 1.35f);
            actor.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, .25f);
            actor.GetComponent<SpriteRenderer>().sortingOrder = 30;
            actor.transform.localScale = new Vector3(2.39802f, 2.96122f, 1f);
            actor.AddComponent<PlayerVisualController>();
            visual = actor.transform.Find("PlayerVisual").GetComponent<SpriteRenderer>();
            // Hold the same authored frame during both renders.
            actor.GetComponent<PlayerVisualController>().enabled = false;
            camera = new GameObject("RenderCamera").AddComponent<UnityEngine.Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 2.3f;
            camera.aspect = 1f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.transform.position = visual.bounds.center + Vector3.back * 10f;
            target = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
            target.Create();
            camera.targetTexture = target;
            readback = new Texture2D(512, 512, TextureFormat.RGBA32, false);
            yield return null;
            yield return null;
            camera.enabled = false;
        }

        private Color32[] Render(Color background)
        {
            camera.backgroundColor = background;
            if (GraphicsSettings.currentRenderPipeline != null)
                RenderPipeline.SubmitRenderRequest(camera, new RenderPipeline.StandardRequest { destination = target });
            else camera.Render();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
            readback.Apply();
            RenderTexture.active = previous;
            return readback.GetPixels32();
        }

        [Test]
        public void OpaqueBodyPixels_DoNotRevealBackground_AndUseSharpSpriteRendering()
        {
            Assert.That(visual.color.a, Is.EqualTo(1f));
            Assert.That(visual.sortingOrder, Is.EqualTo(30));
            Assert.That(visual.sharedMaterial.shader.name, Is.EqualTo("Summit/Player/RecoveredSprite"));
            Assert.That(visual.sharedMaterial.shader.isSupported, Is.True);
            Assert.That(visual.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(visual.sprite.texture.mipmapCount, Is.EqualTo(1));
            Assert.That(UnityEngine.Experimental.Rendering.GraphicsFormatUtility.IsCompressedFormat(
                visual.sprite.texture.graphicsFormat), Is.False);

            Color32[] red = Render(Color.red);
            Color32[] blue = Render(Color.blue);
            Assert.That(red[0].r, Is.GreaterThan(240));
            Assert.That(blue[0].b, Is.GreaterThan(240));
#if UNITY_EDITOR
            Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            source.LoadImage(File.ReadAllBytes(Path.Combine(UnityEngine.Application.dataPath, "..",
                UnityEditor.AssetDatabase.GetAssetPath(visual.sprite.texture))));
            Color32[] pixels = source.GetPixels32();
            int checkedPixels = 0;
            int leakingPixels = 0;
            int errorPixels = 0;
            int recoveredPixels = 0;
            for (int y = 0; y < source.height; y++)
            for (int x = 0; x < source.width; x++)
            {
                Color32 original = pixels[y * source.width + x];
                bool originalBody = original.a == 255;
                // Previously erased dark fabric in this supplied frame.
                bool darkFabric = original.a == 0 && original.r > original.g + 4 && original.r > 30;
                if (!originalBody && !darkFabric) continue;
                if (darkFabric) recoveredPixels++;
                Vector2 local = (new Vector2(x + .5f, y + .5f) - visual.sprite.pivot) / visual.sprite.pixelsPerUnit;
                Vector3 viewport = camera.WorldToViewportPoint(visual.transform.TransformPoint(local));
                int index = Mathf.FloorToInt(viewport.y * 512f) * 512 + Mathf.FloorToInt(viewport.x * 512f);
                Color32 a = red[index];
                Color32 b = blue[index];
                if (Mathf.Abs(a.r - b.r) > 1 || Mathf.Abs(a.g - b.g) > 1 || Mathf.Abs(a.b - b.b) > 1)
                    leakingPixels++;
                if (a.r > 250 && a.b > 250 && a.g < 5) errorPixels++;
                checkedPixels++;
            }
            Object.Destroy(source);
            Assert.That(checkedPixels, Is.GreaterThan(500));
            Assert.That(recoveredPixels, Is.GreaterThan(30), "Verify the missing fabric as well as originally opaque pixels.");
            Assert.That(leakingPixels, Is.Zero, "Changing the background must not change any opaque body pixel.");
            Assert.That(errorPixels, Is.Zero, "The character must not render with Unity's error shader.");
            string evidence = Path.Combine(UnityEngine.Application.dataPath, "../Logs/PlayerRendering.png");
            Directory.CreateDirectory(Path.GetDirectoryName(evidence));
            Render(new Color(.1f, .15f, .22f));
            File.WriteAllBytes(evidence, readback.EncodeToPNG());
#endif
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (target != null) { target.Release(); Object.Destroy(target); }
            if (readback != null) Object.Destroy(readback);
            if (scene.IsValid())
            {
                SceneManager.SetActiveScene(previousScene);
                yield return SceneManager.UnloadSceneAsync(scene);
            }
        }
    }
}

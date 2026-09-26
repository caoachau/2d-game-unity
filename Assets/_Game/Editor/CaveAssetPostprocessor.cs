using UnityEditor;
using UnityEngine;

namespace Summit.Game.Editor
{
    public sealed class CaveAssetPostprocessor : AssetPostprocessor
    {
        private const string CaveRoot = "Assets/_Game/Area2_Cave/Art/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(CaveRoot))
            {
                return;
            }

            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            if (assetPath.EndsWith("player_idle.png"))
            {
                importer.spritePixelsPerUnit = 48f;
            }
            else if (assetPath.EndsWith("cave_backdrop.png"))
            {
                importer.spritePixelsPerUnit = 100f;
            }
            else
            {
                importer.spritePixelsPerUnit = 32f;
            }

            TextureImporterSettings settings = new();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
        }
    }
}

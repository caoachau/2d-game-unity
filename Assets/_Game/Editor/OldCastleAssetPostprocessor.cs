using UnityEditor;
using UnityEngine;

namespace Summit.Game.Editor
{
    public sealed class OldCastleAssetPostprocessor : AssetPostprocessor
    {
        private const string Root = "Assets/_Game/Area3_OldCastle/Art/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Root)) return;

            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = GetPixelsPerUnit(assetPath);
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            TextureImporterSettings settings = new();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
        }

        private static float GetPixelsPerUnit(string path)
        {
            if (path.Contains("/Characters/")) return 48f;
            if (path.Contains("/Background/")) return 100f;
            if (path.Contains("/UI/")) return 64f;
            return 32f;
        }
    }
}

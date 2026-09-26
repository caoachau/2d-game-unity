using UnityEditor;
using UnityEngine;

namespace Summit.Game.Editor
{
    /// <summary>
    /// Keeps imported kit art crisp and designer-friendly.
    /// Raw third-party files keep the project's existing 16 PPU convention.
    /// The generated AuthoringLibrary uses folder-specific PPU values so its
    /// individual PNGs can be dragged directly into scenes and UI.
    /// </summary>
    public sealed class KitAssetPostprocessor : AssetPostprocessor
    {
        private const string ThirdPartyRoot = "Assets/_Game/ThirdParty/";
        private const string AuthoringRoot = "Assets/_Game/AuthoringLibrary/";

        private void OnPreprocessTexture()
        {
            bool isThirdParty = assetPath.StartsWith(ThirdPartyRoot);
            bool isAuthoring = assetPath.StartsWith(AuthoringRoot);
            if (!isThirdParty && !isAuthoring)
            {
                return;
            }

            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = GetPixelsPerUnit(assetPath, isAuthoring);

            TextureImporterSettings settings = new();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);

            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }

        private static float GetPixelsPerUnit(string path, bool isAuthoring)
        {
            if (!isAuthoring)
            {
                // Preserve the convention already used by the playable vertical slice.
                return 16f;
            }

            if (path.Contains("/Characters/PlayerFrames/"))
            {
                return 32f;
            }

            if (path.Contains("/UI/"))
            {
                return 32f;
            }

            return 16f;
        }
    }
}

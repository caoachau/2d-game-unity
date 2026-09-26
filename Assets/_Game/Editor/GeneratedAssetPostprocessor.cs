using UnityEditor;
using UnityEngine;

namespace Summit.Game.Editor
{
    /// <summary>
    /// Import defaults for the generated Fallen Spires art pack.
    /// Keeps the new assets isolated from gameplay code while making PNGs
    /// immediately usable as crisp sprites/UI inside the Unity Editor.
    /// </summary>
    public sealed class GeneratedAssetPostprocessor : AssetPostprocessor
    {
        private const string GeneratedRoot = "Assets/_Game/GeneratedAssets/FallenSpires_AllAssets_From2Screens/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(GeneratedRoot))
            {
                return;
            }

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
            if (path.Contains("/02_Gameplay/SeparatedAssets/Character/") ||
                path.Contains("/03_ExtraCharacterSheets/"))
            {
                return 48f;
            }

            if (path.Contains("/02_Gameplay/SeparatedAssets/UI/") ||
                path.Contains("/02_Gameplay/SeparatedAssets/Environment/") ||
                path.Contains("/02_Gameplay/SeparatedAssets/Props/") ||
                path.Contains("/02_Gameplay/SeparatedAssets/NPCEnemies/") ||
                path.Contains("/02_Gameplay/SeparatedAssets/Effects/") ||
                path.Contains("/02_Gameplay/SeparatedAssets/VFX/"))
            {
                return 32f;
            }

            // Main-menu art, references and large backgrounds are mainly used in Canvas UI
            // or as designer reference images; 100 PPU gives a practical default scene scale.
            return 100f;
        }
    }
}

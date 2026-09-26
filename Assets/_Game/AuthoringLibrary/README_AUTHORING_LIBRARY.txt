FALLEN SPIRES - AUTHORING LIBRARY

This folder contains designer-friendly individual sprites extracted from the original third-party kits.
The original kits and license/readme files remain unchanged under Assets/_Game/ThirdParty.

Folders:
- World/CaveTiles: 16x16 cave tiles.
- World/ForestTiles: 16x16 forest tiles.
- World/ForestDecor: 16x16 forest decorations.
- World/CastlePieces: individual castle/platform chunks extracted from the packed sheet.
- World/CastleDecor: individual castle decorative chunks.
- Characters/PlayerFrames: player animation frames.
- Characters/SlimeFrames: 16x16 slime frames.
- Backgrounds: drag directly into Scene for parallax/background composition.
- UI: UI sprites/icons. Use the menu Fallen Spires > Authoring > Create UI Image From Selected Sprite for Canvas UI.
- Props/AnimationFrames: torch/light/diamond frames.

Designer workflow:
1. Open Assets/_Game/AuthoringLibrary.
2. Drag a sprite directly into Scene for decoration/background, OR select it and use:
   Fallen Spires > Authoring > Create Platform From Selected Sprite
   Fallen Spires > Authoring > Create Decoration From Selected Sprite
   Fallen Spires > Authoring > Create UI Image From Selected Sprite
3. Use Fallen Spires > Authoring > Save Selected Scene Object As Prefab to create reusable prefabs.
4. For Tilemap painting: Window > 2D > Tile Palette, create a palette, then drag the individual 16x16 sprites from World/CaveTiles or World/ForestTiles into the Tile Palette. Unity creates Tile assets automatically.

Do not delete Assets/_Game/ThirdParty license/readme files.

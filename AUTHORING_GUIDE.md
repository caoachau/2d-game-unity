# Fallen Spires - Unity Authoring Guide

Project version: **Unity 6000.6.0f1**.

This package keeps the existing playable project and adds a designer-friendly asset library under:

`Assets/_Game/AuthoringLibrary`

The original third-party kits and their license/readme files remain under:

`Assets/_Game/ThirdParty`

## First open

1. Open the project folder with Unity Hub using Unity **6000.6.0f1**.
2. Wait until Unity finishes importing all assets and compiling scripts.
3. Open `Assets/_Game/Scenes/Game.unity`.
4. Run **Fallen Spires > Authoring > Validate Authoring Setup**.
5. Run **Fallen Spires > Authoring > Open Authoring Library** to jump to the designer-friendly assets.

## Drag/drop workflow

### Decoration or background

Drag an individual PNG from `Assets/_Game/AuthoringLibrary` directly into the Scene view.

You can also select a sprite in the Project window and use:

- `Fallen Spires > Authoring > Create Decoration From Selected Sprite`
- `Fallen Spires > Authoring > Create Background From Selected Sprite`

### Platform with collider

Select a sprite from `AuthoringLibrary/World`, then run:

`Fallen Spires > Authoring > Create Platform From Selected Sprite`

The tool creates a normal Scene GameObject containing:

- SpriteRenderer
- BoxCollider2D

Move/duplicate/scale it normally in Scene view. When satisfied, use:

`Fallen Spires > Authoring > Save Selected Scene Object As Prefab`

Reusable prefabs are saved to `Assets/_Game/Prefabs/Authoring`.

### Tilemap painting

1. Run `Fallen Spires > Authoring > Create Ground Tilemap`, or create a rectangular Tilemap manually.
2. Open `Window > 2D > Tile Palette`.
3. Create a new palette.
4. Drag the individual sprites from:
   - `AuthoringLibrary/World/ForestTiles`
   - `AuthoringLibrary/World/CaveTiles`
   into the Tile Palette window.
5. Unity will create Tile assets and you can paint the map with the brush.

### UI

Select an icon/sprite under `AuthoringLibrary/UI`, then run:

`Fallen Spires > Authoring > Create UI Image From Selected Sprite`

This creates a normal `UnityEngine.UI.Image` under a Canvas, so it remains editable through RectTransform/Inspector.

## Recommended hierarchy

Run:

`Fallen Spires > Authoring > Create Standard Level Hierarchy`

It creates:

```text
Level
├── Zone_01_Forest
│   ├── Ground
│   ├── Platforms
│   ├── Decoration
│   ├── Background
│   └── Triggers
├── Zone_02_Cave
├── Zone_03_Castle
├── Zone_04_Tower
└── Zone_05_Summit
```

Every zone is a normal GameObject hierarchy and can be edited with Unity's native drag/drop workflow.

## Asset library contents

- `World/CaveTiles`: extracted 16x16 cave tiles.
- `World/ForestTiles`: extracted 16x16 forest tiles.
- `World/ForestDecor`: extracted forest decoration cells.
- `World/CastlePieces`: individual pieces extracted from the packed castle/platform sheet.
- `World/CastleDecor`: individual castle decorative pieces.
- `Characters/PlayerFrames`: existing player animation frames copied into one easy-to-find folder.
- `Characters/SlimeFrames`: four extracted slime frames.
- `Backgrounds`: forest and castle background layers.
- `Props/AnimationFrames`: torch, light and diamond animation frames.
- `UI/DarkAges`: extracted Dark Ages UI cells.
- `UI/Medieval`: medieval fantasy UI icons and menu elements.

## Important

- Do **not** delete `.meta` files after Unity has imported the project.
- Keep the license/readme files in `Assets/_Game/ThirdParty`.
- `Library`, `Temp`, `Logs`, `obj`, `.vs` are intentionally not included in the delivery ZIP; Unity regenerates them.
- The existing gameplay scenes/scripts are preserved. The Authoring Library is additive and does not replace the current Game scene.

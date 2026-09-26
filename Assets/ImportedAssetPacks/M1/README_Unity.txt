M2 - Separated Unity Assets
============================

Total PNG assets: 453

Folders:
- 01_vegetation
- 02_props_lights_banners
- 03_character_fx
- 04_backgrounds_weather
- 05_ui
- 06_stone_platforms
- 07_wood_structures

Every output PNG:
- is a standalone image (NOT a spritesheet)
- uses RGBA transparency
- has transparent padding around the visible asset
- is cropped from the supplied source sheets

Recommended Unity import:
1. Drag the M2 folder (or its category folders) into Assets/.
2. Texture Type: Sprite (2D and UI)
3. Sprite Mode: Single
4. Alpha Is Transparency: enabled
5. Mesh Type: Full Rect if you want predictable collider/UI bounds,
   or Tight if you prefer tighter rendering bounds.
6. Compression / Filter Mode can be adjusted per project style.

manifest.csv and manifest.json contain original source coordinates and dimensions.

Extraction note:
Assets were separated by their visible alpha-connected regions. Extremely faint
background-generation speckles were filtered out so they do not become useless
standalone PNG files.

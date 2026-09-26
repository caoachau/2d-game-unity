FALLEN SPIRES - AREA 3: OLD CASTLE

This project extends the previous Area 2 source and adds an editable Area 3 scene generator.

Gameplay goals implemented:
- Vertical level, bottom-to-top progression.
- Medium-high difficulty.
- Smaller platforms than Area 2.
- Frequent horizontal jumps requiring direction + charge planning.
- 3 checkpoints.
- Moving platforms and spike/pendulum hazards.
- Gems as optional collectibles.
- Final approach platform triggers Princess Elira dialogue WITHOUT pausing gameplay.
- After the dialogue appears, the player must make ONE FINAL HORIZONTAL JUMP to the princess platform.
- Touching Princess Elira completes Area 3.

Unity menu:
Fallen Spires > Area 3 - Old Castle > Create or Rebuild Old Castle Scene
Fallen Spires > Area 3 - Old Castle > Open Old Castle Art Folder

Generated scene:
Assets/_Game/Scenes/Area3_OldCastle.unity

Area 3 runtime code:
Assets/_Game/Scripts/Area3OldCastle/

Area 3 scene builder/importer:
Assets/_Game/Editor/Area3OldCastleSceneBuilder.cs
Assets/_Game/Editor/OldCastleAssetPostprocessor.cs

Area 3 art:
Assets/_Game/Area3_OldCastle/Art/

IMPORTANT:
The scene is generated after Unity finishes importing/compiling. If it is not automatically created, run the menu command above once.
All generated level objects are normal editable GameObjects, SpriteRenderers and Collider2D components. Platforms can be moved, duplicated and resized in Scene View.

Recommended Unity version: use the version already recorded in ProjectSettings/ProjectVersion.txt.

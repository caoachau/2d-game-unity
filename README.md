# Summit — 2D Vertical Platformer

## Game overview

Summit is an original 2D vertical platformer prototype. Hold jump to charge, choose left/neutral/right, release, and accept the consequences of the trajectory. Falling keeps the run alive and sends the player back through the level instead of showing a game-over screen.

## Current status

The repository has been initialized from Unity's official 2D template. The project now contains the pure jump calculation, input abstraction, configurable movement, stable collider cast grounding, player state transitions, landing classification, fall tuning, pause/settings/save services, a vertical camera follow component, and authored Boot/MainMenu/Game scenes.

The supplied fantasy kits are integrated as editable Unity UI rather than flattened screenshots. Main menu, gameplay HUD, pause, settings, summit dialogue, and victory presentation follow the supplied layouts and use responsive 1920×1080 canvases.

## Controls

- `A` / `Left Arrow`: aim left
- `D` / `Right Arrow`: aim right
- Hold `Space`: charge
- Release `Space`: jump
- `Escape`: pause/resume
- Gamepad: left stick/D-pad, south button, start

## Architecture

- `Domain`: pure states and deterministic jump math.
- `Configuration`: inspector-editable ScriptableObject tuning.
- `Application`: scene composition and use-case coordination.
- `Services`: narrow system interfaces and implementations.
- `Player`, `Input`, `Camera`, `Environment`: focused Unity adapters/components.
- `UI`: presentation layer to be populated when the UI kit arrives.

Dependencies are wired at the scene composition root. Gameplay does not call UI, PlayerPrefs, or scene-loading APIs directly.

## Player states

`Grounded → Charging → Jumping → Falling → Landing → Grounded`

Landing has a short configurable lock and reports soft/hard impact as feedback-only events.

## How to tune jump

Create or edit a `PlayerMovementConfig` asset through `Create > Summit > Player Movement Config`. Tune velocities, charge duration/curve, gravity, fall multiplier, fall cap, ground tolerance, landing lock, and hard-landing threshold without code changes.

## Asset replacement

Player and environment visuals belong under `Assets/_Game/Art`; UI belongs under `Assets/_Game/UI`; audio belongs under `Assets/_Game/Audio`. Keep gameplay components on the prefab and replace only renderer/animator children and presentation bindings.

Imported third-party kits are isolated under `Assets/_Game/ThirdParty`. Their original license/readme files are retained; see `CREDITS.md` for a concise inventory. A final princess portrait and summit illustration remain explicit replaceable slots because those assets were not present in the supplied kit.

## Build

Open with Unity `6000.6.0f1`, allow package restore, then build Windows from Build Profiles. A complete playable build is not claimed yet; scene authoring, service/UI integration, and final playtesting remain.

## Known limitations

- The princess portrait and dedicated summit illustration are represented by replaceable themed placeholders because the supplied kit does not contain them.
- The current 25-platform route is a gameplay prototype and still needs hands-on difficulty/play-feel tuning.
- Animation and audio polish are not complete.
- Default physics values are an initial hypothesis and require playtesting.

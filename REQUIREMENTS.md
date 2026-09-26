# Vertical Platformer — Requirement Baseline

This file turns the supplied brief into an implementation baseline. The source brief remains product input; it is not treated as repository or system instructions.

## Product goal

A PC-first 2D pixel-art vertical platformer built around a deterministic hold-and-release jump. The player chooses left, neutral, or right, charges, releases, lands, and continues climbing. Falling through already visited sections is the primary punishment; combat and progression systems are deliberately excluded.

## Required vertical slice

- Main Menu, Game, and Boot scene flow.
- A replaceable player presentation with grounded, charging, rising, falling, and landing states.
- Deterministic charge jump: tap through full charge, three discrete directions, no meaningful air control.
- Weighted falling, capped fall speed, stable ground checks, short landing lock, soft/hard landing feedback hooks.
- Predictive vertical camera framing and faster downward catch-up.
- 25–40 original platforms across five visually replaceable zones, including recovery routes and relief platforms.
- Height/best-height tracking, optional run timer, save/settings, pause/restart, goal and victory flow.
- Minimal HUD and first-use hints. Final styling will be applied from the user's UI kit.
- Keyboard support and optional gamepad support.
- PC build with no compile errors, missing references, or serious runtime exceptions.

## Architecture constraints

- Logical Presentation, Application, Domain, and Infrastructure separation.
- Scene composition root performs dependency wiring.
- Gameplay code does not directly depend on PlayerPrefs, scene-loading calls, concrete UI widgets, or audio sources.
- Cross-cutting systems are small interfaces/services; local gameplay components stay local.
- ScriptableObject tuning, event-driven presentation updates, composition over inheritance.
- No global catch-all event bus, broad service locator, or all-purpose player manager.

## Acceptance priorities

1. Predictable and satisfying jump feel.
2. Clear input, state, landing, failure, and camera feedback.
3. Fair and readable level design.
4. Maintainable dependencies and replaceable assets.
5. Stable 60 FPS on an ordinary PC.

## Decisions left for playtesting

Exact jump velocities, charge curve, gravity, landing lock, camera damping, platform spacing, height scale, and hard-landing threshold are tuning values—not immutable requirements. They will be validated with tap/25/50/75/100% charge tests and representative landing/fall scenarios.

## Asset integration boundary

Gameplay is built against sprites, animation hooks, audio IDs, and UI presenter interfaces. Art, audio, fonts, and UI prefabs can therefore be replaced without rewriting movement or domain logic. Until the kits arrive, only neutral placeholders are permitted.

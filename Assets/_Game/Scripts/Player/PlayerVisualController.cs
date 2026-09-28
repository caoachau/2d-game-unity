using System;
using System.Collections.Generic;
using System.Linq;
using Summit.Game.Domain;
using UnityEngine;

namespace Summit.Game.Player
{
    /// <summary>
    /// Sprite state machine for the supplied Adventurer action sequences. It only swaps animation
    /// frames; Rigidbody2D is solely responsible for world movement.
    /// </summary>
    [AddComponentMenu("Summit/Player Animator")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer), typeof(PlayerMovement), typeof(PlayerStateController))]
    public sealed class PlayerVisualController : MonoBehaviour
    {
        private const string CharacterResourcePath = "Characters/Adventurer";
        private const string CharacterMaterialPath = "Characters/AdventurerSprite";
        [SerializeField] private bool useAlternateRun;

        [SerializeField, Min(1f)] private float idleFramesPerSecond = 7f;
        [SerializeField, Min(1f)] private float runFramesPerSecond = 11f;
        [SerializeField, Min(1f)] private float actionFramesPerSecond = 12f;
        [SerializeField, Min(0.1f)] private float visualScale = 1.2f;

        private readonly Dictionary<string, Sprite[]> animations = new();
        private SpriteRenderer spriteRenderer;
        private PlayerMovement movement;
        private PlayerStateController stateController;
        private BoxCollider2D bodyCollider;
        private Transform visualTransform;
        private string activeAnimation;
        private float frameTimer;
        private int frameIndex;
        private bool activeAnimationFinished;
        private int facing = 1;
        private float activeRenderedScale;

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            stateController = GetComponent<PlayerStateController>();
            bodyCollider = GetComponent<BoxCollider2D>();
            activeRenderedScale = visualScale;
            spriteRenderer = CreateVisualRenderer(GetComponent<SpriteRenderer>());
            BuildAnimationLookup();
            if (animations.ContainsKey("idle")) BeginAnimation("idle");
        }

        private SpriteRenderer CreateVisualRenderer(SpriteRenderer source)
        {
            GameObject visual = new("PlayerVisual", typeof(SpriteRenderer));
            visual.transform.SetParent(transform, false);
            visualTransform = visual.transform;
            ApplyVisualTransform(activeRenderedScale);

            SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
            renderer.sortingLayerID = source.sortingLayerID;
            renderer.sortingOrder = source.sortingOrder;
            Material characterMaterial = Resources.Load<Material>(CharacterMaterialPath);
            if (characterMaterial != null) renderer.sharedMaterial = characterMaterial;
            renderer.color = Color.white;
            renderer.maskInteraction = SpriteMaskInteraction.None;
            source.enabled = false;
            return renderer;
        }

        private void ApplyVisualTransform(float renderedScale)
        {
            if (visualTransform == null)
            {
                return;
            }

            float colliderFoot = bodyCollider != null
                ? bodyCollider.offset.y - bodyCollider.size.y * 0.5f
                : -0.675f;
            // Align the current frame, including short crouch frames and scaled
            // running frames. A scene-specific downward offset sinks the boots.
            float spriteFoot = spriteRenderer != null && spriteRenderer.sprite != null
                ? spriteRenderer.sprite.bounds.min.y : colliderFoot;
            visualTransform.localPosition = new Vector3(bodyCollider != null ? bodyCollider.offset.x : 0f,
                colliderFoot - spriteFoot * renderedScale, 0f);
            visualTransform.localScale = Vector3.one * renderedScale;
        }

        private void Update()
        {
            if (animations.Count == 0 || Time.timeScale <= 0f)
            {
                return;
            }

            UpdateFacing();
            string nextAnimation = GetRequestedAnimation();
            if (!animations.ContainsKey(nextAnimation))
            {
                nextAnimation = "idle";
            }

            if (!string.Equals(activeAnimation, nextAnimation, StringComparison.Ordinal))
            {
                BeginAnimation(nextAnimation);
            }

            AdvanceFrame((activeAnimation == "climb" || activeAnimation == "slide") && Mathf.Abs(movement.Velocity.y) < .01f
                ? 0f : Time.deltaTime);
        }

        private void BuildAnimationLookup()
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(CharacterResourcePath);
            AddAnimation("run", sprites.Where(s => !s.name.StartsWith("run_alt_")), "run_");
            AddAnimation("run_alt", sprites, "run_alt_");
            AddAnimation("climb", sprites, "climb_up_");
            AddAnimation("slide", sprites, "slide_down_");
            // Frames 02-04 include neighbouring characters in the supplied sheet.
            // Use clean poses for anticipation and takeoff instead.
            AddSequence("idle", sprites, 1);
            AddSequence("crouch", sprites, 10, 11);
            AddSequence("jump", sprites, 5, 6, 7);
            AddSequence("fall", sprites, 8, 9);
            AddSequence("land", sprites, 10, 11, 12);
        }

        private void AddSequence(string key, Sprite[] sprites, params int[] indices)
        {
            Sprite[] frames = indices.Select(index => sprites.FirstOrDefault(
                sprite => sprite.name == $"jump_vertical_{index:00}"))
                .Where(sprite => sprite != null).ToArray();
            if (frames.Length > 0) animations[key] = frames;
        }

        private void AddAnimation(string animationName, IEnumerable<Sprite> sprites, string prefix)
        {
            Sprite[] frames = sprites
                .Where(sprite => sprite.name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(sprite => sprite.name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (frames.Length > 0)
            {
                animations[animationName] = frames;
            }
        }

        private string GetRequestedAnimation()
        {
            if (stateController.CurrentState == PlayerState.Climbing)
            {
                if (movement.Velocity.y < -.01f) return "slide";
                if (movement.Velocity.y > .01f) return "climb";
                return activeAnimation is "climb" or "slide" ? activeAnimation : "climb";
            }
            // A new jump, charge or ladder action takes priority over landing.
            if (activeAnimation == "land" && !activeAnimationFinished &&
                stateController.CurrentState == PlayerState.Grounded &&
                Mathf.Abs(movement.MoveInput) < .05f) return "land";

            return stateController.CurrentState switch
            {
                PlayerState.Charging => "crouch",
                PlayerState.Jumping => "jump",
                PlayerState.Falling => "fall",
                PlayerState.Landing => "land",
                PlayerState.Climbing => "climb",
                _ => Mathf.Abs(movement.MoveInput) > 0.05f ? (useAlternateRun ? "run_alt" : "run") : "idle"
            };
        }

        private void BeginAnimation(string animationName)
        {
            activeAnimation = animationName;
            frameTimer = 0f;
            frameIndex = 0;
            activeAnimationFinished = false;
            SetFrame();
        }

        private void AdvanceFrame(float deltaTime)
        {
            if (activeAnimationFinished)
            {
                return;
            }

            frameTimer += deltaTime;
            float frameDuration = 1f / GetFramesPerSecond(activeAnimation);
            while (frameTimer >= frameDuration && !activeAnimationFinished)
            {
                frameTimer -= frameDuration;
                Sprite[] frames = animations[activeAnimation];
                if (frameIndex < frames.Length - 1)
                {
                    frameIndex++;
                }
                else if (Loops(activeAnimation))
                {
                    frameIndex = 0;
                }
                else
                {
                    // Hold the final pose until the physical state changes.
                    activeAnimationFinished = true;
                }

                SetFrame();
            }
        }

        private float GetFramesPerSecond(string animationName)
        {
            return animationName switch
            {
                "idle" => idleFramesPerSecond,
                "run" or "run_alt" => runFramesPerSecond,
                _ => actionFramesPerSecond
            };
        }

        private static bool Loops(string animationName)
        {
            return animationName is "idle" or "run" or "run_alt" or "climb" or "slide";
        }

        private void UpdateFacing()
        {
            if (Mathf.Abs(movement.MoveInput) > 0.05f)
            {
                facing = movement.MoveInput < 0f ? -1 : 1;
            }
            else if (Mathf.Abs(movement.Velocity.x) > 0.05f)
            {
                facing = movement.Velocity.x < 0f ? -1 : 1;
            }

            spriteRenderer.flipX = facing < 0;
        }

        private void SetFrame()
        {
            Sprite[] frames = animations[activeAnimation];
            Sprite frame = frames[Mathf.Clamp(frameIndex, 0, frames.Length - 1)];
            spriteRenderer.sprite = frame;

            // Import settings normalize each action at a shared character height.
            // Trimmed sprite bounds keep boots aligned with the physical feet.
            activeRenderedScale = visualScale;
            ApplyVisualTransform(activeRenderedScale);
        }
    }
}

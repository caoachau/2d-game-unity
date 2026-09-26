using System;
using System.Collections.Generic;
using System.Linq;
using Summit.Game.Domain;
using UnityEngine;

namespace Summit.Game.Player
{
    /// <summary>
    /// Sprite state machine for the Fallen Spires character. It only swaps animation
    /// frames; Rigidbody2D is solely responsible for world movement.
    /// </summary>
    [AddComponentMenu("Summit/Player Animator")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer), typeof(PlayerMovement), typeof(PlayerStateController))]
    public sealed class PlayerVisualController : MonoBehaviour
    {
        private const string CharacterResourcePath = "Characters/FallenSpires";
        private const float PixelsPerUnit = 48f;
        private const float StandingFrameHeightPixels = 49f;

        [SerializeField, Min(1f)] private float idleFramesPerSecond = 7f;
        [SerializeField, Min(1f)] private float runFramesPerSecond = 11f;
        [SerializeField, Min(1f)] private float actionFramesPerSecond = 12f;
        [SerializeField, Min(0f)] private float visualFloorDrop = 0.55f;
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
            renderer.color = Color.white;
            renderer.maskInteraction = SpriteMaskInteraction.None;
            source.enabled = false;
            return renderer;
        }

        /// <summary>Sets a scene-specific visual correction without moving the Rigidbody2D.</summary>
        public void SetVisualFloorDrop(float floorDrop)
        {
            visualFloorDrop = Mathf.Max(0f, floorDrop);
            ApplyVisualTransform(activeRenderedScale);
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
            // Enlarging around the sprite pivot would lower its boots. Offset the
            // child back up by the same amount so its feet keep touching the floor.
            visualTransform.localPosition = new Vector3(0f,
                -visualFloorDrop - colliderFoot * (renderedScale - 1f), 0f);
            visualTransform.localScale = Vector3.one * renderedScale;
        }

        private void Update()
        {
            if (animations.Count == 0)
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

            AdvanceFrame(Time.deltaTime);
        }

        private void BuildAnimationLookup()
        {
            Sprite[] sprites = Resources.LoadAll<Texture2D>(CharacterResourcePath)
                .Select(CreateRuntimeSprite)
                .ToArray();
            AddAnimation("idle", sprites, "idle_");
            AddAnimation("run", sprites, "run_");
            // Charging intentionally stops at crouch_02; crouch_03 is not used.
            AddAnimation("crouch", sprites.Where(sprite => !string.Equals(sprite.name, "crouch_03", StringComparison.OrdinalIgnoreCase)), "crouch_");
            AddAnimation("jump", sprites, "jump_");
            AddAnimation("fall", sprites, "fall_");
            AddAnimation("land", sprites, "land_");
        }

        private Sprite CreateRuntimeSprite(Texture2D texture)
        {
            // The art files have different heights. Align their lower edge to the
            // physical bottom of the player's BoxCollider2D, so the visible boots
            // rest on a platform instead of floating above it.
            float colliderFoot = bodyCollider != null
                ? bodyCollider.offset.y - bodyCollider.size.y * 0.5f
                : -0.675f;
            float pivotY = Mathf.Clamp01((-colliderFoot * PixelsPerUnit) / texture.height);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, pivotY), PixelsPerUnit);
            sprite.name = texture.name;
            return sprite;
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
            // Never skip the jump sequence, including on a minimum-charge hop.
            if (activeAnimation == "jump" && !activeAnimationFinished)
            {
                return "jump";
            }

            // Let landing finish before returning to Idle or Run.
            if (activeAnimation == "land" && !activeAnimationFinished)
            {
                return "land";
            }

            return stateController.CurrentState switch
            {
                PlayerState.Charging => "crouch",
                PlayerState.Jumping => "jump",
                PlayerState.Falling => "fall",
                PlayerState.Landing => "land",
                _ => Mathf.Abs(movement.MoveInput) > 0.05f ? "run" : "idle"
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
                    // crouch_02, jump_05 and land_05 stay on screen.
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
                "run" => runFramesPerSecond,
                _ => actionFramesPerSecond
            };
        }

        private static bool Loops(string animationName)
        {
            return animationName is "idle" or "run" or "fall";
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

            // Run art is 44 px high while idle art is 49 px. Normalize only the
            // running animation so walking left/right never makes the hero shrink.
            activeRenderedScale = activeAnimation == "run"
                ? visualScale * StandingFrameHeightPixels / frame.texture.height
                : visualScale;
            ApplyVisualTransform(activeRenderedScale);
        }
    }
}

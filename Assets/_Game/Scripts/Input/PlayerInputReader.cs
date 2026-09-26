using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Summit.Game.Input
{
    public sealed class PlayerInputReader : MonoBehaviour, IPlayerInput
    {
        private bool jumpPressed;
        private bool jumpReleased;

        public event Action PausePressed;

        public float Horizontal { get; private set; }
        public bool JumpHeld { get; private set; }

        private void Update()
        {
            ReadKeyboard();
            ReadGamepad();
        }

        public bool ConsumeJumpPressed()
        {
            bool value = jumpPressed;
            jumpPressed = false;
            return value;
        }

        public bool ConsumeJumpReleased()
        {
            bool value = jumpReleased;
            jumpReleased = false;
            return value;
        }

        private void ReadKeyboard()
        {
            Keyboard keyboard = Keyboard.current;
            float keyboardHorizontal = 0f;

            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                {
                    keyboardHorizontal -= 1f;
                }

                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                {
                    keyboardHorizontal += 1f;
                }

                CaptureJump(keyboard.spaceKey.wasPressedThisFrame, keyboard.spaceKey.wasReleasedThisFrame,
                    keyboard.spaceKey.isPressed);

                if (keyboard.escapeKey.wasPressedThisFrame)
                {
                    PausePressed?.Invoke();
                }
            }

            Horizontal = Mathf.Clamp(keyboardHorizontal, -1f, 1f);
        }

        private void ReadGamepad()
        {
            Gamepad gamepad = Gamepad.current;
            if (gamepad == null)
            {
                return;
            }

            float gamepadHorizontal = gamepad.dpad.left.isPressed ? -1f :
                gamepad.dpad.right.isPressed ? 1f : gamepad.leftStick.x.ReadValue();
            if (Mathf.Abs(gamepadHorizontal) > Mathf.Abs(Horizontal))
            {
                Horizontal = gamepadHorizontal;
            }

            CaptureJump(gamepad.buttonSouth.wasPressedThisFrame, gamepad.buttonSouth.wasReleasedThisFrame,
                gamepad.buttonSouth.isPressed);

            if (gamepad.startButton.wasPressedThisFrame)
            {
                PausePressed?.Invoke();
            }
        }

        private void CaptureJump(bool pressed, bool released, bool held)
        {
            jumpPressed |= pressed;
            jumpReleased |= released;
            JumpHeld |= held;

            if (released && !held)
            {
                JumpHeld = false;
            }
        }

        private void LateUpdate()
        {
            bool keyboardHeld = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
            bool gamepadHeld = Gamepad.current != null && Gamepad.current.buttonSouth.isPressed;
            JumpHeld = keyboardHeld || gamepadHeld;
        }
    }
}

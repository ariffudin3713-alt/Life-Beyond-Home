using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PixelAdventure
{
    /// <summary>Thin input layer so the game supports both the new Input System and the legacy one.</summary>
    public static class GameInput
    {
        public static float MoveX
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                float value = 0f;
                Keyboard kb = Keyboard.current;
                if (kb != null)
                {
                    if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) value -= 1f;
                    if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) value += 1f;
                }
                Gamepad pad = Gamepad.current;
                if (pad != null)
                {
                    float stick = pad.leftStick.x.ReadValue();
                    if (Mathf.Abs(stick) > 0.2f) value += stick;
                    if (pad.dpad.left.isPressed) value -= 1f;
                    if (pad.dpad.right.isPressed) value += 1f;
                }
                return Mathf.Clamp(value, -1f, 1f);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Mathf.Clamp(Input.GetAxisRaw("Horizontal"), -1f, 1f);
#else
                return 0f;
#endif
            }
        }

        public static bool JumpPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && (kb.spaceKey.wasPressedThisFrame ||
                                          kb.wKey.wasPressedThisFrame ||
                                          kb.upArrowKey.wasPressedThisFrame);
                bool button = pad != null && pad.buttonSouth.wasPressedThisFrame;
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) ||
                       Input.GetKeyDown(KeyCode.UpArrow);
#else
                return false;
#endif
            }
        }

        public static bool JumpHeld
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && (kb.spaceKey.isPressed || kb.wKey.isPressed ||
                                          kb.upArrowKey.isPressed);
                bool button = pad != null && pad.buttonSouth.isPressed;
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) ||
                       Input.GetKey(KeyCode.UpArrow);
#else
                return false;
#endif
            }
        }

        public static bool ConfirmPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && (kb.enterKey.wasPressedThisFrame ||
                                          kb.numpadEnterKey.wasPressedThisFrame ||
                                          kb.spaceKey.wasPressedThisFrame);
                bool button = pad != null && (pad.startButton.wasPressedThisFrame ||
                                              pad.buttonSouth.wasPressedThisFrame);
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ||
                       Input.GetKeyDown(KeyCode.Space);
#else
                return false;
#endif
            }
        }

        public static bool PausePressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && (kb.escapeKey.wasPressedThisFrame ||
                                          kb.pKey.wasPressedThisFrame);
                bool button = pad != null && pad.startButton.wasPressedThisFrame;
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
#else
                return false;
#endif
            }
        }

        public static bool RestartPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && kb.rKey.wasPressedThisFrame;
                bool button = pad != null && pad.selectButton.wasPressedThisFrame;
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.R);
#else
                return false;
#endif
            }
        }

        public static bool MutePressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                return kb != null && kb.mKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.M);
#else
                return false;
#endif
            }
        }
    }
}

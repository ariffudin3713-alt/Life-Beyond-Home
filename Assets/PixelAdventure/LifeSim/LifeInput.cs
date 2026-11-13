using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PixelAdventure.LifeSim
{
    /// <summary>Input for the top-down life sim, supporting the new Input System and legacy input.</summary>
    public static class LifeInput
    {
        public static float X
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                float v = 0f;
                Keyboard kb = Keyboard.current;
                if (kb != null)
                {
                    if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) v -= 1f;
                    if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) v += 1f;
                }
                Gamepad pad = Gamepad.current;
                if (pad != null)
                {
                    float stick = pad.leftStick.x.ReadValue();
                    if (Mathf.Abs(stick) > 0.2f) v += stick;
                    if (pad.dpad.left.isPressed) v -= 1f;
                    if (pad.dpad.right.isPressed) v += 1f;
                }
                return Mathf.Clamp(v, -1f, 1f);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Mathf.Clamp(Input.GetAxisRaw("Horizontal"), -1f, 1f);
#else
                return 0f;
#endif
            }
        }

        public static float Y
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                float v = 0f;
                Keyboard kb = Keyboard.current;
                if (kb != null)
                {
                    if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v -= 1f;
                    if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v += 1f;
                }
                Gamepad pad = Gamepad.current;
                if (pad != null)
                {
                    float stick = pad.leftStick.y.ReadValue();
                    if (Mathf.Abs(stick) > 0.2f) v += stick;
                    if (pad.dpad.down.isPressed) v -= 1f;
                    if (pad.dpad.up.isPressed) v += 1f;
                }
                return Mathf.Clamp(v, -1f, 1f);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Mathf.Clamp(Input.GetAxisRaw("Vertical"), -1f, 1f);
#else
                return 0f;
#endif
            }
        }

        public static bool InteractPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && (kb.eKey.wasPressedThisFrame ||
                                          kb.spaceKey.wasPressedThisFrame ||
                                          kb.enterKey.wasPressedThisFrame ||
                                          kb.numpadEnterKey.wasPressedThisFrame);
                bool button = pad != null && pad.buttonSouth.wasPressedThisFrame;
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) ||
                       Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
#else
                return false;
#endif
            }
        }

        public static bool UpPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && (kb.wKey.wasPressedThisFrame ||
                                          kb.upArrowKey.wasPressedThisFrame);
                bool button = pad != null && pad.dpad.up.wasPressedThisFrame;
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
#else
                return false;
#endif
            }
        }

        public static bool DownPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && (kb.sKey.wasPressedThisFrame ||
                                          kb.downArrowKey.wasPressedThisFrame);
                bool button = pad != null && pad.dpad.down.wasPressedThisFrame;
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
#else
                return false;
#endif
            }
        }

        /// <summary>G / tombol Select gamepad: buka buku panduan.</summary>
        public static bool GuidePressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                return kb != null && kb.gKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.G);
#else
                return false;
#endif
            }
        }

        /// <summary>Klik kiri mouse pada frame ini.</summary>
        public static bool MouseClicked
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Mouse m = Mouse.current;
                return m != null && m.leftButton.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetMouseButtonDown(0);
#else
                return false;
#endif
            }
        }

        /// <summary>Ketuk layar sentuh atau klik kiri mouse pada frame ini.</summary>
        public static bool PointerTapped
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Touchscreen ts = Touchscreen.current;
                if (ts != null && ts.primaryTouch.press.wasPressedThisFrame) return true;
                Mouse m = Mouse.current;
                return m != null && m.leftButton.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
                for (int i = 0; i < Input.touchCount; i++)
                    if (Input.GetTouch(i).phase == TouchPhase.Began) return true;
                return Input.GetMouseButtonDown(0);
#else
                return false;
#endif
            }
        }

        /// <summary>Posisi ketukan / mouse dalam piksel layar.</summary>
        public static Vector2 PointerPosition
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Touchscreen ts = Touchscreen.current;
                if (ts != null && ts.primaryTouch.press.isPressed) return ts.primaryTouch.position.ReadValue();
                if (ts != null && ts.primaryTouch.press.wasReleasedThisFrame) return ts.primaryTouch.position.ReadValue();
                Mouse m = Mouse.current;
                return m != null ? m.position.ReadValue() : Vector2.zero;
#elif ENABLE_LEGACY_INPUT_MANAGER
                if (Input.touchCount > 0) return Input.GetTouch(0).position;
                return Input.mousePosition;
#else
                return Vector2.zero;
#endif
            }
        }

        /// <summary>Posisi mouse dalam piksel layar.</summary>
        public static Vector2 MousePosition
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Mouse m = Mouse.current;
                return m != null ? m.position.ReadValue() : Vector2.zero;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.mousePosition;
#else
                return Vector2.zero;
#endif
            }
        }

        /// <summary>SHIFT (kiri/kanan) ditahan / pelatuk kiri gamepad: berlari.</summary>
        public static bool RunHeld
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed);
                bool button = pad != null && pad.leftTrigger.ReadValue() > 0.4f;
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#else
                return false;
#endif
            }
        }

        /// <summary>TAB / bahu kanan gamepad: buka tablet.</summary>
        public static bool TabletPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && kb.tabKey.wasPressedThisFrame;
                bool button = pad != null && pad.rightShoulder.wasPressedThisFrame;
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.Tab);
#else
                return false;
#endif
            }
        }

        /// <summary>I / bahu kiri gamepad: buka inventori.</summary>
        public static bool InventoryPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && kb.iKey.wasPressedThisFrame;
                bool button = pad != null && pad.leftShoulder.wasPressedThisFrame;
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.I);
#else
                return false;
#endif
            }
        }

        /// <summary>X / Delete / tombol kiri gamepad: hapus slot simpanan.</summary>
        public static bool DeletePressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && (kb.xKey.wasPressedThisFrame || kb.deleteKey.wasPressedThisFrame);
                bool button = pad != null && pad.buttonWest.wasPressedThisFrame;
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Delete);
#else
                return false;
#endif
            }
        }

        /// <summary>H: kembali ke menu utama dari layar jeda.</summary>
        public static bool HomePressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && kb.hKey.wasPressedThisFrame;
                bool button = pad != null && pad.selectButton.wasPressedThisFrame;
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.H);
#else
                return false;
#endif
            }
        }

        public static bool LeftPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame);
                bool button = pad != null && pad.dpad.left.wasPressedThisFrame;
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
#else
                return false;
#endif
            }
        }

        public static bool RightPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame);
                bool button = pad != null && pad.dpad.right.wasPressedThisFrame;
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
#else
                return false;
#endif
            }
        }

        /// <summary>Roda mouse: positif = ke atas, negatif = ke bawah (satuan "takik").</summary>
        public static float ScrollWheel
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Mouse mouse = Mouse.current;
                if (mouse == null) return 0f;
                float v = mouse.scroll.ReadValue().y;
                return Mathf.Abs(v) < 0.01f ? 0f : Mathf.Sign(v) * Mathf.Max(1f, Mathf.Abs(v) / 120f);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.mouseScrollDelta.y;
#else
                return 0f;
#endif
            }
        }

        static int _cancelFrame = -1;

        /// <summary>Tombol X di layar diklik: dianggap sama dengan ESC pada frame ini.</summary>
        public static void InjectCancel() => _cancelFrame = Time.frameCount;

        public static bool CancelPressed
        {
            get
            {
                if (_cancelFrame == Time.frameCount) return true;
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && (kb.escapeKey.wasPressedThisFrame ||
                                          kb.backspaceKey.wasPressedThisFrame);
                bool button = pad != null && pad.buttonEast.wasPressedThisFrame;
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace);
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

        public static bool JournalPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && kb.jKey.wasPressedThisFrame;
                bool button = pad != null && pad.buttonNorth.wasPressedThisFrame;
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.J);
#else
                return false;
#endif
            }
        }

        /// <summary>V / tekan stik kanan gamepad: panggil atau parkir kendaraan pribadi.</summary>
        public static bool VehiclePressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && kb.vKey.wasPressedThisFrame;
                bool button = pad != null && pad.rightStickButton.wasPressedThisFrame;
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.V);
#else
                return false;
#endif
            }
        }

        /// <summary>T / tombol kiri gamepad: buka daftar tugas.</summary>
        public static bool TasksPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                Gamepad pad = Gamepad.current;
                bool key = kb != null && kb.tKey.wasPressedThisFrame;
                bool button = pad != null && pad.buttonWest.wasPressedThisFrame;
                return key || button;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.T);
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

        public static bool NewGamePressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard kb = Keyboard.current;
                return kb != null && kb.nKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.N);
#else
                return false;
#endif
            }
        }

        public static bool AnyConfirm
        {
            get { return InteractPressed; }
        }
    }
}

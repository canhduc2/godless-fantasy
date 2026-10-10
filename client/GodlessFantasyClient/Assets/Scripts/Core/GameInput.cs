using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GodlessFantasy.Core
{
    /// <summary>
    /// Bộ đọc Input đa nền tảng thông minh:
    /// Tự động sử dụng New Input System (UnityEngine.InputSystem) nếu có,
    /// và fallback về Legacy Input nếu cần. Triệt tiêu 100% lỗi InvalidOperationException!
    /// </summary>
    public static class GameInput
    {
        public static bool GetLeftMouseButtonDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
                return Mouse.current.leftButton.wasPressedThisFrame;
#endif
            try { return Input.GetMouseButtonDown(0); } catch { return false; }
        }

        public static bool GetRightMouseButtonDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
                return Mouse.current.rightButton.wasPressedThisFrame;
#endif
            try { return Input.GetMouseButtonDown(1); } catch { return false; }
        }

        public static Vector2 GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
                return Mouse.current.position.ReadValue();
#endif
            try { return Input.mousePosition; } catch { return Vector2.zero; }
        }

        public static float GetMouseScroll()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                float scrollY = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(scrollY) > 0.01f) return Mathf.Sign(scrollY);
                return 0f;
            }
#endif
            try { return Input.GetAxisRaw("Mouse ScrollWheel"); } catch { return 0f; }
        }

        public static Vector2 GetMovementVector()
        {
            Vector2 dir = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) dir.y += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) dir.y -= 1f;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) dir.x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) dir.x += 1f;
                return dir.normalized;
            }
#endif
            try
            {
                dir.x = Input.GetAxisRaw("Horizontal");
                dir.y = Input.GetAxisRaw("Vertical");
                return dir.normalized;
            }
            catch { return Vector2.zero; }
        }

        public static bool GetDashKeyDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
                return Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.rightShiftKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame;
#endif
            try { return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.Space); } catch { return false; }
        }

        public static bool GetSkill1KeyDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
                return Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame;
#endif
            try { return Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1); } catch { return false; }
        }

        public static bool GetSkill2KeyDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
                return Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame;
#endif
            try { return Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2); } catch { return false; }
        }

        public static bool GetSkill3KeyDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
                return Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame;
#endif
            try { return Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3); } catch { return false; }
        }

        public static bool GetSkill4KeyDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
                return Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame;
#endif
            try { return Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4); } catch { return false; }
        }

        public static bool GetHpPotionKeyDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
                return Keyboard.current.qKey.wasPressedThisFrame;
#endif
            try { return Input.GetKeyDown(KeyCode.Q); } catch { return false; }
        }

        public static bool GetManaPotionKeyDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
                return Keyboard.current.eKey.wasPressedThisFrame;
#endif
            try { return Input.GetKeyDown(KeyCode.E); } catch { return false; }
        }

        public static bool GetInteractKeyDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
                return Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame;
#endif
            try { return Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F); } catch { return false; }
        }
    }
}

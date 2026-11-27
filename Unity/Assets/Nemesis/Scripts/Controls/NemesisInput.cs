using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Nemesis.Controls
{
    public enum NemesisInputAction
    {
        Attack,
        Dodge,
        SealStrike,
        ShadowDash,
        EarthRend,
        AbyssRelease,
        Barrier,
        Interact
    }

    /// <summary>
    /// Centralized keyboard/mouse input. Uses the Input System package when enabled
    /// and falls back to Unity's legacy Input Manager for older project settings.
    /// </summary>
    public static class NemesisInput
    {
        public static Vector2 Move
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard keyboard = Keyboard.current;
                Vector2 value = Vector2.zero;
                if (keyboard != null)
                {
                    value.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                            - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
                    value.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                            - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
                }

                Gamepad gamepad = Gamepad.current;
                if (gamepad != null) value += gamepad.leftStick.ReadValue();
                return Vector2.ClampMagnitude(value, 1f);
#else
                return Vector2.ClampMagnitude(new Vector2(
                    UnityEngine.Input.GetAxisRaw("Horizontal"),
                    UnityEngine.Input.GetAxisRaw("Vertical")), 1f);
#endif
            }
        }

        public static bool SprintHeld
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard keyboard = Keyboard.current;
                Gamepad gamepad = Gamepad.current;
                return (keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed))
                    || (gamepad != null && gamepad.leftStickButton.isPressed);
#else
                return UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift);
#endif
            }
        }

        public static bool WasPressed(NemesisInputAction action)
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            Gamepad gamepad = Gamepad.current;

            switch (action)
            {
                case NemesisInputAction.Attack:
                    return (keyboard != null && keyboard.fKey.wasPressedThisFrame)
                        || (mouse != null && mouse.leftButton.wasPressedThisFrame)
                        || (gamepad != null && gamepad.rightTrigger.wasPressedThisFrame);
                case NemesisInputAction.Dodge:
                    return (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
                        || (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);
                case NemesisInputAction.SealStrike:
                    return (keyboard != null && keyboard.zKey.wasPressedThisFrame)
                        || (gamepad != null && gamepad.buttonWest.wasPressedThisFrame);
                case NemesisInputAction.ShadowDash:
                    return (keyboard != null && keyboard.xKey.wasPressedThisFrame)
                        || (gamepad != null && gamepad.buttonNorth.wasPressedThisFrame);
                case NemesisInputAction.EarthRend:
                    return (keyboard != null && keyboard.cKey.wasPressedThisFrame)
                        || (gamepad != null && gamepad.leftShoulder.wasPressedThisFrame);
                case NemesisInputAction.AbyssRelease:
                    return (keyboard != null && keyboard.vKey.wasPressedThisFrame)
                        || (gamepad != null && gamepad.rightShoulder.wasPressedThisFrame);
                case NemesisInputAction.Barrier:
                    return (keyboard != null && keyboard.bKey.wasPressedThisFrame)
                        || (gamepad != null && gamepad.buttonEast.wasPressedThisFrame);
                case NemesisInputAction.Interact:
                    return (keyboard != null && keyboard.eKey.wasPressedThisFrame)
                        || (gamepad != null && gamepad.dpad.down.wasPressedThisFrame);
                default:
                    return false;
            }
#else
            switch (action)
            {
                case NemesisInputAction.Attack:
                    return UnityEngine.Input.GetMouseButtonDown(0) || UnityEngine.Input.GetKeyDown(KeyCode.F);
                case NemesisInputAction.Dodge: return UnityEngine.Input.GetKeyDown(KeyCode.Space);
                case NemesisInputAction.SealStrike: return UnityEngine.Input.GetKeyDown(KeyCode.Z);
                case NemesisInputAction.ShadowDash: return UnityEngine.Input.GetKeyDown(KeyCode.X);
                case NemesisInputAction.EarthRend: return UnityEngine.Input.GetKeyDown(KeyCode.C);
                case NemesisInputAction.AbyssRelease: return UnityEngine.Input.GetKeyDown(KeyCode.V);
                case NemesisInputAction.Barrier: return UnityEngine.Input.GetKeyDown(KeyCode.B);
                case NemesisInputAction.Interact: return UnityEngine.Input.GetKeyDown(KeyCode.E);
                default: return false;
            }
#endif
        }
    }
}

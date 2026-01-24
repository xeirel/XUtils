using UnityEngine;
using System.Collections.Generic;
using XUtils.MathUtils;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace XUtils.InputUtils
{
    public static class XInput
    {
#if ENABLE_INPUT_SYSTEM
        private static readonly Dictionary<string, InputAction> _cache = new();

        /// <summary>
        /// High level type of the last used input device.
        /// </summary>
        public enum InputTypeEnum
        {
            Gamepad,
            KeyboardMouse
        }

        /// <summary>
        /// Enables all actions in the global <see cref="InputSystem.actions"/> asset.
        /// </summary>
        public static void EnableAllActions()
        {
            foreach (var action in InputSystem.actions)
            {
                if (!action.enabled)
                    action.Enable();
            }
        }

        /// <summary>
        /// Clears the internal cache of actions. Call this if you reload or replace your actions asset at runtime.
        /// </summary>
        public static void ClearCache()
        {
            _cache.Clear();
        }

        /// <summary>
        /// Returns an input action by name, enabling it if necessary and caching the reference.
        /// </summary>
        private static InputAction FindAndEnableAction(string actionName)
        {
            if (string.IsNullOrEmpty(actionName))
                return null;

            if (_cache.TryGetValue(actionName, out var cached))
                return cached;

            var action = InputSystem.actions.FindAction(actionName);
            if (action == null)
            {
                Debug.LogWarning($"[XInput] Action '{actionName}' not found.");
                return null;
            }

            if (!action.enabled)
                action.Enable();

            _cache[actionName] = action;
            return action;
        }

        /// <summary>
        /// Returns true during the frame the given button action is pressed.
        /// Mirrors <c>Input.GetButtonDown</c> behaviour.
        /// </summary>
        public static bool GetButtonDown(string buttonName)
        {
            var action = FindAndEnableAction(buttonName);
            return action != null && action.WasPressedThisFrame();
        }

        /// <summary>
        /// Returns true the frame the given button action is released.
        /// Mirrors <c>Input.GetButtonUp</c> behaviour.
        /// </summary>
        public static bool GetButtonUp(string buttonName)
        {
            var action = FindAndEnableAction(buttonName);
            return action != null && action.WasReleasedThisFrame();
        }

        /// <summary>
        /// Returns true while the given button action is held.
        /// Mirrors <c>Input.GetButton</c> behaviour.
        /// </summary>
        public static bool GetButton(string buttonName)
        {
            var action = FindAndEnableAction(buttonName);
            return action != null && action.IsPressed();
        }

        /// <summary>
        /// Returns a scaled 2D axis value for the given action name.
        /// This is intended to feel similar to legacy <c>Input.GetAxis</c> for vector axes.
        /// </summary>
        public static Vector2 GetAxis(string axisName)
        {
            var action = FindAndEnableAction(axisName);
            if (action == null)
                return Vector2.zero;

            try
            {
                Vector2 raw = action.ReadValue<Vector2>();
                float sensitivity = axisName.Contains("Move") ? 1f : 0.05f;
                Vector2 scaled = raw * sensitivity;
                return Vector2.ClampMagnitude(scaled, 20f);
            }
            catch (System.InvalidOperationException e)
            {
                Debug.LogError($"[XInput] Cannot read Vector2 from '{axisName}': {e.Message}");
                return Vector2.zero;
            }
        }

        /// <summary>
        /// Returns a float axis value for the given action name.
        /// Mirrors <c>Input.GetAxis</c> for single-axis values (e.g. triggers).
        /// </summary>
        public static float GetAxisFloat(string axisName)
        {
            var action = FindAndEnableAction(axisName);
            if (action == null)
                return 0f;

            try
            {
                return action.ReadValue<float>();
            }
            catch (System.InvalidOperationException e)
            {
                Debug.LogError($"[XInput] Cannot read float from '{axisName}': {e.Message}");
                return 0f;
            }
        }

        /// <summary>
        /// Returns the raw (unscaled) vector value for the given axis action.
        /// Mirrors <c>Input.GetAxisRaw</c> for vector axes.
        /// </summary>
        public static Vector2 GetAxisRaw(string axisName)
        {
            var action = FindAndEnableAction(axisName);
            if (action == null)
                return Vector2.zero;

            try
            {
                return action.ReadValue<Vector2>();
            }
            catch (System.InvalidOperationException e)
            {
                Debug.LogError($"[XInput] Cannot read Vector2 from '{axisName}': {e.Message}");
                return Vector2.zero;
            }
        }

        /// <summary>
        /// Returns the effective control path of the first matching binding on the given action.
        /// Useful for displaying prompts like "Press [A]".
        /// </summary>
        public static string GetControlPath(string actionName, bool preferGamepad = false)
        {
            var action = FindAndEnableAction(actionName);
            if (action == null)
                return string.Empty;

            foreach (var binding in action.bindings)
            {
                if (binding.isComposite || binding.isPartOfComposite)
                    continue;

                if (preferGamepad && binding.effectivePath != null && binding.effectivePath.Contains("Gamepad"))
                    return binding.effectivePath;
                if (!preferGamepad && binding.effectivePath != null &&
                    (binding.effectivePath.Contains("Keyboard") || binding.effectivePath.Contains("Mouse")))
                    return binding.effectivePath;
            }

            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (!binding.isComposite)
                    continue;

                int compositeStart = i + 1;
                for (int j = compositeStart; j < action.bindings.Count && action.bindings[j].isPartOfComposite; j++)
                {
                    var partBinding = action.bindings[j];
                    if (preferGamepad && partBinding.effectivePath != null && partBinding.effectivePath.Contains("Gamepad"))
                        return partBinding.effectivePath;
                    if (!preferGamepad && partBinding.effectivePath != null &&
                        (partBinding.effectivePath.Contains("Keyboard") || partBinding.effectivePath.Contains("Mouse")))
                        return partBinding.effectivePath;
                }
            }

            foreach (var binding in action.bindings)
            {
                if (!string.IsNullOrEmpty(binding.effectivePath))
                    return binding.effectivePath;
            }

            Debug.LogWarning($"[XInput] No valid binding found for action '{actionName}'.");
            return string.Empty;
        }

        public static Quaternion WithCenterMouseOffset(this Quaternion rotation, float amplitude = 10f, float maxAngle = 30)
        {
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector2 mouseOffset = Mouse.current.position.ReadValue() - screenCenter;

            Vector2 normalized = new Vector2(mouseOffset.x / screenCenter.x, mouseOffset.y / screenCenter.y);

            Quaternion offsetRot = Quaternion.Euler((-normalized.y * amplitude).Clamp(-maxAngle, maxAngle), (normalized.x * amplitude).Clamp(-maxAngle, maxAngle), 0f);
            return rotation * offsetRot;
        }


        private static InputTypeEnum _lastInputType = InputTypeEnum.KeyboardMouse;

        static XInput()
        {
            _onInputSystemEvent = (inputEvent, device) =>
            {
                if (device is Gamepad)
                    _lastInputType = InputTypeEnum.Gamepad;
                else if (device is Keyboard || device is Mouse)
                    _lastInputType = InputTypeEnum.KeyboardMouse;
            };
            InputSystem.onEvent += _onInputSystemEvent;

#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += RemoveInputSystemListener;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

        private static System.Action<UnityEngine.InputSystem.LowLevel.InputEventPtr, InputDevice> _onInputSystemEvent;

#if UNITY_EDITOR
        private static void RemoveInputSystemListener()
        {
            if (_onInputSystemEvent != null)
                InputSystem.onEvent -= _onInputSystemEvent;
        }

        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                RemoveInputSystemListener();
        }
#endif

        /// <summary>
        /// Returns the last detected input type (gamepad or keyboard/mouse).
        /// Useful for UI prompts that adapt to the active device.
        /// </summary>
        public static InputTypeEnum InputType => _lastInputType;
#endif //ENABLE_INPUT_SYSTEM
    }
}

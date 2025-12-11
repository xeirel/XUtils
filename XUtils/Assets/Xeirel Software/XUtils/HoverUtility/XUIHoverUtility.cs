using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace XUtils.UIUtils
{
    public static class XUIHoverUtility
    {
        private static PointerEventData _pointerData = new PointerEventData(EventSystem.current);
        private static List<RaycastResult> _results = new List<RaycastResult>();
        private static GameObject _currentHovered;
        private static Vector2 _lastMousePos;

        private static float _timer = 0f;
        private static GameObject _lastHovered;

        public static GameObject CurrentHovered => _currentHovered;

        public static Action<GameObject> OnHoverChanged;
        public static Action<GameObject> OnHoverEnter;
        public static Action<GameObject> OnHoverExit;

        public static float SecondsSinceHoverEnter => _timer;

        /// <summary>
        /// Should be called every frame to update the current hovered UI element.
        /// You can call this from a MonoBehaviour's Update method.
        /// </summary>
        public static void Update()
        {
            if (!Application.isFocused || EventSystem.current == null)
            {
                _currentHovered = null;
                _timer = 0f;
                _lastHovered = null;
                return;
            }

            if (_pointerData == null)
            {
                _pointerData = new PointerEventData(EventSystem.current);
            }

            if (_pointerData == null)
            {
                _pointerData = new PointerEventData(EventSystem.current);
            }
#if ENABLE_INPUT_SYSTEM
            Vector2 currentMousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
#else
            Vector2 currentMousePos = Input.mousePosition;
#endif
            if (currentMousePos == _lastMousePos)
            {
                if (_currentHovered != null && _currentHovered == _lastHovered)
                {
                    _timer += Time.unscaledDeltaTime;
                }
                return;
            }
            _lastMousePos = currentMousePos;

            _pointerData.position = currentMousePos;
            _results.Clear();
            EventSystem.current.RaycastAll(_pointerData, _results);

            GameObject newHovered = _results.Count > 0 ? _results[0].gameObject : null;

            if (newHovered != _lastHovered)
            {
                _timer = 0f;
                _lastHovered = newHovered;
            }
            else if (newHovered != null)
            {
                _timer += Time.unscaledDeltaTime;
            }

            if (_currentHovered != newHovered)
            {
                var oldHovered = _currentHovered;
                _currentHovered = newHovered;

                // First notify listeners about exit of the old hovered object
                if (oldHovered != null)
                {
                    OnHoverExit?.Invoke(oldHovered);
                }

                // Then notify listeners about enter of the new hovered object
                if (newHovered != null)
                {
                    OnHoverEnter?.Invoke(newHovered);
                }

                // Finally, notify that hovered object has changed
                OnHoverChanged?.Invoke(newHovered);
            }
        }

        public static bool IsHovered(GameObject go)
        {
            return _currentHovered == go;
        }

        public static bool IsHovered(RectTransform rt)
        {
            return _currentHovered != null && _currentHovered.transform == rt.transform && Application.isFocused;
        }
        public static bool IsHoveredIncludingSelfAndChildren(RectTransform target)
        {
            if (CurrentHovered == null) return false;

            var hoveredTransform = CurrentHovered.transform;
            return Application.isFocused && (hoveredTransform == target || hoveredTransform.IsChildOf(target));
        }
        public static bool IsHoveredIncludingSelfAndChildren(GameObject target)
        {
            if (CurrentHovered == null) return false;

            var hoveredTransform = CurrentHovered.transform;
            return Application.isFocused && (hoveredTransform == target.transform || hoveredTransform.IsChildOf(target.transform));
        }
    }
}
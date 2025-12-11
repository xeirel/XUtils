using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace XUtils
{
    /// <summary>
    /// Double-click event interface for UI elements.
    /// Implement this on components that want to receive double-click events.
    /// </summary>
    public interface IPointerDoubleClickHandler : IEventSystemHandler
    {
        void OnPointerDoubleClick(PointerEventData eventData);
    }

    /// <summary>
    /// Internal listener that detects double-clicks based on click interval
    /// and dispatches them to IPointerDoubleClickHandler implementers.
    /// Users do not need to add this manually; UIDoubleClickHandler adds it automatically.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    internal class DoubleClickListener : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private float doubleClickThreshold = 0.3f;

        private float _lastClickTime;

        public float DoubleClickThreshold
        {
            get => doubleClickThreshold;
            set => doubleClickThreshold = Mathf.Max(0f, value);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            float timeSinceLastClick = Time.unscaledTime - _lastClickTime;

            if (timeSinceLastClick <= doubleClickThreshold)
            {
                // Dispatch double-click event to all IPointerDoubleClickHandler components on this GameObject
                ExecuteEvents.Execute<IPointerDoubleClickHandler>(
                    gameObject,
                    eventData,
                    (handler, data) => handler.OnPointerDoubleClick(eventData));
            }

            _lastClickTime = Time.unscaledTime;
        }
    }

    /// <summary>
    /// Component that exposes UI double-click as a UnityEvent and/or overridable method.
    /// Users only need to add this MonoBehaviour to a UI object; it will auto-wire the listener.
    /// </summary>
    public class UIDoubleClickHandler : MonoBehaviour, IPointerDoubleClickHandler
    {
        [Serializable]
        public class DoubleClickEvent : UnityEvent<PointerEventData>
        {
        }

        [SerializeField]
        private DoubleClickEvent onDoubleClick = new DoubleClickEvent();

        /// <summary>
        /// Inspector event invoked when a double-click is detected.
        /// </summary>
        public DoubleClickEvent OnDoubleClick => onDoubleClick;

        [Tooltip("Maximum time (seconds) between two clicks to be considered a double-click.")]
        [SerializeField]
        private float doubleClickThreshold = 0.3f;

        private void Awake()
        {
            EnsureListenerExists();
        }

        private void EnsureListenerExists()
        {
            // Ensure there is exactly one DoubleClickListener on this GameObject
            var listener = GetComponent<DoubleClickListener>();
            if (listener == null)
            {
                listener = gameObject.AddComponent<DoubleClickListener>();
            }

            listener.DoubleClickThreshold = doubleClickThreshold;
        }

        /// <summary>
        /// Called by DoubleClickListener when a double-click is detected.
        /// </summary>
        public void OnPointerDoubleClick(PointerEventData eventData)
        {
            onDoubleClick?.Invoke(eventData);
            HandleDoubleClick(eventData);
        }
        public void SetDoubleClickThreshold(float value)
        {
            doubleClickThreshold = Mathf.Max(0f, value);
        }
        /// <summary>
        /// Override this in a subclass to handle double-click in code.
        /// </summary>
        protected virtual void HandleDoubleClick(PointerEventData eventData)
        {
        }
    }
}

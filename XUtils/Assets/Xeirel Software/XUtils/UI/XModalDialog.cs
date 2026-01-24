// By @Xeirel, you can customize and extend this code as needed for your Unity projects.
// More for github.com/Xeirel

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XUtils.InputUtils;
using XUtils.UnityUtils;
namespace XUtils.UIUtils
{
    /// <summary>
    /// Provides a modal dialog manager for displaying customizable dialogs, messages, confirmations, and input prompts
    /// within a Unity application.
    /// </summary>
    /// <remarks>Use the static methods to create and display dialogs with various configurations, such as
    /// message boxes, confirmation dialogs, and input fields. The dialog is presented as a modal overlay, blocking
    /// interaction with the underlying UI until the dialog is dismissed. Only one dialog can be active at a time. This
    /// class is implemented as a singleton and should be accessed via the Instance property or static methods. Thread
    /// safety is not guaranteed; all interactions should occur on the Unity main thread.</remarks>
    public class XModalDialog : MonoBehaviour
    {
        // Singleton Instance
        public static XModalDialog Instance { get; private set; }

        // Prefabs
        [Header("Prefabs - You can replace or customize them, just match the script.")]
        public Button ButtonPrefab;
        public TMP_InputField InputFieldPrefab;
        public TMP_Text TextPrefab;

        // Static Objects
        [Header("Static Objects")]
        public TMP_Text TitleText;
        public Transform ButtonContainer;
        public Transform ContentContainer;
        public CanvasGroup ModalCanvasGroup;
        private Coroutine _fadeRoutine;

        // Runtime State
        private DialogBuilder _activeDialog;
        private DialogData _activeData;
        private readonly List<GameObject> _spawnedElements = new List<GameObject>();
        private readonly Dictionary<string, TMP_InputField> _inputFields = new Dictionary<string, TMP_InputField>();

        public bool IsDialogActive => _activeDialog != null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Hide(true);
        }

        private void Update()
        {
            bool escape = false;
#if ENABLE_INPUT_SYSTEM
            escape = XInput.GetButtonDown("Escape");
#else
            escape = Input.GetKeyDown(KeyCode.Escape);
#endif

            if (_activeDialog != null && _activeDialog.EscapeToCancel && escape)
            {
                CloseDialog(ModalDialogResult.Cancel);
            }
        }

        #region Public API

        public static DialogBuilder CreateDialog()
        {
            return new DialogBuilder();
        }

        public static DialogBuilder ShowMessage(string message, string title = "Message")
        {
            var builder = new DialogBuilder()
                .WithTitle(title)
                .WithMessage(message)
                .AddButton("OK", ModalDialogResult.OK);
            builder.Show();
            return builder;
        }

        public static DialogBuilder ShowConfirm(string message, string title = "Confirm")
        {
            var builder = new DialogBuilder()
                .WithTitle(title)
                .WithMessage(message)
                .AddButton("OK", ModalDialogResult.OK)
                .AddButton("Cancel", ModalDialogResult.Cancel);
            builder.Show();
            return builder;
        }

        public static DialogBuilder ShowYesNo(string message, string title = "Question")
        {
            var builder = new DialogBuilder()
                .WithTitle(title)
                .WithMessage(message)
                .AddButton("Yes", ModalDialogResult.Yes)
                .AddButton("No", ModalDialogResult.No);
            builder.Show();
            return builder;
        }

        public static DialogBuilder ShowYesNoCancel(string message, string title = "Question")
        {
            var builder = new DialogBuilder()
                .WithTitle(title)
                .WithMessage(message)
                .AddButton("Yes", ModalDialogResult.Yes)
                .AddButton("No", ModalDialogResult.No)
                .AddButton("Cancel", ModalDialogResult.Cancel);
            builder.Show();
            return builder;
        }

        public static DialogBuilder ShowInput(string message, string inputKey, string title = "Input", string placeholder = "", string defaultValue = "")
        {
            var builder = new DialogBuilder()
                .WithTitle(title)
                .WithMessage(message)
                .AddInputField(inputKey, placeholder, defaultValue)
                .AddButton("OK", ModalDialogResult.OK)
                .AddButton("Cancel", ModalDialogResult.Cancel);
            builder.Show();
            return builder;
        }

        #endregion

        #region Dialog Management

        public void ShowDialog(DialogBuilder builder)
        {
            if (_activeDialog != null)
            {
                CloseDialog(ModalDialogResult.Cancel);
            }

            _activeDialog = builder;
            _activeData = new DialogData();

            BuildDialogUI(builder);
            Show();
        }

        public void CloseDialog(ModalDialogResult result, string customKey = null)
        {
            if (_activeDialog == null) return;

            CollectInputValues();

            _activeData.Result = result;
            _activeData.CustomResultKey = customKey;

            var callback = _activeDialog.OnResultCallback;
            var data = _activeData;

            ClearDialog();
            Hide();

            callback?.Invoke(data);
        }

        #endregion

        #region UI Building

        private void BuildDialogUI(DialogBuilder builder)
        {
            // Title
            if (TitleText != null)
            {
                TitleText.text = builder.Title ?? "";
            }

            // Message
            if (!string.IsNullOrEmpty(builder.Message))
            {
                SpawnText(builder.Message, TextAnchor.MiddleCenter);
            }

            // Custom Texts
            foreach (var textConfig in builder.Texts)
            {
                SpawnText(textConfig.Text, textConfig.Alignment);
            }

            // Input Fields
            foreach (var inputConfig in builder.Inputs)
            {
                SpawnInputField(inputConfig);
            }

            // Buttons
            foreach (var buttonConfig in builder.Buttons)
            {
                SpawnButton(buttonConfig);
            }
        }

        private void SpawnText(string text, TextAnchor alignment)
        {
            if (TextPrefab == null || ContentContainer == null) return;

            var textObj = Instantiate(TextPrefab, ContentContainer);
            textObj.text = text;
            textObj.alignment = ConvertAlignment(alignment);
            _spawnedElements.Add(textObj.gameObject);
        }

        private void SpawnInputField(DialogBuilder.DialogInputConfig config)
        {
            if (InputFieldPrefab == null || ContentContainer == null) return;

            var inputField = Instantiate(InputFieldPrefab, ContentContainer);
            inputField.contentType = config.ContentType;
            inputField.text = config.DefaultValue ?? "";

            if (inputField.placeholder is Text placeholderText)
            {
                placeholderText.text = config.Placeholder ?? "";
            }
            else if (inputField.placeholder is TMP_Text tmpPlaceholder)
            {
                tmpPlaceholder.text = config.Placeholder ?? "";
            }

            _inputFields[config.Key] = inputField;
            _spawnedElements.Add(inputField.gameObject);
        }

        private void SpawnButton(DialogBuilder.DialogButtonConfig config)
        {
            if (ButtonPrefab == null || ButtonContainer == null) return;

            var button = Instantiate(ButtonPrefab, ButtonContainer);

            var buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = config.Text;
            }
            else
            {
                var legacyText = button.GetComponentInChildren<Text>();
                if (legacyText != null)
                {
                    legacyText.text = config.Text;
                }
            }

            var result = config.Result;
            var customKey = config.CustomKey;
            var onClick = config.OnClick;

            button.onClick.AddListener(() =>
            {
                CollectInputValues();
                onClick?.Invoke(_activeData);

                if (_activeDialog.CloseOnResult)
                {
                    CloseDialog(result, customKey);
                }
            });

            _spawnedElements.Add(button.gameObject);
        }

        private void CollectInputValues()
        {
            foreach (var kvp in _inputFields)
            {
                _activeData.SetInput(kvp.Key, kvp.Value.text);
            }
        }

        private void ClearDialog()
        {
            foreach (var element in _spawnedElements)
            {
                if (element != null)
                {
                    Destroy(element);
                }
            }
            _spawnedElements.Clear();
            _inputFields.Clear();
            _activeDialog = null;
            _activeData = null;
        }

        private TextAlignmentOptions ConvertAlignment(TextAnchor anchor)
        {
            return anchor switch
            {
                TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft,
                TextAnchor.UpperCenter => TextAlignmentOptions.Top,
                TextAnchor.UpperRight => TextAlignmentOptions.TopRight,
                TextAnchor.MiddleLeft => TextAlignmentOptions.Left,
                TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
                TextAnchor.MiddleRight => TextAlignmentOptions.Right,
                TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft,
                TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
                TextAnchor.LowerRight => TextAlignmentOptions.BottomRight,
                _ => TextAlignmentOptions.Center
            };
        }

        #endregion

        #region Visibility

        private void Show()
        {
            if (ModalCanvasGroup != null)
            {
                Fade(1f, 0.25f);
                ModalCanvasGroup.SetActiveSafe(true);
                ModalCanvasGroup.interactable = true;
                ModalCanvasGroup.blocksRaycasts = true;
            }
        }

        private void Hide(bool skipFade = false)
        {
            if (ModalCanvasGroup != null)
            {
                ModalCanvasGroup.interactable = false;
                ModalCanvasGroup.blocksRaycasts = false;

                if (skipFade)
                {
                    ModalCanvasGroup.SetActiveSafe(false);
                    ModalCanvasGroup.alpha = 0f;
                }
                else
                    Fade(0f, 0.25f);
            }
        }

        public void Fade(float targetAlpha, float duration)
        {
            if (_fadeRoutine != null)
                StopCoroutine(_fadeRoutine);

            _fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, duration));
        }

        IEnumerator FadeRoutine(float target, float duration)
        {
            float start = ModalCanvasGroup.alpha;
            float time = 0f;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                ModalCanvasGroup.alpha = Mathf.Lerp(start, target, time / duration);
                yield return null;
            }

            ModalCanvasGroup.alpha = target;

            ModalCanvasGroup.interactable = target > 0.99f;
            ModalCanvasGroup.blocksRaycasts = target > 0.99f;
            ModalCanvasGroup.SetActiveSafe(target > 0.01f);

            _fadeRoutine = null;
        }

        #endregion
    }
    public class DialogBuilder
    {
        public struct DialogButtonConfig
        {
            public string Text;
            public ModalDialogResult Result;
            public string CustomKey;
            public Action<DialogData> OnClick;
        }

        public struct DialogInputConfig
        {
            public string Key;
            public string Placeholder;
            public string DefaultValue;
            public TMP_InputField.ContentType ContentType;
        }

        public struct DialogTextConfig
        {
            public string Text;
            public TextAnchor Alignment;
        }

        private string _title;
        private string _message;
        private readonly List<DialogButtonConfig> _buttons = new List<DialogButtonConfig>();
        private readonly List<DialogInputConfig> _inputs = new List<DialogInputConfig>();
        private readonly List<DialogTextConfig> _texts = new List<DialogTextConfig>();
        private Action<DialogData> _onResult;
        private bool _closeOnResult = true;
        private bool _escapeToCancel = true;

        public string Title => _title;
        public string Message => _message;
        public IReadOnlyList<DialogButtonConfig> Buttons => _buttons;
        public IReadOnlyList<DialogInputConfig> Inputs => _inputs;
        public IReadOnlyList<DialogTextConfig> Texts => _texts;
        public Action<DialogData> OnResultCallback => _onResult;
        public bool CloseOnResult => _closeOnResult;
        public bool EscapeToCancel => _escapeToCancel;

        public DialogBuilder WithTitle(string title)
        {
            _title = title;
            return this;
        }

        public DialogBuilder WithMessage(string message)
        {
            _message = message;
            return this;
        }

        public DialogBuilder AddButton(string text, ModalDialogResult result, Action<DialogData> onClick = null)
        {
            _buttons.Add(new DialogButtonConfig
            {
                Text = text,
                Result = result,
                CustomKey = null,
                OnClick = onClick
            });
            return this;
        }

        public DialogBuilder AddButton(string text, string customKey, Action<DialogData> onClick = null)
        {
            _buttons.Add(new DialogButtonConfig
            {
                Text = text,
                Result = ModalDialogResult.Custom,
                CustomKey = customKey,
                OnClick = onClick
            });
            return this;
        }

        public DialogBuilder AddInputField(string key, string placeholder = "", string defaultValue = "", TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard)
        {
            _inputs.Add(new DialogInputConfig
            {
                Key = key,
                Placeholder = placeholder,
                DefaultValue = defaultValue,
                ContentType = contentType
            });
            return this;
        }

        public DialogBuilder AddText(string text, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            _texts.Add(new DialogTextConfig
            {
                Text = text,
                Alignment = alignment
            });
            return this;
        }

        public DialogBuilder OnResult(Action<DialogData> callback)
        {
            _onResult = callback;
            return this;
        }

        public DialogBuilder SetCloseOnResult(bool close)
        {
            _closeOnResult = close;
            return this;
        }

        public DialogBuilder SetEscapeToCancel(bool escape)
        {
            _escapeToCancel = escape;
            return this;
        }

        public void Show()
        {
            XModalDialog.Instance.ShowDialog(this);
        }
    }
    public class DialogData
    {
        public ModalDialogResult Result { get; set; } = ModalDialogResult.None;
        public string CustomResultKey { get; set; }

        private readonly Dictionary<string, string> _inputValues = new Dictionary<string, string>();

        public void SetInput(string key, string value)
        {
            _inputValues[key] = value;
        }

        public string GetInput(string key, string defaultValue = "")
        {
            return _inputValues.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public bool TryGetInput(string key, out string value)
        {
            return _inputValues.TryGetValue(key, out value);
        }

        public IReadOnlyDictionary<string, string> GetAllInputs()
        {
            return _inputValues;
        }

        public bool HasInput(string key)
        {
            return _inputValues.ContainsKey(key);
        }
    }
    public enum ModalDialogResult
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Yes = 3,
        No = 4,
        Custom = 100
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using XUtils.CollectionsUtils;

namespace XUtils.Console
{
    public class XConsoleGUI : MonoBehaviour
    {
        private XConsole console;
        private bool visible = false;
        private float height = 486f;
        private float maxWidth = 960f;
        private int fontSize = 18;
        private int headerFontSize = 21;
        [SerializeField] private int maxVisibleLines = 18;
        [SerializeField] private float animationSpeed = 10f;

        private const float InputRowHeight = 44f;
        private const float PanelMargin = 24f;
        private const float HeaderHeight = 28f;
        private const float SubtitleHeight = 22f;
        private const float DividerHeight = 1f;
        private const float SectionSpacing = 10f;
        private const float ButtonWidth = 90f;

        private string lastInput = "";
        private string inputText = "";
        private Vector2 scrollPosition;
        private bool submitRequested;
        private bool refreshHintsRequested;
        private bool autoScrollRequested;
        private float visibilityAnimation;
        private float hintsAnimation;
        private int selectedHintIndex;
        private int inputHistoryIndex;

        public Dictionary<string, Action<string[]>> commands = new();
        private List<string> closestEntries = new();
        private readonly List<string> inputHistory = new();
        private XConsole subscribedConsole;

        private GUIStyle windowStyle;
        private GUIStyle headerStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle dividerStyle;
        private GUIStyle historyContainerStyle;
        private GUIStyle historyLabelStyle;
        private GUIStyle inputStyle;
        private GUIStyle buttonStyle;
        private GUIStyle hintsContainerStyle;
        private GUIStyle hintLabelStyle;
        private GUIStyle selectedHintLabelStyle;
        private GUIStyle scrollbarStyle;

        private Texture2D panelTexture;
        private Texture2D historyTexture;
        private Texture2D inputTexture;
        private Texture2D buttonTexture;
        private Texture2D buttonHoverTexture;
        private Texture2D hintTexture;
        private Texture2D selectedHintTexture;
        private Texture2D dividerTexture;
        private Texture2D scrollbarTexture;

        private void Awake()
        {
            visibilityAnimation = visible ? 1f : 0f;
            inputHistoryIndex = inputHistory.Count;

            if (windowStyle != null)
                ClearStyle();

            TryBindConsole();
        }

        private void OnEnable()
        {
            TryBindConsole();
        }

        private void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.f2Key.wasPressedThisFrame)
                    visible = !visible;

                if (visible && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
                    submitRequested = true;

                if (visible && Keyboard.current.tabKey.wasPressedThisFrame)
                {
                    if (closestEntries.Count > 0)
                        ApplySelectedHint();
                }

                if (visible && Keyboard.current.upArrowKey.wasPressedThisFrame)
                {
                    if (!TryMoveHintSelection(-1))
                        NavigateInputHistory(-1);
                }

                if (visible && Keyboard.current.downArrowKey.wasPressedThisFrame)
                {
                    if (!TryMoveHintSelection(1))
                        NavigateInputHistory(1);
                }
            }

            if (console == null)
                TryBindConsole();

            if (refreshHintsRequested)
            {
                refreshHintsRequested = false;
                OnInputChanged();
            }

            if (submitRequested)
            {
                submitRequested = false;
                Submit();
            }

            float lerpFactor = 1f - Mathf.Exp(-animationSpeed * Time.unscaledDeltaTime);
            visibilityAnimation = Mathf.Lerp(visibilityAnimation, visible ? 1f : 0f, lerpFactor);

            bool showHints = visible && !string.IsNullOrWhiteSpace(inputText) && closestEntries.Count > 0;
            hintsAnimation = Mathf.Lerp(hintsAnimation, showHints ? 1f : 0f, lerpFactor);
        }

        private void OnGUI()
        {
            if (!visible && visibilityAnimation <= 0.01f)
                return;

            EnsureStyles();
            DrawInputGUI();
            DrawHints();
        }

        private void OnDestroy()
        {
            UnsubscribeFromConsole();
            ClearStyle();
        }
        private void ClearStyle()
        {
            DestroyTexture(panelTexture);
            DestroyTexture(historyTexture);
            DestroyTexture(inputTexture);
            DestroyTexture(buttonTexture);
            DestroyTexture(buttonHoverTexture);
            DestroyTexture(hintTexture);
            DestroyTexture(selectedHintTexture);
            DestroyTexture(dividerTexture);
            DestroyTexture(scrollbarTexture);
            windowStyle = null;
        }
        private void DrawInputGUI()
        {
            float easedAnimation = EaseOutCubic(visibilityAnimation);
            float width = Mathf.Min(Screen.width - 40f, maxWidth);
            float left = (Screen.width - width) * 0.5f;
            float animatedHeight = Mathf.Max(1f, Mathf.Lerp(0f, height, easedAnimation));
            float top = Screen.height - animatedHeight - PanelMargin;

            Color previousColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, visibilityAnimation);

            Rect panelRect = new(left, top, width, animatedHeight);
            GUI.Box(panelRect, GUIContent.none, windowStyle);

            float contentLeft = panelRect.x + windowStyle.padding.left;
            float contentTop = panelRect.y + windowStyle.padding.top;
            float contentWidth = panelRect.width - windowStyle.padding.horizontal;
            float contentHeight = panelRect.height - windowStyle.padding.vertical;

            Rect headerRect = new(contentLeft, contentTop, contentWidth, HeaderHeight);
            GUI.Label(headerRect, "Developer Console", headerStyle);

            Rect subtitleRect = new(contentLeft, headerRect.yMax + 2f, contentWidth, SubtitleHeight);
            GUI.Label(subtitleRect, "F2  Toggle    •    Tab  Autocomplete    •    Enter  Execute", subtitleStyle);

            Rect dividerRect = new(contentLeft, subtitleRect.yMax + 6f, contentWidth, DividerHeight);
            GUI.Box(dividerRect, GUIContent.none, dividerStyle);

            float inputTop = panelRect.yMax - windowStyle.padding.bottom - InputRowHeight;
            Rect inputRect = new(contentLeft, inputTop, contentWidth - ButtonWidth - 8f, InputRowHeight);
            Rect buttonRect = new(inputRect.xMax + 8f, inputTop, ButtonWidth, InputRowHeight);

            float historyTop = dividerRect.yMax + 8f;
            float historyHeight = Mathf.Max(80f, inputRect.y - SectionSpacing - historyTop);
            Rect historyRect = new(contentLeft, historyTop, contentWidth, historyHeight);

            DrawHistory(historyRect);

            inputText = GUI.TextField(inputRect, inputText, inputStyle);

            if (inputText != lastInput)
                refreshHintsRequested = true;

            lastInput = inputText;
            bool submit = GUI.Button(buttonRect, "Run", buttonStyle);

            if (submit)
                submitRequested = true;

            GUI.color = previousColor;
        }

        private void OnInputChanged()
        {
            if (string.IsNullOrWhiteSpace(inputText))
            {
                closestEntries.Clear();
                selectedHintIndex = 0;
                return;
            }

            closestEntries = XConsoleRegistry.CommandNames.Concat(XConsoleRegistry.VariableNames).FuzzyMatch(i => i, inputText, 5).Select(x => x.item).ToList();
            selectedHintIndex = Mathf.Clamp(selectedHintIndex, 0, Mathf.Max(closestEntries.Count - 1, 0));
        }

        private void DrawHistory(Rect historyRect)
        {
            GUI.Box(historyRect, GUIContent.none, historyContainerStyle);

            Rect innerRect = new(
                historyRect.x + historyContainerStyle.padding.left,
                historyRect.y + historyContainerStyle.padding.top,
                historyRect.width - historyContainerStyle.padding.horizontal,
                historyRect.height - historyContainerStyle.padding.vertical);

            float contentWidth = innerRect.width - scrollbarStyle.fixedWidth - scrollbarStyle.margin.left;
            if (contentWidth < 1f)
                contentWidth = innerRect.width;

            float contentHeight = console == null
                ? historyLabelStyle.CalcHeight(new GUIContent("No XConsole instance found."), contentWidth)
                : CalculateHistoryContentHeight(contentWidth);

            Rect viewRect = new(0f, 0f, Mathf.Max(contentWidth, innerRect.width - 4f), Mathf.Max(contentHeight, innerRect.height));
            scrollPosition = GUI.BeginScrollView(innerRect, scrollPosition, viewRect, false, true, GUIStyle.none, scrollbarStyle);

            if (console == null)
            {
                GUI.Label(new Rect(0f, 0f, contentWidth, contentHeight), "No XConsole instance found.", historyLabelStyle);
            }
            else
            {
                IReadOnlyList<string> history = console.History;
                float currentY = 0f;
                for (int i = 0; i < history.Count; i++)
                {
                    float lineHeight = historyLabelStyle.CalcHeight(new GUIContent(history[i]), contentWidth);
                    GUI.Label(new Rect(0f, currentY, contentWidth, lineHeight), history[i], historyLabelStyle);
                    currentY += lineHeight + historyLabelStyle.margin.vertical;
                }
            }

            GUI.EndScrollView();

            if (autoScrollRequested && Event.current.type == EventType.Repaint)
            {
                scrollPosition.y = float.MaxValue;
                autoScrollRequested = false;
            }
        }

        private void DrawHints()
        {
            if (hintsAnimation <= 0.01f || closestEntries.Count == 0)
                return;

            float width = Mathf.Min(Screen.width - 40f, maxWidth);
            float left = (Screen.width - width) * 0.5f;
            float contentWidth = width - hintsContainerStyle.padding.horizontal;
            float contentHeight = CalculateHintsContentHeight(contentWidth);
            float hintHeight = Mathf.Lerp(0f, contentHeight + hintsContainerStyle.padding.vertical, EaseOutCubic(hintsAnimation));
            float y = Screen.height - height - hintHeight - 42f;

            Color previousColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, hintsAnimation * visibilityAnimation);

            Rect hintsRect = new(left, y, width, hintHeight);
            GUI.Box(hintsRect, GUIContent.none, hintsContainerStyle);

            Rect contentRect = new(
                hintsRect.x + hintsContainerStyle.padding.left,
                hintsRect.y + hintsContainerStyle.padding.top,
                hintsRect.width - hintsContainerStyle.padding.horizontal,
                hintsRect.height - hintsContainerStyle.padding.vertical);

            float currentY = 0f;
            for (int i = 0; i < closestEntries.Count; i++)
            {
                GUIStyle labelStyle = i == selectedHintIndex ? selectedHintLabelStyle : hintLabelStyle;
                string prefix = i == selectedHintIndex ? "› " : "  ";
                string text = prefix + closestEntries[i];
                float lineHeight = labelStyle.CalcHeight(new GUIContent(text), contentRect.width);
                GUI.Label(new Rect(contentRect.x, contentRect.y + currentY, contentRect.width, lineHeight), text, labelStyle);
                currentY += lineHeight + labelStyle.margin.vertical;
            }

            GUI.color = previousColor;
        }

        private void Submit()
        {
            if (console == null || string.IsNullOrWhiteSpace(inputText))
                return;

            string submittedInput = inputText;
            if (inputHistory.Count == 0 || inputHistory[inputHistory.Count - 1] != submittedInput)
                inputHistory.Add(submittedInput);

            inputHistoryIndex = inputHistory.Count;
            console.Execute(inputText);
            inputText = string.Empty;
            lastInput = string.Empty;
            closestEntries.Clear();
            selectedHintIndex = 0;
            autoScrollRequested = true;
        }

        private void TryBindConsole()
        {
            if (console == null)
            {
                console = XConsole.Instance;
                if (console == null)
                    console = FindFirstObjectByType<XConsole>(FindObjectsInactive.Include);
            }

            if (console == null || subscribedConsole == console)
                return;

            UnsubscribeFromConsole();
            subscribedConsole = console;
            subscribedConsole.EntryLogged += HandleConsoleEntryLogged;
            autoScrollRequested = true;
        }

        private void EnsureStyles()
        {
            if (windowStyle != null)
                return;

            Color panelColor = new(0.08f, 0.09f, 0.11f, 0.97f);
            Color historyColor = new(0.11f, 0.12f, 0.15f, 1f);
            Color inputColor = new(0.14f, 0.15f, 0.18f, 1f);
            Color buttonColor = new(0.20f, 0.24f, 0.31f, 1f);
            Color buttonHoverColor = new(0.27f, 0.32f, 0.40f, 1f);
            Color hintColor = new(0.09f, 0.10f, 0.13f, 0.98f);
            Color accentTextColor = new(0.92f, 0.95f, 1f, 1f);
            Color secondaryTextColor = new(0.65f, 0.72f, 0.82f, 1f);
            Color bodyTextColor = new(0.83f, 0.87f, 0.93f, 1f);

            panelTexture = CreateTexture(panelColor);
            historyTexture = CreateTexture(historyColor);
            inputTexture = CreateTexture(inputColor);
            buttonTexture = CreateTexture(buttonColor);
            buttonHoverTexture = CreateTexture(buttonHoverColor);
            hintTexture = CreateTexture(hintColor);
            selectedHintTexture = CreateTexture(new Color(0.22f, 0.30f, 0.42f, 1f));
            dividerTexture = CreateTexture(new Color(1f, 1f, 1f, 0.08f));
            scrollbarTexture = CreateTexture(new Color(0.24f, 0.28f, 0.36f, 1f));

            windowStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(18, 18, 16, 18),
                margin = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(1, 1, 1, 1)
            };
            windowStyle.normal.background = panelTexture;

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = headerFontSize,
                fontStyle = FontStyle.Bold,
                margin = new RectOffset(0, 0, 0, 2)
            };
            headerStyle.normal.textColor = accentTextColor;

            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize - 1,
                margin = new RectOffset(0, 0, 0, 10)
            };
            subtitleStyle.normal.textColor = secondaryTextColor;

            dividerStyle = new GUIStyle(GUI.skin.box)
            {
                margin = new RectOffset(0, 0, 4, 8),
                padding = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(0, 0, 0, 0)
            };
            dividerStyle.normal.background = dividerTexture;

            historyContainerStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(12, 12, 10, 10),
                margin = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(1, 1, 1, 1)
            };
            historyContainerStyle.normal.background = historyTexture;

            historyLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                wordWrap = true,
                richText = true,
                margin = new RectOffset(0, 0, 2, 2)
            };
            historyLabelStyle.normal.textColor = bodyTextColor;

            inputStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = fontSize + 1,
                padding = new RectOffset(14, 14, 10, 10),
                margin = new RectOffset(0, 8, 0, 0),
                border = new RectOffset(8, 8, 8, 8),
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = InputRowHeight
            };
            inputStyle.normal.background = inputTexture;
            inputStyle.focused.background = inputTexture;
            inputStyle.hover.background = inputTexture;
            inputStyle.active.background = inputTexture;
            inputStyle.normal.textColor = accentTextColor;
            inputStyle.focused.textColor = accentTextColor;
            inputStyle.hover.textColor = accentTextColor;
            inputStyle.active.textColor = accentTextColor;

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = fontSize + 1,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(12, 12, 10, 10),
                margin = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(8, 8, 8, 8),
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = InputRowHeight
            };
            buttonStyle.normal.background = buttonTexture;
            buttonStyle.hover.background = buttonHoverTexture;
            buttonStyle.active.background = buttonHoverTexture;
            buttonStyle.normal.textColor = accentTextColor;
            buttonStyle.hover.textColor = accentTextColor;
            buttonStyle.active.textColor = accentTextColor;

            hintsContainerStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(14, 14, 10, 10),
                margin = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(1, 1, 1, 1)
            };
            hintsContainerStyle.normal.background = hintTexture;

            hintLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                margin = new RectOffset(0, 0, 2, 2),
                padding = new RectOffset(8, 8, 6, 6)
            };
            hintLabelStyle.normal.textColor = accentTextColor;

            selectedHintLabelStyle = new GUIStyle(hintLabelStyle);
            selectedHintLabelStyle.normal.background = selectedHintTexture;
            selectedHintLabelStyle.normal.textColor = Color.white;

            scrollbarStyle = new GUIStyle(GUI.skin.verticalScrollbar)
            {
                fixedWidth = 12f,
                margin = new RectOffset(8, 0, 0, 0)
            };
            scrollbarStyle.normal.background = scrollbarTexture;
            scrollbarStyle.hover.background = scrollbarTexture;
            scrollbarStyle.active.background = scrollbarTexture;
        }

        private bool TryMoveHintSelection(int direction)
        {
            if (closestEntries.Count == 0 || string.IsNullOrWhiteSpace(inputText))
                return false;

            selectedHintIndex = Mathf.Clamp(selectedHintIndex + direction, 0, closestEntries.Count - 1);
            return true;
        }

        private void ApplySelectedHint()
        {
            if (closestEntries.Count == 0)
                return;

            selectedHintIndex = Mathf.Clamp(selectedHintIndex, 0, closestEntries.Count - 1);
            inputText = closestEntries[selectedHintIndex];
            lastInput = inputText;
            refreshHintsRequested = true;
        }

        private void NavigateInputHistory(int direction)
        {
            if (inputHistory.Count == 0)
                return;

            inputHistoryIndex = Mathf.Clamp(inputHistoryIndex + direction, 0, inputHistory.Count);
            if (inputHistoryIndex >= inputHistory.Count)
            {
                inputText = string.Empty;
            }
            else
            {
                inputText = inputHistory[inputHistoryIndex];
            }

            lastInput = inputText;
            refreshHintsRequested = true;
        }

        private void HandleConsoleEntryLogged(string message)
        {
            autoScrollRequested = true;
        }

        private float CalculateHistoryContentHeight(float width)
        {
            if (console == null)
                return 0f;

            IReadOnlyList<string> history = console.History;
            float totalHeight = 0f;
            for (int i = 0; i < history.Count; i++)
                totalHeight += historyLabelStyle.CalcHeight(new GUIContent(history[i]), width) + historyLabelStyle.margin.vertical;

            return totalHeight;
        }

        private float CalculateHintsContentHeight(float width)
        {
            float totalHeight = 0f;
            for (int i = 0; i < closestEntries.Count; i++)
            {
                GUIStyle labelStyle = i == selectedHintIndex ? selectedHintLabelStyle : hintLabelStyle;
                string prefix = i == selectedHintIndex ? "› " : "  ";
                totalHeight += labelStyle.CalcHeight(new GUIContent(prefix + closestEntries[i]), width) + labelStyle.margin.vertical;
            }

            return totalHeight;
        }

        private void UnsubscribeFromConsole()
        {
            if (subscribedConsole == null)
                return;

            subscribedConsole.EntryLogged -= HandleConsoleEntryLogged;
            subscribedConsole = null;
        }

        private static float EaseOutCubic(float value)
        {
            float inverse = 1f - value;
            return 1f - inverse * inverse * inverse;
        }

        private static Texture2D CreateTexture(Color color)
        {
            Texture2D texture = new(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }

        private static void DestroyTexture(Texture2D texture)
        {
            if (texture != null)
                Destroy(texture);
        }

    }
}

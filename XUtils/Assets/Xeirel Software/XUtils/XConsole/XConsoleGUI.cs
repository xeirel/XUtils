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
        [SerializeField] private XConsole console;
        [SerializeField] private bool visible = true;
        [SerializeField] private float height = 320f;
        [SerializeField] private int maxVisibleLines = 18;

        private string inputText = "";
        private Vector2 scrollPosition;
        private bool submitRequested;

        public Dictionary<string, Action<string[]>> commands = new();

        private void Awake()
        {
            TryBindConsole();
        }

        private void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.backquoteKey.wasPressedThisFrame)
                    visible = !visible;

                if (visible && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
                    submitRequested = true;
            }

            if (console == null)
                TryBindConsole();

            if (submitRequested)
            {
                submitRequested = false;
                Submit();
            }
        }

        private void OnGUI()
        {
            if (!visible)
                return;

            DrawInputGUI();
            DrawHints();
        }

        private void DrawInputGUI()
        {
            float width = Screen.width - 40f;
            float top = Screen.height - height - 20f;
            GUILayout.BeginArea(new Rect(20f, top, width, height), GUI.skin.box);

            DrawHistory();

            GUILayout.BeginHorizontal();
            inputText = GUILayout.TextField(inputText);
            bool submit = GUILayout.Button("Enter", GUILayout.Width(60f));
            GUILayout.EndHorizontal();

            if (submit)
                submitRequested = true;

            GUILayout.EndArea();
        }

        private void DrawHistory()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

            if (console == null)
            {
                GUILayout.Label("No XConsole instance found.");
            }
            else
            {
                IReadOnlyList<string> history = console.History;
                int startIndex = Mathf.Max(0, history.Count - maxVisibleLines);
                for (int i = startIndex; i < history.Count; i++)
                    GUILayout.Label(history[i]);
            }

            GUILayout.EndScrollView();
            scrollPosition.y = float.MaxValue;
        }

        private void DrawHints()
        {
            // hints based on inputText and available commands and history in the console

            if (string.IsNullOrEmpty(inputText))
                return;

            IReadOnlyList<string> hints = XConsoleRegistry.CommandNames.FuzzyMatch(i => i, inputText, 5).Select(x => x.item).ToList();

            if (hints.Count == 0)
                return;

            float width = Screen.width - 40f;
            GUILayout.BeginArea(new Rect(20f, Screen.height - 60f - hints.Count * 22f, width, (hints.Count + 1) * 22f), GUI.skin.box);
            foreach (string hint in hints)
                GUILayout.Label(hint);
            GUILayout.EndArea();

        }

        private void Submit()
        {
            if (console == null || string.IsNullOrWhiteSpace(inputText))
                return;

            console.Execute(inputText);
            inputText = string.Empty;
        }

        private void TryBindConsole()
        {
            if (console != null)
                return;

            console = XConsole.Instance;
            if (console == null)
                console = FindFirstObjectByType<XConsole>(FindObjectsInactive.Include);
        }

    }
}

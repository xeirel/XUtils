using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XUtils.Console;

namespace XUtils
{
    [DefaultExecutionOrder(-1000)]
    public class XConsole : MonoBehaviour
    {
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool refreshOnEnable = true;
        [SerializeField] private bool refreshOnSceneLoaded = true;
        [SerializeField] private int maxHistoryCount = 128;

        private readonly List<string> history = new();

        public static XConsole Instance { get; private set; }
        public IReadOnlyList<string> History => history;
        public event Action<string> EntryLogged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            if (Instance != this)
                return;

            if (refreshOnSceneLoaded)
                SceneManager.sceneLoaded += HandleSceneLoaded;

            if (refreshOnEnable)
                RefreshRegistry(false);
        }

        private void OnDisable()
        {
            if (Instance != this)
                return;

            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void RefreshRegistry(bool logResult = true)
        {
            XConsoleRegistry.Clear();

            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null)
                    continue;

                XConsoleRegistry.Register(behaviour);
            }

            if (logResult)
                WriteLine($"Registry refreshed. {XConsoleRegistry.CommandCount} commands, {XConsoleRegistry.VariableCount} variables.");
        }

        public void RegisterTarget(object target)
        {
            XConsoleRegistry.Register(target);
        }

        public void UnregisterTarget(object target)
        {
            XConsoleRegistry.Unregister(target);
        }

        public bool Execute(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            string trimmedInput = input.Trim();
            WriteLine($"> {trimmedInput}");

            bool success = XConsoleRegistry.TryExecute(trimmedInput, out string result);
            if (!string.IsNullOrWhiteSpace(result))
                WriteLine(result);

            return success;
        }

        public void ClearHistory()
        {
            history.Clear();
        }

        public void WriteLine(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            history.Add(message);

            while (history.Count > maxHistoryCount)
                history.RemoveAt(0);

            EntryLogged?.Invoke(message);
        }

        [DevCommand("help", Description = "Lists registered commands and variables.")]
        private string Help(string filter = null)
        {
            IReadOnlyList<string> entries = XConsoleRegistry.DescribeEntries(filter);
            if (entries.Count == 0)
                return string.IsNullOrWhiteSpace(filter) ? "No commands or variables registered." : $"No entries found for '{filter}'.";

            return string.Join(Environment.NewLine, entries);
        }

        [DevCommand("refresh", Description = "Refreshes the command and variable registry.")]
        private string Refresh()
        {
            RefreshRegistry(false);
            return $"Registry refreshed. {XConsoleRegistry.CommandCount} commands, {XConsoleRegistry.VariableCount} variables.";
        }

        [DevCommand("clear", Description = "Clears console history.")]
        private string Clear()
        {
            ClearHistory();
            return "Console history cleared.";
        }

        [DevVariable("console.historyCount", Description = "Current console history count.")]
        private int HistoryCount => history.Count;

        private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            RefreshRegistry(false);
        }
    }
}

using UnityEngine;

namespace XUtils
{
    public static class XUtilsBase
    {
        const string LOG_TAG = "<color=green>[XEU]</color> ";
        public static void Log(object message) => Debug.Log(LOG_TAG + message);
        public static void LogWarning(object message) => Debug.LogWarning(LOG_TAG + message);
        public static void LogError(object message) => Debug.LogError(LOG_TAG + message);
        public static void ShowEditorNotification(object message)
        {
            var focusedWindow = UnityEditor.EditorWindow.focusedWindow;
            if (focusedWindow != null)
                focusedWindow.ShowNotification(new GUIContent(message.ToString()));
        }
    }
}

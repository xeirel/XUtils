using UnityEngine;
using UnityEditor;
using Unity.Mathematics;

namespace XUtils.UI
{
    public static class XEditorUI
    {
        // ----------------------------------
        // BASIC LAYOUT HELPERS
        // ----------------------------------

        public static void Header(string text)
        {
            GUILayout.Space(6);

            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14
            };

            GUILayout.Label(text, style);
            GUILayout.Space(4);
        }

        public static void SectionBegin()
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Space(4);
        }

        public static void SectionEnd()
        {
            GUILayout.Space(4);
            EditorGUILayout.EndVertical();
            GUILayout.Space(4);
        }

        public static void Divider(float thickness = 1f, float padding = 6f)
        {
            GUILayout.Space(padding);

            Rect r = EditorGUILayout.GetControlRect(false, thickness);
            EditorGUI.DrawRect(r, new Color(0.10f, 0.10f, 0.10f));

            GUILayout.Space(padding);
        }

        public static bool Button(string label)
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(10, 10, 4, 4)
            };

            return GUILayout.Button(label, style);
        }

        public static void ScrollViewBegin(ref Vector2 scrollPosition)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        }
        public static void ScrollViewEnd()
        {
            EditorGUILayout.EndScrollView();
        }


        // ----------------------------------
        // INPUT ELEMENTS
        // ----------------------------------

        public static string TextField(string label, string value)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label(label, EditorStyles.label);
            string result = EditorGUILayout.TextField(value);
            EditorGUILayout.EndVertical();
            return result;
        }

        public static bool Toggle(string label, bool value)
        {
            return EditorGUILayout.Toggle(label, value);
        }

        public static int IntField(string label, int value)
        {
            return EditorGUILayout.IntField(label, value);
        }

        public static float FloatField(string label, float value)
        {
            return EditorGUILayout.FloatField(label, value);
        }

        public static int2 ResolutionField(string label, int2 resolution)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label(label, EditorStyles.label);
            EditorGUILayout.BeginHorizontal();
            int width = EditorGUILayout.IntField("Width", resolution.x);
            int height = EditorGUILayout.IntField("Height", resolution.y);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return new int2(width, height);
        }


        // ----------------------------------
        // SLIDERS
        // ----------------------------------

        public static float Slider(string label, float value, float min, float max)
        {
            return EditorGUILayout.Slider(label, value, min, max);
        }

        public static int IntSlider(string label, int value, int min, int max)
        {
            return EditorGUILayout.IntSlider(label, value, min, max);
        }


        // ----------------------------------
        // PATH / FILE INPUTS
        // ----------------------------------

        public static string FolderField(string label, string path, string panelTitle = "Select Folder")
        {
            EditorGUILayout.BeginHorizontal();
            path = EditorGUILayout.TextField(label, path);

            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string startPath = string.IsNullOrEmpty(path) ? Application.dataPath : path;
                string selected = EditorUtility.OpenFolderPanel(panelTitle, startPath, "");
                if (!string.IsNullOrEmpty(selected))
                    path = selected;
            }

            EditorGUILayout.EndHorizontal();
            return path;
        }

        public static string FileField(string label, string path, string extension = "*", string panelTitle = "Select File")
        {
            EditorGUILayout.BeginHorizontal();
            path = EditorGUILayout.TextField(label, path);

            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string directory = string.IsNullOrEmpty(path) ? Application.dataPath : System.IO.Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(directory))
                    directory = Application.dataPath;

                string file = System.IO.Path.GetFileName(path);
                string selected = EditorUtility.OpenFilePanel(panelTitle, directory, extension);
                if (!string.IsNullOrEmpty(selected))
                    path = selected;
            }

            EditorGUILayout.EndHorizontal();
            return path;
        }


        // ----------------------------------
        // FLEX ROW (yan yana element düzeni)
        // ----------------------------------

        public static void Horizontal(System.Action content, float spacing = 6f)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(spacing);
            content?.Invoke();
            GUILayout.Space(spacing);
            GUILayout.EndHorizontal();
        }


        // ----------------------------------
        // DROPDOWNS / ENUMS
        // ----------------------------------

        public static int Popup(string label, int selectedIndex, string[] options)
        {
            return EditorGUILayout.Popup(label, selectedIndex, options);
        }

        public static T EnumPopup<T>(string label, T selected) where T : System.Enum
        {
            return (T)EditorGUILayout.EnumPopup(label, selected);
        }


        // ----------------------------------
        // SPACING HELPERS
        // ----------------------------------

        public static void Space(float px = 8f)
        {
            GUILayout.Space(px);
        }
    }
}

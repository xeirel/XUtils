using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using XUtils.UIUtils;

namespace XUtils.ScreenshotUtils
{
    public class XScreenshotWindow : EditorWindow
    {
        private string _outputFolder = Application.dataPath;
        private string _fileName = "screenshot";
        private int2 _resolution = new(1920, 1080);
        private bool _transparentBackground;
        private Vector2 _scrollPosition;

        [MenuItem("Tools/X Utils/XScreenshot Tool")]
        private static void ShowWindow()
        {
            var window = GetWindow<XScreenshotWindow>();
            window.titleContent = new GUIContent("XScreenshot Tool");
            window.minSize = new Vector2(320f, 200f);
        }

        private float _quality = 85f;
        private XScreenshotUtils.ScreenshotFormat _format;

        private void OnEnable() => Selection.selectionChanged += Repaint;
        private void OnDisable() => Selection.selectionChanged -= Repaint;

        private void OnGUI()
        {
            XEditorUI.ScrollViewBegin(ref _scrollPosition);
            XEditorUI.SectionBegin();
            XEditorUI.Header("XScreenshot Tool");
            XEditorUI.Divider();

            Camera[] selectedCameras = Selection.GetFiltered<Camera>(SelectionMode.ExcludePrefab | SelectionMode.Editable | SelectionMode.Deep);

            if (selectedCameras.Length == 0)
            {
                XEditorUI.Space(4);
                EditorGUILayout.HelpBox("No Camera selected in the Hierarchy. Please select at least one Camera to take screenshots from.", MessageType.Warning);
                XEditorUI.Space(4);
            }
            else
            {
                XEditorUI.Space(4);
                EditorGUILayout.HelpBox($"{selectedCameras.Length} Camera(s) selected. Screenshots will be taken from each selected Camera.\nSelected Cameras: {string.Join(", ", selectedCameras.Select(i => i.name))}", MessageType.Info);
                XEditorUI.Space(4);
            }

            _outputFolder = XEditorUI.FolderField("Output Folder", _outputFolder);
            _fileName = XEditorUI.TextField("File Name", _fileName);
            _resolution = XEditorUI.ResolutionField("Resolution", _resolution);

            XEditorUI.Divider();
            _format = XEditorUI.EnumPopup("Format ", _format);

            if (_format == XScreenshotUtils.ScreenshotFormat.JPG)
                _quality = XEditorUI.Slider("JPG Quality", _quality, 0f, 100f);

            if (_format == XScreenshotUtils.ScreenshotFormat.PNG)
            {
                _transparentBackground = XEditorUI.Toggle("Transparent Background", _transparentBackground);
                EditorGUILayout.HelpBox("Post Processing should be disabled on the cameras and Background Type should be Uninitialized or Solid Color to get transparent texture!", MessageType.Info);
            }


            if (selectedCameras.Length > 0)
            {
                XEditorUI.Divider();
                if (XEditorUI.Button("Take Screenshot"))
                    foreach (Camera cam in selectedCameras)
                        cam.CaptureAndSaveScreenshot(Path.Combine(_outputFolder, $"{_fileName}_{cam.name}_{Mathf.Abs(cam.GetInstanceID())}"), _resolution.x, _resolution.y, _transparentBackground, _format, (int)_quality);
            }

            XEditorUI.Divider();
            XEditorUI.SectionEnd();
            XEditorUI.ScrollViewEnd();
        }

    }
}

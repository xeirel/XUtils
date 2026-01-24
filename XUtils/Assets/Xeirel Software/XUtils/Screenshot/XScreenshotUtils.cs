using UnityEditor;
using UnityEngine;

namespace XUtils.ScreenshotUtils
{
    public static class XScreenshotUtils
    {
        public enum ScreenshotFormat
        {
            PNG,
            JPG,
            EXR
        }
        public static Texture2D CaptureScreenshot(this Camera _camera, int width = 1920, int height = 1080, bool transparentBackground = false)
        {
            RenderTexture rt = new RenderTexture(width, height, 24);
            _camera.targetTexture = rt;
            TextureFormat format = transparentBackground ? TextureFormat.RGBA32 : TextureFormat.RGB24;
            Texture2D screenshot = new Texture2D(width, height, format, false);
            Color clearColor = transparentBackground ? new Color(0, 0, 0, 0) : Color.black;
            GL.Clear(true, true, clearColor);
            _camera.Render();
            RenderTexture.active = rt;
            screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenshot.Apply();
            _camera.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
            return screenshot;
        }
        public static byte[] EncodeScreenshot(this Texture2D screenshot, ScreenshotFormat format, int jpgQuality = 75)
        {
            return format switch
            {
                ScreenshotFormat.PNG => screenshot.EncodeToPNG(),
                ScreenshotFormat.JPG => screenshot.EncodeToJPG(jpgQuality),
                ScreenshotFormat.EXR => screenshot.EncodeToEXR(),
                _ => screenshot.EncodeToPNG(),
            };
        }
        public static void SaveScreenshot(this byte[] imageData, string filePath, string extension)
        {
            System.IO.File.WriteAllBytes(filePath + extension, imageData);
            XUtilsBase.Log($"Screenshot saved to: {filePath}");
        }
        public static void CaptureAndSaveScreenshot(this Camera camera, string filePath, int width = 1920, int height = 1080, bool transparentBackground = false, ScreenshotFormat format = ScreenshotFormat.PNG, int jpgQuality = 75)
        {
            Texture2D screenshot = camera.CaptureScreenshot(width, height, transparentBackground);
            byte[] imageData = screenshot.EncodeScreenshot(format, jpgQuality);
            string extension = format switch
            {
                ScreenshotFormat.PNG => ".png",
                ScreenshotFormat.JPG => ".jpg",
                ScreenshotFormat.EXR => ".exr",
                _ => ".png",
            };
            imageData.SaveScreenshot(filePath, extension);
            Object.DestroyImmediate(screenshot);
        }
#if UNITY_EDITOR
        [MenuItem("CONTEXT/Camera/[XEU] Capture Screenshot", priority = 0)]
        private static void CaptureScreenshotMenuItem(MenuCommand command)
        {
            Camera camera = (Camera)command.context;
            string filePath = EditorUtility.SaveFilePanel("Save Screenshot", "", $"{Application.productName}_{Random.Range(0, int.MaxValue)}_{System.DateTime.UtcNow.ToFileTimeUtc()}.png", "png");
            if (!string.IsNullOrEmpty(filePath))
            {
                camera.CaptureAndSaveScreenshot(filePath);
                EditorUtility.RevealInFinder(filePath);
                XUtilsBase.ShowEditorNotification("Screenshot captured and saved.");
            }
        }
#endif
    }
}
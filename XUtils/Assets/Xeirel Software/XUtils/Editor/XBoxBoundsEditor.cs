using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace XUtils.UnityUtils
{
    [CustomEditor(typeof(XBoxBounds)), CanEditMultipleObjects]
    public class XBoxBoundsEditor : Editor
    {
        private BoxBoundsHandle m_BoundsHandle;

        private void OnEnable()
        {
            m_BoundsHandle = new BoxBoundsHandle();
        }

        private void OnSceneGUI()
        {
            var iBounds = (XBoxBounds)target;

            using (new Handles.DrawingScope(iBounds.transform.localToWorldMatrix))
            {
                m_BoundsHandle.center = iBounds.LocalBounds.center;
                m_BoundsHandle.size = iBounds.LocalBounds.size;

                EditorGUI.BeginChangeCheck();
                m_BoundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Vector3 center = m_BoundsHandle.center,
                    size = m_BoundsHandle.size;

                    Undo.RecordObject(target, "Bounds");

                    iBounds.LocalBounds.center = center;
                    iBounds.LocalBounds.size = size;

                    EditorUtility.SetDirty(target);
                }
            }
        }
    }
}
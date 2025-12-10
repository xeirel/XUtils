using Unity.Mathematics;
using UnityEngine;

namespace XUtils.Math
{
    public static class XMath
    {
        public static float Lerp(float current, float target, float t, float snapDistance = 0.0001f)
        {
            float value = Mathf.Lerp(current, target, t);
            float diff = target - value;

            if (diff <= snapDistance && diff >= -snapDistance)
                return target;

            return value;
        }
        public static Vector3 Lerp(Vector3 current, Vector3 target, float t, float snapDistance = 0.0001f)
        {
            Vector3 value = Vector3.Lerp(current, target, t);
            Vector3 diff = target - value;
            if (diff.sqrMagnitude <= snapDistance * snapDistance)
                return target;
            return value;
        }
        public static Quaternion Lerp(Quaternion current, Quaternion target, float t, float snapDistance = 0.0001f)
        {
            Quaternion value = Quaternion.Slerp(current, target, t);
            float angle = Quaternion.Angle(value, target);
            if (angle <= snapDistance)
                return target;
            return value;
        }
        public static Vector2 Lerp(Vector2 current, Vector2 target, float t, float snapDistance = 0.0001f)
        {
            Vector2 value = Vector2.Lerp(current, target, t);
            Vector2 diff = target - value;
            if (diff.sqrMagnitude <= snapDistance * snapDistance)
                return target;
            return value;
        }


        public static Quaternion ToEuler(this Vector3 v3)
        {
            return quaternion.Euler(v3.x, v3.y, v3.z);
        }
        public static Vector3 FromEuler(this Quaternion q)
        {
            return q.eulerAngles;
        }
    }
}
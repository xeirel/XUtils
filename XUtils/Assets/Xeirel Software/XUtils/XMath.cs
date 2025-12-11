using Unity.Mathematics;
using UnityEngine;

namespace XUtils.MathUtils
{
    public static class XMath
    {
        private static System.Random _rand = new();
        public static System.Random Random => _rand ?? (_rand = new());

        #region Index helpers
        public static int2 To2D(this int val, int2 size)
            => new int2(val % size.x, val / size.x);
        public static int3 To3D(this int val, int3 size)
            => new int3(val % size.x, (val / size.x) % size.y, val / (size.x * size.y));
        public static int4 To4D(this int val, int4 size)
        {
            int w = val / (size.x * size.y * size.z);
            val %= (size.x * size.y * size.z);

            int z = val / (size.x * size.y);
            val %= (size.x * size.y);

            int y = val / size.x;
            int x = val % size.x;

            return new int4(x, y, z, w);
        }
        public static int To1D(this int2 val, int2 size)
            => val.x + val.y * size.x;
        public static int To1D(this int3 val, int3 size)
            => val.x + val.y * size.x + val.z * (size.x * size.y);
        public static int To1D(this int4 val, int4 size)
            => val.x + val.y * size.x + val.z * (size.x * size.y) + val.w * (size.x * size.y * size.z);
        #endregion

        #region Lerp helpers
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
        public static float Lerp(this float val, float to, float time) => Mathf.Lerp(val, to, time);
        public static float LerpTo(this ref float val, float to, float time, int? r = null)
        {
            float v = Mathf.Lerp(val, to, time);
            if (r.HasValue)
                v = v.R(r.Value);

            return val = v;
        }
        public static Color Lerp(this Color val, Color to, float time) => Color.Lerp(val, to, time);
        public static Color LerpTo(this ref Color val, Color to, float time) => val = Color.Lerp(val, to, time);
        public static Vector3 Lerp(this Vector3 val, Vector3 to, float time) => Vector3.Lerp(val, to, time);
        public static Vector2 Lerp(this Vector2 val, Vector2 to, float time) => Vector2.Lerp(val, to, time);
        public static Vector4 Lerp(this Vector4 val, Vector4 to, float time) => Vector4.Lerp(val, to, time);
        public static Quaternion Lerp(this Quaternion val, Quaternion to, float time) => Quaternion.Lerp(val, to, time);
        public static Vector3 Lerp3(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            return t < 0f ? Vector3.LerpUnclamped(a, b, t + 1f) : Vector3.LerpUnclamped(b, c, t);
        }
        public static Vector3 Lerp3Clamped(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            t = Mathf.Clamp(t, -1f, 1f);
            return Lerp3(a, b, c, t);
        }

        #endregion

        #region Float / numeric helpers
        public static float R(this float val, int r) => Mathf.Round(val * r) / r;
        public static float Abs(this float val) => Mathf.Abs(val);
        public static float Sin(this float val) => Mathf.Sin(val);
        public static float Cos(this float val) => Mathf.Cos(val);
        public static float Tan(this float val) => Mathf.Tan(val);
        public static float NotNaN(this float val) => !float.IsNaN(val) ? val : 0f;
        public static float Pow(this float a, float b) => Mathf.Pow(a, b);
        public static float Distance(this float a, float b) => Mathf.Max(a, b) - Mathf.Min(a, b);
        public static int R(this int val, int r) => (int)(Mathf.Round(val * r) / r);
        public static int Abs(this int val) => Mathf.Abs(val);
        public static int Pow(this int a, int b) => (int)Mathf.Pow(a, b);
        public static int Distance(this int a, int b) => Mathf.Max(a, b) - Mathf.Min(a, b);
        public static float Clamp(this float val, float min = 0f, float max = 1f) => System.Math.Clamp(val, min, max);
        public static float Min(this float val, float other) => System.Math.Min(val, other);
        public static float Max(this float val, float other) => System.Math.Max(val, other);
        public static int Clamp(this int val, int min, int max) => System.Math.Clamp(val, min, max);
        public static int Min(this int val, int other) => System.Math.Min(val, other);
        public static int Max(this int val, int other) => System.Math.Max(val, other);
        public static double Clamp(this double val, double min, double max) => System.Math.Clamp(val, min, max);
        public static long Clamp(this long val, long min, long max) => System.Math.Clamp(val, min, max);
        public static double NotNaN(this double val) => !double.IsNaN(val) ? val : 0d;

        public static float Remap(this float valueIn, float baseMin, float baseMax, float limitMin, float limitMax)
            => ((limitMax - limitMin) * (valueIn - baseMin) / (baseMax - baseMin)) + limitMin;

        public static double Remap(this double valueIn, double baseMin, double baseMax, double limitMin, double limitMax)
            => ((limitMax - limitMin) * (valueIn - baseMin) / (baseMax - baseMin)) + limitMin;
        #endregion

        #region Vector helpers
        public static Vector2Int ToInt(this Vector2 vec, bool round = false)
        {
            return new Vector2Int(
                round ? Mathf.RoundToInt(vec.x) : (int)vec.x,
                round ? Mathf.RoundToInt(vec.y) : (int)vec.y);
        }

        public static Vector3Int ToInt(this Vector3 vec, bool round = false)
        {
            return new Vector3Int(
                round ? Mathf.RoundToInt(vec.x) : (int)vec.x,
                round ? Mathf.RoundToInt(vec.y) : (int)vec.y,
                round ? Mathf.RoundToInt(vec.z) : (int)vec.z);
        }
        public static Vector2 ToFloat(this Vector2Int vec) => new Vector2(vec.x, vec.y);
        public static Vector3 ToFloat(this Vector3Int vec) => new Vector3(vec.x, vec.y, vec.z);
        public static Vector3 Multiply(this Vector3 a, Vector3 b) => Vector3.Scale(a, b);
        public static Vector2 Multiply(this Vector2 a, Vector2 b) => Vector2.Scale(a, b);
        public static float Distance(this Vector3 a, Vector3 b) => Vector3.Distance(a, b);
        public static float Distance(this Vector2 a, Vector2 b) => Vector2.Distance(a, b);
        public static float DistanceEps(this Vector3 a, Vector3 b, float eps = 0.001f)
        {
            float d = Vector3.Distance(a, b);
            return d < eps ? 0f : d;
        }
        public static float DistanceEps(this Vector2 a, Vector2 b, float eps = 0.001f)
        {
            float d = Vector2.Distance(a, b);
            return d < eps ? 0f : d;
        }
        public static float DistanceR(this Vector3 a, Vector3 b, int r = 1000)
        {
            return Mathf.Round(Vector3.Distance(a, b) * r) / r;
        }
        public static float Dot(this Vector3 a, Vector3 b) => Vector3.Dot(a, b);
        public static float Dot(this Vector2 a, Vector2 b) => Vector2.Dot(a, b);
        public static float Angle(this Vector3 a, Vector3 b) => Vector3.Angle(a, b);
        public static float Angle(this Vector2 a, Vector2 b) => Vector2.Angle(a, b);
        public static Vector3 R(this Vector3 val, int r)
        {
            return new Vector3(val.x.R(r), val.y.R(r), val.z.R(r));
        }
        public static Vector3 Abs(this Vector3 val)
        {
            return new Vector3(val.x.Abs(), val.y.Abs(), val.z.Abs());
        }
        public static Vector3 Pow(this Vector3 val, float p)
        {
            return new Vector3(val.x.Pow(p), val.y.Pow(p), val.z.Pow(p));
        }
        public static Vector2 R(this Vector2 val, int r)
        {
            return new Vector2(val.x.R(r), val.y.R(r));
        }
        public static Vector2 Abs(this Vector2 val)
        {
            return new Vector2(val.x.Abs(), val.y.Abs());
        }
        public static Vector2 Pow(this Vector2 val, float p)
        {
            return new Vector2(val.x.Pow(p), val.y.Pow(p));
        }
        public static Vector2 XY(this Vector3 vec) => new Vector2(vec.x, vec.y);
        public static Vector2 YZ(this Vector3 vec) => new Vector2(vec.y, vec.z);
        public static Vector2 XZ(this Vector3 vec) => new Vector2(vec.x, vec.z);
        public static Vector2 XY(this Vector4 vec) => new Vector2(vec.x, vec.y);
        public static Vector2 YZ(this Vector4 vec) => new Vector2(vec.y, vec.z);
        public static Vector2 XZ(this Vector4 vec) => new Vector2(vec.x, vec.z);
        public static Vector3 XYz(this Vector2 vec, float z) => new Vector3(vec.x, vec.y, z);
        public static Vector3 xXY(this Vector2 vec, float x) => new Vector3(x, vec.x, vec.y);
        public static Vector3 XyY(this Vector2 vec, float y) => new Vector3(vec.x, y, vec.y);
        public static Vector3 XYZ(this Vector4 vec) => new Vector3(vec.x, vec.y, vec.z);
        public static Vector3 YZX(this Vector4 vec) => new Vector3(vec.y, vec.z, vec.x);
        public static Vector3 XZY(this Vector4 vec) => new Vector3(vec.x, vec.z, vec.y);
        public static Vector2Int XY(this Vector3Int vec) => new Vector2Int(vec.x, vec.y);
        public static Vector2Int YZ(this Vector3Int vec) => new Vector2Int(vec.y, vec.z);
        public static Vector2Int XZ(this Vector3Int vec) => new Vector2Int(vec.x, vec.z);
        public static Quaternion Inverse(this Quaternion qua) => Quaternion.Inverse(qua);
        #endregion
        public static Quaternion ToEuler(this Vector3 v3)
            => quaternion.Euler(v3.x, v3.y, v3.z);
        /// <summary>
        /// Transforms the specified position vector by the given matrix and returns the resulting position.
        /// </summary>
        /// <param name="mat">The matrix used to transform the position vector.</param>
        /// <param name="pos">The position vector to be transformed.</param>
        /// <returns>A <see cref="Vector3"/> representing the transformed position.</returns>
        public static Vector3 Calc(this Matrix4x4 mat, Vector3 pos) => (mat * Matrix4x4.Translate(pos)).GetPosition();
        /// <summary>
        /// Transforms the specified local-space bounds to world-space coordinates using the provided transform.
        /// </summary>
        /// <param name="localBounds">The bounds defined in the local space to be transformed.</param>
        /// <param name="transform">The transform whose local-to-world matrix is used to convert the bounds to world space. Cannot be null.</param>
        /// <returns>A new Bounds instance representing the original bounds in world-space coordinates.</returns>
        public static Bounds TransformBoundsToWorld(this Bounds localBounds, Transform transform) => localBounds.TransformMatrix(transform.localToWorldMatrix);
        /// <summary>
        /// Transforms the specified local-space bounding box by the given matrix and returns the resulting axis-aligned
        /// bounding box in world space.
        /// </summary>
        /// <remarks>This method calculates the world-space bounds by transforming all eight corners of the local bounding
        /// box and encapsulating them. The resulting bounds are axis-aligned in world space and may be larger than the original
        /// bounds if the transformation includes rotation or scaling.</remarks>
        /// <param name="localBounds">The bounding box defined in local space to be transformed.</param>
        /// <param name="transform">The transformation matrix to apply to the local bounding box.</param>
        /// <returns>A new Bounds instance representing the axis-aligned bounding box in world space after applying the transformation.</returns>
        public static Bounds TransformMatrix(this Bounds localBounds, Matrix4x4 transform)
        {
            Bounds worldBounds = new Bounds(transform.Calc(localBounds.center), Vector3.zero);

            Vector3[] corners = new Vector3[8];
            Vector3 extents = localBounds.extents;

            corners[0] = new Vector3(extents.x, extents.y, extents.z);
            corners[1] = new Vector3(extents.x, extents.y, -extents.z);
            corners[2] = new Vector3(extents.x, -extents.y, extents.z);
            corners[3] = new Vector3(extents.x, -extents.y, -extents.z);
            corners[4] = new Vector3(-extents.x, extents.y, extents.z);
            corners[5] = new Vector3(-extents.x, extents.y, -extents.z);
            corners[6] = new Vector3(-extents.x, -extents.y, extents.z);
            corners[7] = new Vector3(-extents.x, -extents.y, -extents.z);

            for (int i = 0; i < 8; i++)
            {
                Vector3 worldCorner = transform.Calc(corners[i] + localBounds.center);
                worldBounds.Encapsulate(worldCorner);
            }

            return worldBounds;
        }

        /// <summary>
        /// Generates a random single-precision floating-point number within the specified range.
        /// </summary>
        /// <remarks>This method extends <see cref="System.Random"/> to support generating random float
        /// values within a specified range. The distribution is uniform across the interval.</remarks>
        /// <param name="rand">The random number generator used to produce the value. Cannot be null.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. Must be greater than <paramref name="minValue"/>.</param>
        /// <returns>A random float greater than or equal to <paramref name="minValue"/> and less than <paramref
        /// name="maxValue"/>.</returns>
        public static float NextFloat(this System.Random rand, float minValue, float maxValue)
        {
            return (float)(rand.NextDouble() * (maxValue - minValue) + minValue);
        }
        /// <summary>
        /// Returns a random position within the specified bounding box.
        /// </summary>
        /// <param name="bounds">The bounds within which to generate the random position. The position will be inside the minimum and maximum
        /// coordinates of the bounds.</param>
        /// <returns>A <see cref="Vector3"/> representing a randomly selected position inside the given bounds.</returns>
        public static Vector3 GetRandomPosition(this Bounds bounds)
        {
            return new Vector3(
                Random.NextFloat(bounds.min.x, bounds.max.x),
                Random.NextFloat(bounds.min.y, bounds.max.y),
                Random.NextFloat(bounds.min.z, bounds.max.z));
        }

    }
}
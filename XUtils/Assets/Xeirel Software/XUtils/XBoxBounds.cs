using Unity.Mathematics;
using UnityEngine;
using XUtils.MathUtils;

namespace XUtils.UnityUtils
{
    public class XBoxBounds : MonoBehaviour
    {
        /// <summary>
        /// Local-space bounds relative to this transform.
        /// </summary>
        [Tooltip("Local-space bounds relative to this transform.")]
        public Bounds LocalBounds;

        /// <summary>
        /// Cached world-space bounds for this box. Updated when accessed.
        /// </summary>
        [HideInInspector]
        public Bounds WorldBoundsCache;

        /// <summary>
        /// World-space bounds calculated from <see cref="LocalBounds"/> and this transform.
        /// Accessing this property also refreshes <see cref="WorldBoundsCache"/>.
        /// </summary>
        public Bounds WorldBounds => WorldBoundsCache = LocalBounds.TransformBoundsToWorld(transform);

        private void Reset()
        {
            // Sensible default: unit cube centered slightly above the pivot
            LocalBounds = new Bounds(new Vector3(0f, 1f, 0f), new float3(1));
        }

        private void Start()
        {
            WorldBoundsCache = LocalBounds.TransformBoundsToWorld(transform);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = new Color(1, 0.5f, 0, 0.85f);
            Gizmos.DrawWireCube(LocalBounds.center, LocalBounds.size);
            Gizmos.color = new Color(1, 0.5f, 0, 0.10f);
            Gizmos.DrawCube(LocalBounds.center, LocalBounds.size);
        }
#endif
    }
}
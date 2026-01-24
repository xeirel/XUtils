using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XUtils.CollectionsUtils;

namespace XUtils.UnityUtils
{
    public static class XObject
    {
#nullable enable
        #region Component lookup

        public static bool TryGetComponentInChildren<T>(this GameObject gameObject, out T component, bool includeInactive = false)
            where T : Component
        {
            component = gameObject.GetComponentInChildren<T>(includeInactive);
            return component != null;
        }

        public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T component, bool includeInactive = false)
            where T : Component
        {
            component = gameObject.GetComponentInParent<T>(includeInactive);
            return component != null;
        }

        public static bool TryFindFirstObject<T>(out T? result, bool includeInactive = false)
            where T : Component
        {
            T component = MonoBehaviour.FindFirstObjectByType<T>(
                includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);

            if (component != null)
            {
                result = component;
                return true;
            }

            result = null;
            return false;
        }

        public static bool HasComponent<T>(this GameObject gameObject)
            where T : Component =>
            gameObject.GetComponent<T>() != null;

        public static bool HasComponent<T>(this Component component)
            where T : Component =>
            component.GetComponent<T>() != null;

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
                component = gameObject.AddComponent<T>();

            return component;
        }

        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            return component.gameObject.GetOrAddComponent<T>();
        }

        public static T? FindInChildren<T>(this GameObject gameObject, bool includeInactive = false) where T : Component
        {
            return gameObject.GetComponentInChildren<T>(includeInactive);
        }

        public static IEnumerable<T> FindAllInChildren<T>(this GameObject gameObject, bool includeInactive = false) where T : Component
        {
            return gameObject.GetComponentsInChildren<T>(includeInactive);
        }

        public static T? GetParentComponentExcludingSelf<T>(this Component current)
            where T : Component
        {
            Transform? currentParent = current.transform.parent;

            while (currentParent != null)
            {
                T component = currentParent.GetComponent<T>();
                if (component != null)
                    return component;

                currentParent = currentParent.parent;
            }

            return null;
        }

        public static T? GetParentComponent<T>(this Component current)
            where T : Component
        {
            Transform? currentParent = current.transform;

            while (currentParent != null)
            {
                T component = currentParent.GetComponent<T>();
                if (component != null)
                    return component;

                currentParent = currentParent.parent;
            }

            return null;
        }
        public static void ForEachComponents<T>(this GameObject obj, Action<T> action, bool includeChilds = false, bool includeInactive = true) where T : Component
        {
            foreach (var comp in includeChilds ? obj.GetComponentsInChildren<T>(includeInactive) : obj.GetComponents<T>())
                action(comp);
        }

        public static void ForEachComponents<T>(this Component comp, Action<T> action, bool includeChilds = false, bool includeInactive = true) where T : Component =>
            ForEachComponents<T>(comp.gameObject, action, includeChilds, includeInactive);

        public static void ForEachBehaviours<T0, T1>(this GameObject obj, Action<Behaviour> action, bool includeChilds = false, bool includeInactive = true) where T0 : Behaviour where T1 : Behaviour
        {
            foreach (var comp in includeChilds ? obj.GetComponentsInChildren<T0>(includeInactive) : obj.GetComponents<T0>())
                action(comp);

            foreach (var comp in includeChilds ? obj.GetComponentsInChildren<T1>(includeInactive) : obj.GetComponents<T1>())
                action(comp);
        }

        public static void ForEachBehaviours<T0, T1>(this Component comp, Action<Behaviour> action, bool includeChilds = false, bool includeInactive = true) where T0 : Behaviour where T1 : Behaviour
            => ForEachBehaviours<T0, T1>(comp.gameObject, action, includeChilds, includeInactive);

        public static void ForEachBehaviours<T0, T1, T2>(this GameObject obj, Action<Behaviour> action, bool includeChilds = false, bool includeInactive = true) where T0 : Behaviour where T1 : Behaviour where T2 : Behaviour
        {
            foreach (var comp in includeChilds ? obj.GetComponentsInChildren<T0>(includeInactive) : obj.GetComponents<T0>())
                action(comp);

            foreach (var comp in includeChilds ? obj.GetComponentsInChildren<T1>(includeInactive) : obj.GetComponents<T1>())
                action(comp);

            foreach (var comp in includeChilds ? obj.GetComponentsInChildren<T2>(includeInactive) : obj.GetComponents<T2>())
                action(comp);
        }

        public static void ForEachBehaviours<T0, T1, T2>(this Component comp, Action<Behaviour> action, bool includeChilds = false, bool includeInactive = true) where T0 : Behaviour where T1 : Behaviour where T2 : Behaviour
            => ForEachBehaviours<T0, T1, T2>(comp.gameObject, action, includeChilds, includeInactive);


        #endregion

        #region Hierarchy search

        public static Transform? GetFirstChildWithName(this Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;

                Transform? found = child.GetFirstChildWithName(name);
                if (found != null)
                    return found;
            }

            return null;
        }

        public static Transform? GetFirstChildWithNameEndsWith(this Transform parent, string nameEnd)
        {
            foreach (Transform child in parent)
            {
                if (child.name.EndsWith(nameEnd, StringComparison.Ordinal))
                    return child;

                Transform? found = child.GetFirstChildWithNameEndsWith(nameEnd);
                if (found != null)
                    return found;
            }

            return null;
        }

        public static GameObject? GetFirstChildWithName(this GameObject parent, string name)
        {
            if (parent == null)
                return null;

            foreach (Transform child in parent.transform)
            {
                if (child.gameObject.name == name)
                    return child.gameObject;
            }

            return null;
        }

        public static IEnumerable<Transform> EnumerateChildren(this Transform parent, bool includeSelf = false)
        {
            if (includeSelf)
                yield return parent;

            foreach (Transform child in parent)
                yield return child;
        }

        public static IEnumerable<Transform> EnumerateChildrenRecursive(this Transform parent, bool includeSelf = false)
        {
            if (includeSelf)
                yield return parent;

            foreach (Transform child in parent)
            {
                yield return child;

                foreach (Transform grandChild in child.EnumerateChildrenRecursive())
                    yield return grandChild;
            }
        }

        public static void SetParentKeepWorld(this Transform transform, Transform? newParent)
        {
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;
            Vector3 scale = transform.lossyScale;

            transform.SetParent(newParent, false);
            transform.position = pos;
            transform.rotation = rot;
            transform.localScale = scale;
        }

        #endregion

        #region Active helpers

        public static void SetActiveSafe(this GameObject? gameObject, bool active)
        {
            if (gameObject != null && gameObject.activeSelf != active)
                gameObject.SetActive(active);
        }

        public static void SetActiveSafe(this Component? component, bool active)
        {
            if (component != null)
                component.gameObject.SetActiveSafe(active);
        }

        #endregion

        #region Layer & Tag
        public static readonly string[] ExcludedLayerNames = new string[] { "TransparentFX", "Ignore Raycast", "Water", "UI" };

        public static LayerMask GetAllLayersExceptByName(string[]? extraLayer = null, string[]? ignoreExcludedLayer = null)
        {
            int layerMask = ~0;
            List<string> layers = ExcludedLayerNames.ToList();

            if (extraLayer != null && extraLayer.Length > 0)
                layers.AddRange(extraLayer);

            if (ignoreExcludedLayer != null)
                foreach (string ignore in ignoreExcludedLayer)
                    if (layers.Contains(ignore)) layers.Remove(ignore);

            foreach (string layerName in layers)
            {
                int layer = LayerMask.NameToLayer(layerName);
                if (layer != -1) layerMask &= ~(1 << layer);
            }

            return layerMask;
        }
        public static LayerMask GetLayersByName(string[]? layers = null)
        {
            int layerMask = 0;
            if (layers != null)
                foreach (string layerName in layers)
                {
                    int layer = LayerMask.NameToLayer(layerName);
                    if (layer != -1) layerMask |= (1 << layer);
                }

            return layerMask;
        }
        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;

            foreach (Transform child in gameObject.transform)
                child.gameObject.SetLayerRecursively(layer);
        }

        public static void SetTagRecursively(this GameObject gameObject, string tag)
        {
            gameObject.tag = tag;

            foreach (Transform child in gameObject.transform)
                child.gameObject.SetTagRecursively(tag);
        }

        #endregion

        #region Collision helpers

        public static bool GetCollisionIgnoreState(Collider colA, Collider colB)
        {
            if (!colA || !colB)
                return false;

            if (colA.isTrigger || colB.isTrigger)
                return false;

            if (colA.includeLayers.Contains(colB.gameObject.layer) ||
                colB.includeLayers.Contains(colA.gameObject.layer))
                return false;

            if (Physics.GetIgnoreLayerCollision(colA.gameObject.layer, colB.gameObject.layer))
                return true;

            if (colA.excludeLayers.Contains(colB.gameObject.layer) ||
                colB.excludeLayers.Contains(colA.gameObject.layer))
                return true;

            return false;
        }

        public static GameObject? GameObj(this RaycastHit hit) => hit.collider != null ? hit.collider.gameObject : null;

        #endregion

        #region Component removal

        public static bool RemoveComponent(this GameObject obj, Type type, bool immediate = false, int maxDepth = 16)
        {
            Component? mainComponent = obj.GetComponent(type);

            if (mainComponent == null || mainComponent is Transform)
                return true;

            Stack<Component> componentStack = new Stack<Component>();
            Component currentComponent = mainComponent;
            componentStack.Push(currentComponent);

            for (int i = 0; i < maxDepth; i++)
            {
                object[] attribs = currentComponent
                    .GetType()
                    .GetCustomAttributes(typeof(RequireComponent), true);

                bool hasAttribute = false;

                foreach (RequireComponent attr in attribs)
                {
                    if (attr.m_Type0 != null &&
                        obj.TryGetComponent(attr.m_Type0, out Component t0) &&
                        t0 is not Transform &&
                        !componentStack.Contains(t0))
                    {
                        componentStack.Push(currentComponent = t0);
                        hasAttribute = true;
                    }

                    if (attr.m_Type1 != null &&
                        obj.TryGetComponent(attr.m_Type1, out Component t1) &&
                        t1 is not Transform &&
                        !componentStack.Contains(t1))
                    {
                        componentStack.Push(currentComponent = t1);
                        hasAttribute = true;
                    }

                    if (attr.m_Type2 != null &&
                        obj.TryGetComponent(attr.m_Type2, out Component t2) &&
                        t2 is not Transform &&
                        !componentStack.Contains(t2))
                    {
                        componentStack.Push(currentComponent = t2);
                        hasAttribute = true;
                    }
                }

                if (!hasAttribute)
                    break;

                if (i == maxDepth - 1)
                    return false;
            }

            while (componentStack.TryPop(out Component comp))
            {
                if (!comp)
                    continue;

                if (immediate)
                    Component.DestroyImmediate(comp);
                else
                    Component.Destroy(comp);
            }

            return true;
        }

        public static bool RemoveComponent(this Component obj, Type type, bool immediate = false, int maxDepth = 16)
            => obj.gameObject.RemoveComponent(type, immediate, maxDepth);

        public static bool RemoveComponent<T>(this GameObject obj, bool immediate = false, int maxDepth = 16)
            where T : Component
            => obj.RemoveComponent(typeof(T), immediate, maxDepth);

        public static bool RemoveComponent<T>(this Component obj, bool immediate = false, int maxDepth = 16)
            where T : Component
            => obj.gameObject.RemoveComponent(typeof(T), immediate, maxDepth);

        #endregion

        #region Scene Management
        public static bool GetSceneNameWithIndex(int buildIndex, out string sceneBuildName) =>
    GetAvailableSceneNamesFromBuild().TryGetValue(buildIndex, out sceneBuildName);
        public static Dictionary<int, string> GetAvailableSceneNamesFromBuild()
        {
            Dictionary<int, string> scenes = new();
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; ++i)
            {
                string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                string currentSceneName = System.IO.Path.GetFileNameWithoutExtension(path);
                scenes.Add(i, currentSceneName);
            }
            return scenes;
        }
        #endregion
#nullable disable
    }
}
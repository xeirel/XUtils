using System.Collections.Generic;
using UnityEngine;
using XUtils.StringUtils;

namespace XUtils.UnityUtils
{
    public static class XResources
    {
        private static readonly Dictionary<string, Object> _cache = new();

        static XResources()
        {
            Application.quitting += Clear;
        }

        public static T Load<T>(string path) where T : Object
        {
            if (_cache.TryGetValue(path, out var cached))
            {
                if (XUtilsBase.DEBUG) XUtilsBase.Log($"Returning cached asset for '{path}'".Color(Color.cyan));
                return (T)cached;
            }

            T loaded = Resources.Load<T>(path);
            if (loaded != null)
            {
                _cache[path] = loaded;
                if (XUtilsBase.DEBUG) XUtilsBase.Log($"Loaded and cached asset '{path}'".Color(Color.yellow));
            }
            else if (XUtilsBase.DEBUG)
            {
                XUtilsBase.LogError($"Failed to load asset '{path}'".Color(Color.red));
            }

            return loaded;
        }

        public static bool TryLoad<T>(string path, out T asset) where T : Object
        {
            asset = Load<T>(path);
            return asset != null;
        }

        public static T LoadFirst<T>(string path) where T : Object
        {
            if (_cache.TryGetValue(path, out var cached))
                return (T)cached;

            var all = Resources.LoadAll<T>(path);
            if (all != null && all.Length > 0 && all[0] != null)
            {
                var first = all[0];
                _cache[path] = first;
                if (XUtilsBase.DEBUG) XUtilsBase.Log($"Loaded first asset '{path}' -> '{first.name}'".Color(Color.yellow));
                return first;
            }

            if (XUtilsBase.DEBUG) XUtilsBase.LogError($"Failed to load first asset from '{path}'".Color(Color.red));
            return null;
        }

        public static bool TryLoadFirst<T>(string path, out T asset) where T : Object
        {
            asset = LoadFirst<T>(path);
            return asset != null;
        }

        public static T LoadAtPath<T>(string folder, string name) where T : Object
        {
            if (string.IsNullOrEmpty(folder))
                return Load<T>(name);

            string path = $"{folder}/{name}";
            return Load<T>(path);
        }

        public static bool TryLoadAtPath<T>(string folder, string name, out T asset) where T : Object
        {
            asset = LoadAtPath<T>(folder, name);
            return asset != null;
        }

        public static IReadOnlyList<T> LoadAll<T>(string path) where T : Object
        {
            var loaded = Resources.LoadAll<T>(path);
            for (int i = 0; i < loaded.Length; i++)
            {
                var obj = loaded[i];
                if (obj != null)
                {
                    string key = $"{path}/{obj.name}";
                    _cache[key] = obj;
                    if (XUtilsBase.DEBUG) XUtilsBase.Log($"Loaded and cached asset '{key}'".Color(Color.yellow));
                }
            }

            return loaded;
        }

        public static bool Unload(string path)
        {
            if (!_cache.TryGetValue(path, out var obj))
                return false;

            _cache.Remove(path);

            if (obj != null)
                Resources.UnloadAsset(obj);

            if (XUtilsBase.DEBUG) XUtilsBase.Log($"Unloaded asset '{path}'".Color(Color.white));

            return true;
        }

        public static void UnloadAll()
        {
            foreach (var kvp in _cache)
            {
                if (kvp.Value != null)
                    Resources.UnloadAsset(kvp.Value);
            }

            if (XUtilsBase.DEBUG && _cache.Count > 0)
                XUtilsBase.Log($"Unloaded {_cache.Count} cached assets".Color(Color.white));

            _cache.Clear();
        }

        public static void Clear()
        {
            if (_cache.Count > 0 && XUtilsBase.DEBUG)
                XUtilsBase.Log($"Clearing {_cache.Count} cached asset references (no unload)".Color(Color.white));

            _cache.Clear();
        }
    }
}
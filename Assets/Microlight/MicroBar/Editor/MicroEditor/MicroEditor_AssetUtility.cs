using System.IO;
using UnityEditor;
using UnityEngine;

namespace Microlight.MicroEditor
{
    // ****************************************************************************************************
    // Utility functions for managing assets (search assets...)
    // ****************************************************************************************************
    internal static class MicroEditor_AssetUtility
    {
        /// <summary>
        ///     Returns GameObject from Assets folder with specified name
        ///     Can return more than one if there is prefab with same name
        ///     or has same name in it (Script and ScriptTwo) will return both
        /// </summary>
        /// <param name="prefabName">Name of the prefab</param>
        public static GameObject GetPrefab(string prefabName)
        {
            // Find prefab
            var guids = AssetDatabase.FindAssets("t:prefab " + prefabName);
            if (guids == null || guids.Length == 0)
            {
                Debug.LogWarning("MicroEditor_AssetUtilities: Can't find prefab.");
                return null;
            }

            if (guids.Length > 1)
            {
                Debug.LogWarning("MicroEditor_AssetUtilities: There are multiple " + prefabName + " prefabs. Found: " +
                                 guids.Length);
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(
                AssetDatabase.GUIDToAssetPath(guids[0])); // Return it as GameObject
        }

        /// <summary>
        ///     Returns path to the folder specified as second parameter
        /// </summary>
        /// <param name="parentFolder">Call this with "Assets" parameter</param>
        /// <param name="targetFolderName">Name of the folder we are looking for</param>
        /// <returns>String of the folder path (no / at the end)</returns>
        public static string FindFolderRecursively(string parentFolder, string targetFolderName)
        {
            var subFolders = AssetDatabase.GetSubFolders(parentFolder);
            foreach (var subFolder in subFolders)
            {
                if (Path.GetFileName(subFolder) == targetFolderName) return subFolder;

                var result = FindFolderRecursively(subFolder, targetFolderName);
                if (!string.IsNullOrEmpty(result)) return result;
            }

            return null;
        }

        /// <summary>
        ///     Returns first prefab from the folder with the specified name
        /// </summary>
        public static GameObject GetPrefab(string prefabFolder, string prefabName)
        {
            // Load all prefab assets from the specified folder
            var guids = AssetDatabase.FindAssets("t:prefab", new[] { prefabFolder });
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                if (prefab != null && prefab.name == prefabName) return prefab;
            }

            return null;
        }
    }
}
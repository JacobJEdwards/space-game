using System;
using System.IO;
using UnityEngine;

namespace Managers
{
    public static class FileManager
    {
        public static bool FileExists(string aFileName)
        {
            var fullPath = Path.Combine(Application.persistentDataPath, aFileName);
            return File.Exists(fullPath);
        }

        public static bool WriteToFile(string aFileName, string aFileContents)
        {
            var fullPath = Path.Combine(Application.persistentDataPath, aFileName);

            try
            {
                File.WriteAllText(fullPath, aFileContents);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to {fullPath} with exception {e}");
                return false;
            }
        }

        public static bool LoadFromFile(string aFileName, out string result)
        {
            var fullPath = Path.Combine(Application.persistentDataPath, aFileName);

            try
            {
                result = File.ReadAllText(fullPath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read from {fullPath} with exception {e}");
                result = "";
                return false;
            }
        }
    }
}
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class JsonDataService : IDataService
{
    public bool Save<T>(T data, string fileName, bool overwrite)
    {
        string path = GetPath(fileName);
        try
        {
            if (File.Exists(path) && !overwrite)
            {
                Debug.LogError($"File already exists and overwrite is false: {path}");
                return false;
            }

            // Use JsonConvert from Newtonsoft.Json
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Could not save data to {path}: {e.Message}");
            return false;
        }
    }

    public T Load<T>(string fileName)
    {
        string path = GetPath(fileName);
        try
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"File not found: {path}");
                return default(T);
            }

            string json = File.ReadAllText(path);
            // Use JsonConvert from Newtonsoft.Json
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Could not load data from {path}: {e.Message}");
            return default(T);
        }
    }

    public void Delete(string fileName)
    {
        string path = GetPath(fileName);
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Could not delete file {path}: {e.Message}");
        }
    }

    public void ClearAllData()
    {
        Debug.LogWarning("ClearAllData not fully implemented. Implement with caution.");
    }

    public IEnumerable<string> ListSaves()
    {
        string directoryPath;

#if UNITY_EDITOR
        directoryPath = Application.dataPath;
#else
    directoryPath = Application.persistentDataPath;
#endif

        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Debug.LogWarning($"Directory not found: {directoryPath}");
                return new List<string>();
            }

            // Get all files starting with "session_" and ending with ".json"
            string[] files = Directory.GetFiles(directoryPath, "session_*.json");

            // Convert full paths to file names only (without directory)
            List<string> saveFiles = new List<string>();
            foreach (string file in files)
            {
                saveFiles.Add(Path.GetFileName(file));
            }

            return saveFiles;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Could not list save files: {e.Message}");
            return new List<string>();
        }
    }

    private string GetPath(string fileName)
    {
#if UNITY_EDITOR
        return Path.Combine(Application.dataPath, fileName);
#else
        return Path.Combine(Application.persistentDataPath, fileName);
#endif
    }
}

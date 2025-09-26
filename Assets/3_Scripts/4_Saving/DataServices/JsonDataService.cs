using UnityEngine;
using System.IO;
using System.Collections.Generic;
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

            string json = JsonUtility.ToJson(data, true);
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
            return JsonUtility.FromJson<T>(json);
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
        // This method should be implemented carefully, potentially deleting all save files.
        // For now, it's left as a placeholder or to be implemented with specific file patterns.
        Debug.LogWarning("ClearAllData not fully implemented. Implement with caution.");
    }

    public IEnumerable<string> ListSaves()
    {
        // This method should return a list of available save files.
        // For now, it's left as a placeholder.
        Debug.LogWarning("ListSaves not fully implemented. Implement with caution.");
        return new List<string>();
    }

    private string GetPath(string fileName)
    {
        return Path.Combine(Application.dataPath, fileName);
    }
}

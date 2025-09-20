using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class JsonDataService : IDataService
{
    public void Save<T>(T data, string fileName)
    {
        //dataPath is to be changed with persistent on build
        string path = Path.Combine(Application.dataPath, fileName);
        try
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save {typeof(T).Name}. Error: {e.Message}");
        }
    }

    public T Load<T>(string fileName) where T : class
    {
        //dataPath is to be changed with persistent on build
        string path = Path.Combine(Application.dataPath, fileName);
        if (!File.Exists(path)) return null;

        try
        {
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load {typeof(T).Name}. Error: {e.Message}");
            return null;
        }
    }
}
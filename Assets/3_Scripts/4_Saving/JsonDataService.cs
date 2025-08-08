using UnityEngine;
using System.IO;
using Newtonsoft.Json; // IMPORTANT: Use Newtonsoft

public class JsonDataService : IDataService
{
    public void Save(SceneSaveData data, string fileName)
    {
        string path = Path.Combine(Application.dataPath, fileName);
        try
        {
            // We don't need custom converters because we are using DTOs.
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save data. Error: {e.Message}");
        }
    }

    public SceneSaveData Load(string fileName)
    {
        string path = Path.Combine(Application.dataPath, fileName);
        if (!File.Exists(path)) return null;

        try
        {
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<SceneSaveData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load data. Error: {e.Message}");
            return null;
        }
    }
}

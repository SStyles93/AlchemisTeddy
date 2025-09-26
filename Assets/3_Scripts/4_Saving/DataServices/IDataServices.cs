using System.Collections.Generic;

/// <summary>
/// Defines the contract for a data service that can save and load the
/// entire hierarchical scene state.
/// This decouples the GameManager from the specific serialization method (e.g., JSON, binary).
/// </summary>
public interface IDataService
{
    bool Save<T>(T data, string fileName, bool overwrite = false);
    T Load<T>(string fileName);
    void Delete(string fileName);
    void ClearAllData();
    IEnumerable<string> ListSaves();
}

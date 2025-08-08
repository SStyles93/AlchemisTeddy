/// <summary>
/// Defines the contract for a data service that can save and load the
/// entire hierarchical scene state.
/// This decouples the GameManager from the specific serialization method (e.g., JSON, binary).
/// </summary>
public interface IDataService
{
    /// <summary>
    /// Saves the provided scene data to a file.
    /// </summary>
    /// <param name="data">The root object containing all hierarchical scene data.</param>
    /// <param name="fileName">The name of the file to save to.</param>
    void Save(SceneSaveData data, string fileName);

    /// <summary>
    /// Loads scene data from a file.
    /// </summary>
    /// <param name="fileName">The name of the file to load from.</param>
    /// <returns>The deserialized root object containing all scene data, or null if not found.</returns>
    SceneSaveData Load(string fileName);
}

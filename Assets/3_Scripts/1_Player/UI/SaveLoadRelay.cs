using UnityEngine;

public class SaveLoadRelay : MonoBehaviour
{
    /// <summary>
    /// Method to call SaveScene on the GameManager
    /// </summary>
    public void SaveGame()
    {
        if(SessionManager.Instance == null)
        {
            Debug.LogWarning($"{this} - SaveGame - No SessionManager Instance available");
            return;
        }
        SessionManager.Instance.SaveSession("1");
    }

    /// <summary>
    /// Method to call LoadScene on the GameManager
    /// </summary>
    public void LoadGame()
    {
        if (SessionManager.Instance == null)
        {
            Debug.LogWarning($"{this} - LoadGame - No SessionManager Instance available");
            return;
        }
        SessionManager.Instance.LoadSession("1");
    }
}

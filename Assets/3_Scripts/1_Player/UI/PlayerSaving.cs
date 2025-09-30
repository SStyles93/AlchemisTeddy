using UnityEngine;

public class PlayerSaving : MonoBehaviour
{
    /// <summary>
    /// Method to call SaveScene on the GameManager
    /// </summary>
    public void SaveGame()
    {
        SessionManager.Instance.SaveSession("1");
    }

    /// <summary>
    /// Method to call LoadScene on the GameManager
    /// </summary>
    public void LoadGame()
    {
        SessionManager.Instance.LoadSession("1");
    }
}

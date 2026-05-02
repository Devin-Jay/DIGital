using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectSite : MonoBehaviour
{   
    public void LoadSite(string siteName)
    {
        Debug.Log("Loading site: " + siteName);

        if (SessionManager.Instance == null)
        {
            Debug.LogError("SessionManager instance is null.");
            return;
        }

        SessionManager.Instance.selectedSite = siteName;
        SceneManager.LoadScene(siteName);
    }
}

using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DroneController : MonoBehaviour
{
    [SerializeField] private TextMeshPro interactPrompt;
    private bool isInArea = false;
    private string areaSceneName;
    // Update is called once per frame
    void Update()
    {
        HandleMovement();

        if (isInArea && Input.GetKeyDown(KeyCode.F) && DoesSceneExist(areaSceneName))
        {
            // Load the scene associated with the area
            UnityEngine.SceneManagement.SceneManager.LoadScene(areaSceneName);
        }
    }

    private void HandleMovement()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, moveVertical,0.0f);
        transform.Translate(movement * Time.deltaTime * 4f);
    }

    public void ShowInteractPrompt(string areaName, string areaSceneName)
    {
        interactPrompt.enabled = true;
        interactPrompt.text = "Press F to travel to " + areaName;
        isInArea = true;
        this.areaSceneName = areaSceneName;
    }
    public void HideInteractPrompt()
    {
        interactPrompt.enabled = false;
        isInArea = false;
    }

    public bool DoesSceneExist(string sceneName)
{
    // Returns -1 if the scene is not found in the Build Settings
    int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);
    return buildIndex != -1;
}
}

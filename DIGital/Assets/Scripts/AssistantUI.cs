using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public class AssistantUI : MonoBehaviour
{
    public GameObject ui;

    public FirstPersonController fpsController;

    public ExcavationUIManager excavationUI;

    // AI STUFF
    // public LLMCharacter llmCharacter;
    public InputField input;
    public Text AIText;
    public string serverBaseUrl = "https://digital-ty59.onrender.com";
    // public string serverBaseUrl = "http://localhost:4000";

    // debug / options
    public KeyCode toggleKey = KeyCode.Tab;

    public void Start()
    {
        // start with UI closeed
        if( ui != null )
        {
            ui.SetActive(false);
        }
    }

    private void Update()
    {
        if (WalkExcavateIntroController.IsIntroOpen || 
           (SurveyModeManager.Instance != null && SurveyModeManager.Instance.IsSurveyModeActive))
        {
            return;
        }

        if (Input.GetKeyDown(toggleKey))
        {
            ToggleAssistant();
        }
    }

    public void ToggleAssistant()
    {
        if (WalkExcavateIntroController.IsIntroOpen || 
           (SurveyModeManager.Instance != null && SurveyModeManager.Instance.IsSurveyModeActive))
        {
            return;
        }

        Debug.Log("[AssistantUI] ToggleAssistant called");

        if (ui == null)
        {
            Debug.LogWarning("[AssistantUI] ui is not assigned!");
            return;
        }

        bool newState = !ui.activeSelf;
        Debug.Log("[AssistantUI] ui active before toggle: " + ui.activeSelf);
        ui.SetActive(newState);

        if (newState)
        {
            // hide welcome text
            if (excavationUI != null) excavationUI.SetWelcomeVisible(false);

            // OPEN: unlock cursor & optionally freeze FPS controller
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;

            if (fpsController != null)
            {
                fpsController.enabled = false;
            }

            // esnure input field is selected object (for global language popup)
            if (input != null)
            {
                EventSystem.current.SetSelectedGameObject(input.gameObject);
                input.ActivateInputField();
                input.Select();
            }
        }
        else
        {
            // show welcome text
            if (excavationUI != null) excavationUI.SetWelcomeVisible(true);

            // CLOSE: lock cursor again & re-enable FPS controller
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;

            if (fpsController != null)
            {
                fpsController.enabled = true;
            }
        }
    }

    public async void onInputFieldSubmit(string message)
    {
        input.interactable = false;
        AIText.text = "...";

        PromptRequest data = new PromptRequest { prompt = message };
        StartCoroutine(PostToApiCoroutine("/api/ai", data));
    }

    public void SetAIText(string text)
    {
        AIText.text = text;
    }

    public void AIReplyComplete()
    {
        input.interactable = true;
        input.Select();
        input.text = "";
    }

    IEnumerator PostToApiCoroutine(string route, object payload)
    {
        string url = $"{serverBaseUrl}{route}";
        string json = JsonUtility.ToJson(payload);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"[API] Sending POST to {url} with payload: {json}");
            yield return req.SendWebRequest();

            string responseText = req.downloadHandler != null
                ? req.downloadHandler.text
                : "(no downloadHandler)";

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[API] Error {req.responseCode}: {req.error}");
                Debug.LogError($"[API] Server response: {responseText}");
                yield break;
            }
            ApiResponse response =
                JsonUtility.FromJson<ApiResponse>(responseText);

            SetAIText(response.reply);
            AIReplyComplete();
        }
    }


    [System.Serializable]
    public class PromptRequest
    {
        public string prompt;
    }

    [System.Serializable]
    public class ApiResponse
    {
        public bool ok;
        public string reply;
    }
}

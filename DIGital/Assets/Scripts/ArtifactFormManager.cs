using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// class
public class ArtifactFormManager : MonoBehaviour
{
    // input fields
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField DateDiscoveredInput;
    [SerializeField] private TMP_InputField InvestigatorInput;
    [SerializeField] private TMP_InputField AreaInput;
    [SerializeField] private TMP_InputField UnitInput;
    [SerializeField] private TMP_InputField LayerInput;
    [SerializeField] private TMP_InputField SiteInput;
    [SerializeField] private TMP_InputField AssociatedFeaturesInput;
    [SerializeField] private TMP_InputField DecorativeTechInput;
    [SerializeField] private TMP_InputField MaterialInput;
    [SerializeField] private TMP_InputField FiringInput;
    [SerializeField] private TMP_InputField PaintInput;
    [SerializeField] private TMP_InputField CulturalAffiliationInput;
    [SerializeField] private TMP_InputField ObjectClassInput;
    [SerializeField] private TMP_InputField BagNumberInput;
    [SerializeField] private TMP_Dropdown ArtifactIdDropdown;

    // popup panel game object  
    [Header("UI Feedback")]
    [SerializeField] private TMP_Text StatusText;
    [SerializeField] private GameObject PanelRoot;

    // analysis scene
    [Header("Analysis Scene")]
    [SerializeField] private string analysisSceneName = "ArtifactAnalysis";

    // api settings
    [Header("API Settings")]
    [SerializeField] private string apiUrl = 
                    "https://digital-ty59.onrender.com/api/artifacts";

    // localization
    [Header("Localization")]
    [SerializeField] private string table = "UI";
    [SerializeField] private string errorDetailsKey = "artifact_collection_status_error_details";

    // registry
    [Header("Registry")]
    [SerializeField] private ArtifactIdRegistry idRegistry;

    // warning
    [Header("Continue Warning")]
    [SerializeField] private GameObject ContinueWarningPanel;
    [SerializeField] private TMP_Text ContinueWarningText;

    [Header("Input Suspension")]
    [SerializeField] private MonoBehaviour[] playerScriptsToDisable;
    [SerializeField] private MonoBehaviour[] surveyScriptsToDisable;

    // excavation UI to hide during analysis
    [Header("Excavation UI To Hide During Analysis")]
    [SerializeField] private GameObject[] excavationUIToHideDuringAnalysis;
    private readonly Dictionary<GameObject, bool> savedExcavationUIStates =
        new Dictionary<GameObject, bool>();

    private readonly Dictionary<MonoBehaviour, bool> savedScriptStates = new Dictionary<MonoBehaviour, bool>();

    // cache to not recreate error message each time
    private LocalizedString errorLocalizedString;

    // artifact interactable reference
    private ArtifactInteractable currentArtifactInteractable;

    // input field types
    [System.Serializable]
    private class ArtifactData
    {
        public string date_discovered;
        public string investigator;
        public string area;
        public string unit;
        public string layer;
        public string site;
        public string associated_features;
        public string decorative_tech;
        public string material;
        public string firing;
        public string paint;
        public string cultural_affiliation;
        public string object_class;
        public string bag_number;
        public string artifact_id;
        public string userId;
    }

    // reference key
    [System.Serializable]
    private class ReferenceArtifact
    {
        public int id;
        public string date_discovered;
        public string investigator;
        public string area;
        public string unit;
        public string layer;
        public string site;
        public string associated_features;
        public string decorative_tech;
        public string material;
        public string firing;
        public string paint;
        public string cultural_affiliation;
        public string object_class;
        public string bag_number;
        public string artifact_id;
        public string created_at;
        public string updated_at;
        public string user_id;
    }

    // reference reponse
    [System.Serializable]
    private class ReferenceArtifactResponse
    {
        public bool ok;
        public ReferenceArtifact artifact;
        public string error;
    }

    private void Awake()
    {
        // cache this once (uses key + table from Inspector)
        errorLocalizedString = new LocalizedString(table, errorDetailsKey);
    }

    private void Start()
    {
        // ensure the panel starts hidden if you want it that way
        if (PanelRoot != null)
        {
            PanelRoot.SetActive(false);
        }

        HideContinueWarning();
        PopulateArtifactIdDropdown();

        if (ArtifactIdDropdown != null)
        {
            ArtifactIdDropdown.onValueChanged.AddListener(OnArtifactDropdownChanged);
        }
    }

    // status text with localization
    private void SetStatus(string key)
    {
        if (StatusText == null) return;
        StatusText.text = LocalizationSettings.StringDatabase.GetLocalizedString(table, key);
    }

    private void SetStatusErrorWithDetails(string details)
    {
        if (StatusText == null) return;

        errorLocalizedString.Arguments = new object[] { new { details } }; // smart string
        StatusText.text = errorLocalizedString.GetLocalizedString();
    }

    public void OpenForm(ArtifactInteractable artifactInteractable)
    {
        currentArtifactInteractable = artifactInteractable;
        ResetStatusUI();

        // preselect artifact ID for the collecton form
        PreselectArtifactIdFromInteractable();

        // hide continue warning when first opening
        HideContinueWarning();

        if (PanelRoot != null)
        {
            Debug.Log("[ArtifactFormManager] OpenForm() - enabling PanelRoot");
            PanelRoot.SetActive(true);

            // handle scripts
            DisableScriptsForForm();

            // unlock cursor for UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;

            // pause game while form is open
            Time.timeScale = 0f;
        }

        else
        {
            Debug.LogWarning("[ArtifactFormManager] PanelRoot is not assigned!");
        }
    }

    public void CloseForm()
    {
        if (PanelRoot != null)
        {
            PanelRoot.SetActive(false);
        }

        // hide warning and handle scripts
        HideContinueWarning();
        RestoreScriptsAfterForm();

        // restore cursor and resume game if you paused it
        RestoreCursorAfterForm();
        Time.timeScale = 1f;
    }


    // submit button clicked
    public void OnSubmitButtonClicked()
    {
        // front end validation
        if (!ValidateInputs())
        {
            return;
        }

        string enteredId = GetSelectedArtifactId();
        
        if (string.IsNullOrEmpty(enteredId))
        {
            SetStatus("artifact_collection_status_invalid_artifact_id");
            return;
        }

        SetStatus("artifact_collection_status_submit");

        ArtifactData data = new ArtifactData
        {
            date_discovered = DateDiscoveredInput.text.Trim(),
            investigator = InvestigatorInput.text.Trim(),
            area = AreaInput.text.Trim(),
            unit = UnitInput.text.Trim(),
            layer = LayerInput.text.Trim(),
            site = SiteInput.text.Trim(),
            associated_features = AssociatedFeaturesInput.text.Trim(),
            decorative_tech = DecorativeTechInput.text.Trim(),
            material = MaterialInput.text.Trim(),
            firing = FiringInput.text.Trim(),
            paint = PaintInput.text.Trim(),
            cultural_affiliation = CulturalAffiliationInput.text.Trim(),
            object_class = ObjectClassInput.text.Trim(),
            bag_number = BagNumberInput.text.Trim(),
            artifact_id = enteredId,
            userId = (SessionManager.Instance != null &&
                        SessionManager.Instance.IsLoggedIn)
                        ? SessionManager.Instance.UserId
                        : null            
        };

        StartCoroutine(SubmitArtifactCoroutine(data));
    }

    // continue button clicked
    public void OnCancelButtonClicked()
    {
        Debug.Log("[ArtifactFormManager] Continue button clicked");
        ResetStatusUI();

        // set text
        if (ContinueWarningText != null)
        {
            ContinueWarningText.text = "Pressing Continue will remove the artifact from the site " +
                                        "without any recordings, and nothing will be sent to the " +
                                        "database. Are you sure you'd like to proceed?";
        }

        // display panel
        if (ContinueWarningPanel != null)
        {
            ContinueWarningPanel.SetActive(true);
        }
    }

    // yes button in warning
    public void OnConfirmContinueWithoutRecording()
    {
        Debug.Log("[ArtifactFormManager] Continue confirmed");

        string artifactId = GetSelectedArtifactId();

        if (string.IsNullOrWhiteSpace(artifactId))
        {
            SetStatus("artifact_collection_status_invalid_artifact_id");
            return;
        }

        // artfiact analysis flow 
        ArtifactAnalysisLaunchContext.StartMode = ArtifactAnalysisStart.AnswerKeyFromExcavation;
        ArtifactAnalysisLaunchContext.UserSubmission = null;
        ArtifactAnalysisLaunchContext.ArtifactId = artifactId;

        FinishArtifactBeforeAnalysisScene();
        LoadAnalysisSceneAdditive();
    }

    // no button in warning
    public void OnCancelContinueWarning()
    {
        // back to form
        Debug.Log("[ArtifactFormManager] Continue warning canceled");
        HideContinueWarning();
    }

    private bool ValidateInputs()
    {
        // required fields
        if (string.IsNullOrWhiteSpace(DateDiscoveredInput.text) ||
            string.IsNullOrWhiteSpace(InvestigatorInput.text) ||
            string.IsNullOrWhiteSpace(AreaInput.text) ||
            string.IsNullOrWhiteSpace(UnitInput.text) ||
            string.IsNullOrWhiteSpace(LayerInput.text) ||
            string.IsNullOrWhiteSpace(SiteInput.text) ||
            string.IsNullOrWhiteSpace(DecorativeTechInput.text) ||
            string.IsNullOrWhiteSpace(MaterialInput.text) ||
            string.IsNullOrWhiteSpace(FiringInput.text) ||
            string.IsNullOrWhiteSpace(PaintInput.text) ||
            string.IsNullOrWhiteSpace(CulturalAffiliationInput.text) ||
            string.IsNullOrWhiteSpace(ObjectClassInput.text) ||
            string.IsNullOrWhiteSpace(BagNumberInput.text))

        {
            SetStatus("artifact_collection_status_missing_fields");
            return false;
        }

        return true;
    }

    // submit artifact coroutine
    private IEnumerator SubmitArtifactCoroutine(ArtifactData artifactData)
    {
        SetStatus("artifact_collection_status_database");

        string json = JsonUtility.ToJson(artifactData);
        byte[] postData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

    #if UNITY_EDITOR
            Debug.Log($"Artifact JSON: {json}");
    #endif

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
    #if UNITY_EDITOR
                Debug.Log("Artifact submit success: " +
                        request.downloadHandler.text);
    #endif

                SingleArtifactResponse response =
                    JsonUtility.FromJson<SingleArtifactResponse>(request.downloadHandler.text);

                if (response == null || !response.ok || response.artifact == null)
                {
                    SetStatusErrorWithDetails("Artifact was submitted, but the response could not be loaded.");
                    yield break;
                }

                SetStatus("artifact_collection_status_success");

                ArtifactAnalysisLaunchContext.StartMode = ArtifactAnalysisStart.UserSubmissionFromExcavation;
                ArtifactAnalysisLaunchContext.UserSubmission = response.artifact;
                ArtifactAnalysisLaunchContext.ArtifactId = response.artifact.artifact_id;

                FinishArtifactBeforeAnalysisScene();
                LoadAnalysisSceneAdditive();
            }

            else
            {
                string serverText = request.downloadHandler != null ? request.downloadHandler.text : "";
                string details = !string.IsNullOrEmpty(serverText) ? serverText : request.error;

                SetStatusErrorWithDetails(details);
            }
        }
    }

    private void FinishArtifactBeforeAnalysisScene()
    {
        if (currentArtifactInteractable != null)
        {
            currentArtifactInteractable.MarkAsRecorded();
            currentArtifactInteractable = null;
        }

        ClearForm();
        ResetStatusUI();
        HideContinueWarning();

        if (PanelRoot != null)
        {
            PanelRoot.SetActive(false);
        }

        HideExcavationUIForAnalysis();

        // moving into an additive ArtifactAnalysis scene
        // keep gameplay scripts disabled until Analysis Continue is clicked.
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // load additive analysis scene
    private void LoadAnalysisSceneAdditive()
    {
        Scene existingScene = SceneManager.GetSceneByName(analysisSceneName);

        if (existingScene.IsValid() && existingScene.isLoaded)
        {
            Debug.LogWarning($"[ArtifactFormManager] {analysisSceneName} is already loaded.");
            return;
        }

        SceneManager.LoadScene(analysisSceneName, LoadSceneMode.Additive);
    }

    // called by ArtifactAnalysisManager when the temporary Analysis Continue button is clicked.
    public void RestoreAfterAnalysisScene()
    {
        RestoreExcavationUIAfterAnalysis();
        RestoreScriptsAfterForm();
        RestoreCursorAfterForm();
        Time.timeScale = 1f;
    }

    // coroutine to fetch and auto-fill the form
    private IEnumerator LoadReferenceArtifactCoroutine(string artifactId)
    {
        if (string.IsNullOrWhiteSpace(artifactId))
        {
            yield break;
        }

        string url = $"{apiUrl}/reference/{UnityWebRequest.EscapeURL(artifactId)}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string serverText = request.downloadHandler != null
                    ? request.downloadHandler.text
                    : "";

                Debug.LogWarning(
                    $"Reference artifact lookup failed: " +
                    $"{request.responseCode} - {request.error} - {serverText}");

                    yield break;
            }

            string json = request.downloadHandler.text;

    #if UNITY_EDITOR
            Debug.Log($"Reference artifact JSON: {json}");
    #endif

            ReferenceArtifactResponse response =
                JsonUtility.FromJson<ReferenceArtifactResponse>(json);

            if (response == null || !response.ok || response.artifact == null)
            {
                Debug.LogWarning("Reference artifact response was empty or invalid.");
                yield break;
            }

            ApplyReferenceArtifactToForm(response.artifact);
        }
    }

    // clear form
    private void ClearForm()
    {
        DateDiscoveredInput.text = "";
        InvestigatorInput.text = "";
        AreaInput.text = "";
        UnitInput.text = "";
        LayerInput.text = "";
        SiteInput.text = "";
        AssociatedFeaturesInput.text = "";
        DecorativeTechInput.text = "";
        MaterialInput.text = "";
        FiringInput.text = "";
        PaintInput.text = "";
        CulturalAffiliationInput.text = "";
        ObjectClassInput.text = "";
        BagNumberInput.text = "";
    }

    private void PopulateArtifactIdDropdown()
    {
        if (ArtifactIdDropdown == null || idRegistry == null || idRegistry.entries == null)
        {
            Debug.LogWarning("[ArtifactFormManager] Dropdown or ID registry not assigned.");
            return;
        }

        var options = new List<TMP_Dropdown.OptionData>();

        foreach (var entry in idRegistry.entries)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.artifactId)) continue;

            string id = entry.artifactId.Trim();
            string label = !string.IsNullOrWhiteSpace(entry.displayName)
                ? $"{entry.displayName} ({id})"
                : id;

            options.Add(new TMP_Dropdown.OptionData(label));
        }

        ArtifactIdDropdown.ClearOptions();
        ArtifactIdDropdown.AddOptions(options);
        ArtifactIdDropdown.value = 0;
        ArtifactIdDropdown.RefreshShownValue();
    }

    private string GetSelectedArtifactId()
    {
        if (ArtifactIdDropdown == null || idRegistry == null || idRegistry.entries == null)
            return null;

        int index = ArtifactIdDropdown.value;
        if (index < 0 || index >= idRegistry.entries.Count)
            return null;

        return idRegistry.entries[index]?.artifactId?.Trim();
    }

    private void PreselectArtifactIdFromInteractable()
    {
        if (currentArtifactInteractable == null || ArtifactIdDropdown == null || 
            idRegistry == null || idRegistry.entries == null)
            {
                return;
            }

        string targetId = currentArtifactInteractable.ArtifactId;

        if (string.IsNullOrWhiteSpace(targetId))
        {
            return;
        }

        targetId = targetId.Trim();

        for ( int index = 0; index < idRegistry.entries.Count; index++ )
        {
            var entry = idRegistry.entries[index];
            
            if (entry == null || string.IsNullOrWhiteSpace(entry.artifactId))
            {
                continue;
            }

            if (string.Equals(entry.artifactId.Trim(), targetId, System.StringComparison.OrdinalIgnoreCase))
            {
                ArtifactIdDropdown.SetValueWithoutNotify(index);
                ArtifactIdDropdown.RefreshShownValue();

                StartCoroutine(LoadReferenceArtifactCoroutine(targetId));

                return;
            }
        }

        Debug.LogWarning($"[ArtifactFormManager] Could not find artifact ID '{targetId}' in ArtifactIdRegistry.");
    }

    // restore cursor so user can interact with artifacts
    // after collection form closed
    private void RestoreCursorAfterForm()
    {
        bool surveyModeActive =
            SurveyModeManager.Instance != null &&
            SurveyModeManager.Instance.IsSurveyModeActive;

        if (surveyModeActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // bool to disable hotkeys while form is open
    public bool IsFormOpen
    {
        get { return PanelRoot != null && PanelRoot.activeInHierarchy; }
    }

    // reset status text after form has been closed
    private void ResetStatusUI()
    {
        if (StatusText != null)
        {
            StatusText.text = "";
        }
    }

    // hide warning panel
    private void HideContinueWarning()
    {
        if (ContinueWarningPanel != null)
        {
            ContinueWarningPanel.SetActive(false);
        }
    }

    // enable/disable script helper
    private void SetScriptsEnabled(MonoBehaviour[] scripts, bool enabledState)
    {
        if (scripts == null) return;

        foreach (MonoBehaviour script in scripts)
        {
            if (script != null)
            {
                script.enabled = enabledState;
            }
        }
    }

    // helper function to disable scripts when form opens
    private void DisableScriptsForForm()
    {
        savedScriptStates.Clear();

        SaveAndDisable(playerScriptsToDisable);
        SaveAndDisable(surveyScriptsToDisable);
    }

    // remember which scripts were disabled
    private void SaveAndDisable(MonoBehaviour[] scripts)
    {
        if (scripts == null) return;

        foreach (MonoBehaviour script in scripts)
        {
            if (script == null) continue;

            if (!savedScriptStates.ContainsKey(script))
            {
                savedScriptStates[script] = script.enabled;
            }

            script.enabled = false;
        }
    }

    // helper to restore saved scripts that were disabled
    private void RestoreScriptsAfterForm()
    {
        foreach (var pair in savedScriptStates)
        {
            if (pair.Key != null)
            {
                pair.Key.enabled = pair.Value;
            }
        }

        savedScriptStates.Clear();
    }

    // helper to decide fields which to get filled
    private void ApplyReferenceArtifactToForm(ReferenceArtifact artifact)
    {
        // check
        if (artifact == null) return;

        // TODO, use smart strings to auto-fill and maybe something like Sys.Date()
        // DateDiscoveredInput.text = FormatDate(artifact.date_discovered);
        // InvestigatorInput.text = artifact.investigator ?? "";

        AreaInput.text = artifact.area ?? "";

        // UnitInput.text = artifact.unit ?? "";
        // LayerInput.text = artifact.layer ?? "";
        
        SiteInput.text = artifact.site ?? "";
        AssociatedFeaturesInput.text = artifact.associated_features ?? "";
        DecorativeTechInput.text = artifact.decorative_tech ?? "";
        MaterialInput.text = artifact.material ?? "";
        FiringInput.text = artifact.firing ?? "";
        PaintInput.text = artifact.paint ?? "";
        CulturalAffiliationInput.text = artifact.cultural_affiliation ?? "";
        ObjectClassInput.text = artifact.object_class ?? "";
        BagNumberInput.text = artifact.bag_number ?? "";
    }

    private void OnArtifactDropdownChanged(int index)
    {
        string selectedId = GetSelectedArtifactId();

        if (string.IsNullOrWhiteSpace(selectedId))
        {
            return;
        }

        StartCoroutine(LoadReferenceArtifactCoroutine(selectedId));
    }

    // hide excavation ui
    private void HideExcavationUIForAnalysis()
    {
        savedExcavationUIStates.Clear();

        if (excavationUIToHideDuringAnalysis == null) return;

        foreach (GameObject UIObject in excavationUIToHideDuringAnalysis)
        {
            if (UIObject == null) continue;

            if (!savedExcavationUIStates.ContainsKey(UIObject))
            {
                savedExcavationUIStates[UIObject] = UIObject.activeSelf;
            }

            UIObject.SetActive(false);
        }
    }

    // restore excavation ui
    private void RestoreExcavationUIAfterAnalysis()
    {
        foreach (var pair in savedExcavationUIStates)
        {
            if (pair.Key != null)
            {
                pair.Key.SetActive(pair.Value);
            }
        }

        savedExcavationUIStates.Clear();
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

// class
public class ArtifactAnalysisManager : MonoBehaviour
{
    // input fields (really output fields but Unity calls them input)
    [Header("Display Fields (InputFields)")]
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

    [Header("Logout / Navigation")]
    [SerializeField] private string accountManagementSceneName = "AccountManagement";

    // dropdown menu
    [Header("Dropdown")]
    [SerializeField] private TMP_Dropdown SelectArtifact;

    // status text
    [Header("Status Text")]
    [SerializeField] private TMP_Text StatusText;

    // api
    [Header("API Settings")]
    [SerializeField] private string apiUrl =
                     "https://digital-ty59.onrender.com/api/artifacts";

    // localization
    [Header("Localization")]
    [SerializeField] private string table = "UI";

    // artifact analysis reference buttons
    [Header("Comparison Buttons")]
    [SerializeField] private GameObject AnswerKeyButton;
    [SerializeField] private GameObject UserSubmissionButton;
    [SerializeField] private GameObject ContinueButton;
    [SerializeField] private TMP_Text ViewModeText;

    // artifact pivot
    [Header("3D Preview Pivot")]
    [SerializeField] private Transform artifactPivot;

    // localization table keys
    [SerializeField] private string LoadingList = "analysis_status_loading_list";
    [SerializeField] private string ErrorLoadingList = "analysis_status_error_loading_list";
    [SerializeField] private string NoArtifactsToAnalyze = "analysis_status_no_artifacts_to_analyze";
    [SerializeField] private string InvalidSelection = "analysis_status_invalid_selection";
    [SerializeField] private string DataLoaded = "analysis_status_data_loaded";
    [SerializeField] private string NoArtifactData = "analysis_status_no_artifact_data";
    [SerializeField] private string NoArtifactsInDb = "analysis_status_no_artifacts_in_db";
    [SerializeField] private string SelectAndAnalyze = "analysis_status_select_and_analyze";
    [SerializeField] private string DropdownLabelKey = "analysis_dropdown_label";
    [SerializeField] private string DropdownPlaceholderKey = "analysis_dropdown_placeholder";

    // re-translation
    private enum StatusMode { None, Key, ServerError }
    private StatusMode _statusMode = StatusMode.None;
    private string _lastStatusKey = null;
    private string _lastServerError = null;

    // smart string error 
    [SerializeField] private string ServerErrorDetails = "analysis_status_server_error_details";

    // registry
    [SerializeField] private ArtifactSceneModelRegistry modelRegistry;
    [SerializeField] private GameObject defaultModel; // optional fallback

    private LocalizedString lsServerErrorDetails;
    private LocalizedString lsDropdownLabel;
    private LocalizedString lsDropdownPlaceholder;

    [Header("UI Panel")]
    [SerializeField] private GameObject BackgroundPanel;

    // local cache of artifacts
    private List<ArtifactRecord> _artifacts = new List<ArtifactRecord>();
    private ArtifactRecord currentUserSubmission;
    private ArtifactRecord currentAnswerKey;
    private string currentArtifactId;

    public GameObject CurrentActiveModel { get; private set; }

    // functions

    private void Awake()
    {
        // intialize error string and dropdown
        lsServerErrorDetails = new LocalizedString(table, ServerErrorDetails);
        lsDropdownLabel = new LocalizedString(table, DropdownLabelKey);
        lsDropdownPlaceholder = new LocalizedString(table, DropdownPlaceholderKey);
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

        if (SelectArtifact != null)
        {
            SelectArtifact.onValueChanged.AddListener(OnArtifactDropdownChanged);
        }
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;

        if (SelectArtifact != null)
        {
            SelectArtifact.onValueChanged.RemoveListener(OnArtifactDropdownChanged);
        }
    }

    private void OnLocaleChanged(Locale _)
    {
        // refresh dropdown labels
        if (_artifacts != null && _artifacts.Count > 0)
            PopulateDropdownOptions();

        // refresh the current status message
        RefreshStatusText();
    }

    private void RefreshStatusText()
    {
        if (StatusText == null) return;

        switch (_statusMode)
        {
            case StatusMode.Key:
                if (!string.IsNullOrEmpty(_lastStatusKey))
                    StatusText.text = LocalizationSettings.StringDatabase.GetLocalizedString(table, _lastStatusKey);
                break;

            case StatusMode.ServerError:
                if (!string.IsNullOrEmpty(_lastServerError))
                {
                    lsServerErrorDetails.Arguments = new object[] { new { error = _lastServerError } };
                    StatusText.text = lsServerErrorDetails.GetLocalizedString();
                }
                break;
        }
    }

    // initialize analysis panel
    private void Start()
    {
        /* // Guard: if not logged in, send them to account management
        if (SessionManager.Instance == null || !SessionManager.Instance.IsLoggedIn)
        {
            SceneManager.LoadScene(accountManagementSceneName);
            return;
        } */

        Debug.Log(
            $"[ArtifactAnalysisManager] Start() called. " +
            $"LaunchedFromExcavation={ArtifactAnalysisLaunchContext.LaunchedFromExcavation}, " +
            $"StartMode={ArtifactAnalysisLaunchContext.StartMode}, " +
            $"ArtifactId={ArtifactAnalysisLaunchContext.ArtifactId}"
        );

        // set fields to read only
        SetFieldsReadOnly(true);

        // set bool
        bool launchedFromExcavation = ArtifactAnalysisLaunchContext.LaunchedFromExcavation;

        if (ContinueButton != null)
        {
            ContinueButton.SetActive(launchedFromExcavation);
        }

        if (launchedFromExcavation)
        {
            SetupFromExcavationLaunch();
            return;
        }

        // normal/final Analysis scene mode
        if (SelectArtifact != null)
        {
            SelectArtifact.gameObject.SetActive(true);
        }

        if (AnswerKeyButton != null)
        {
            AnswerKeyButton.SetActive(true);
        }

        if (UserSubmissionButton != null)
        {
            UserSubmissionButton.SetActive(false);
        }

        if (ViewModeText != null)
        {
            ViewModeText.text = "";
        }

        if (StatusText != null)
        {
            SetStatus(LoadingList);
        }

        StartCoroutine(LoadArtifactListCoroutine());
    }

    private void SetStatus(string key)
    {
        _statusMode = StatusMode.Key;
        _lastStatusKey = key;

        if (StatusText == null) return;
        StatusText.text = LocalizationSettings.StringDatabase.GetLocalizedString(table, key);
    }

    private void SetStatusServerError(string error)
    {
        _statusMode = StatusMode.ServerError;
        _lastServerError = error;

        if (StatusText == null) return;

        // uses Smart String in table
        lsServerErrorDetails.Arguments = new object[] { new { error } };
        StatusText.text = lsServerErrorDetails.GetLocalizedString();
    }

    // logout button
    public void OnLogoutButtonClicked()
    {
        // hide any current active model(s)
        CleanupActiveModelForLogout();

        // clear session
        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.ClearUser();
        }

        // load account management scene
        SceneManager.LoadScene(accountManagementSceneName);
    }

    // hide active models for logging out
    private void CleanupActiveModelForLogout()
    {
        if (CurrentActiveModel == null) return;

        // detach model pivot
        CurrentActiveModel.transform.SetParent(null, worldPositionStays: true);
        CurrentActiveModel.SetActive(false);
        CurrentActiveModel = null;

        // hide panel
        if (BackgroundPanel != null)
        {
            BackgroundPanel.SetActive(true);
        }
    }

    // CHANGED from analyze button to analyze on dropdown clicked
    public void OnArtifactDropdownChanged(int index)
    {
        // error handling
        if( _artifacts == null || _artifacts.Count == 0 )
        {
            SetStatus(NoArtifactsToAnalyze);
            return;
        }

        // index placeholder
        if (index == 0)
        {
            ClearUI();

            if (StatusText != null)
            {
                SetStatus(SelectAndAnalyze);
            }

            return;
        }

        int artifactIndex = index - 1;

        // error handling
        if ( artifactIndex < 0 || artifactIndex >= _artifacts.Count )
        {
            SetStatus(InvalidSelection);
            return;
        }

        // select artifact
        ArtifactRecord selectedArtifact = _artifacts[artifactIndex];

        currentUserSubmission = selectedArtifact;
        currentAnswerKey = null;
        currentArtifactId = selectedArtifact.artifact_id;

        PopulateUI(selectedArtifact);
        SetViewMode("User Submission");

        if (UserSubmissionButton != null)
        {
            UserSubmissionButton.SetActive(true);
        }

        if (AnswerKeyButton != null)
        {
            AnswerKeyButton.SetActive(true);
        }

        if( StatusText != null )
        {
            SetStatus(DataLoaded);
        }
    }

    // set input to read only
    private void SetFieldsReadOnly(bool readOnly)
    {
        TMP_InputField[] fields =
        {
            DateDiscoveredInput, InvestigatorInput, AreaInput, UnitInput,
            LayerInput, SiteInput, AssociatedFeaturesInput,
            DecorativeTechInput, MaterialInput, FiringInput,
            PaintInput, CulturalAffiliationInput, ObjectClassInput,
            BagNumberInput
        };

        foreach ( var field in fields )
        {
            if( field == null ) continue;
            field.readOnly = readOnly;
            field.interactable = true;
        }

        if (ArtifactIdDropdown != null)
        {
            ArtifactIdDropdown.interactable = false;
        }
    }

    private void SetArtifactIdDropdown(string artifactId)
    {
        if (ArtifactIdDropdown == null) return;

        ArtifactIdDropdown.ClearOptions();

        ArtifactIdDropdown.AddOptions(new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData(artifactId ?? "")
        });

        ArtifactIdDropdown.SetValueWithoutNotify(0);
        ArtifactIdDropdown.RefreshShownValue();

        ArtifactIdDropdown.interactable = false;
    }

    // clear fields when placeholder is selected
    private void ClearUI()
    {
        // reset comparison
        currentUserSubmission = null;
        currentAnswerKey = null;
        currentArtifactId = null;
        SetViewMode("");

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
        SetArtifactIdDropdown("");

        if (modelRegistry != null)
        {
            modelRegistry.HideAll();
        }

        if (defaultModel != null)
        {
            defaultModel.SetActive(false);
        }

        CurrentActiveModel = null;

        if (BackgroundPanel != null)
        {
            BackgroundPanel.SetActive(true);
        }

        if (UserSubmissionButton != null)
        {
            UserSubmissionButton.SetActive(false);
        }
    }

    // load artifact from database
    private IEnumerator LoadArtifactListCoroutine()
    {
        // make sure user is logged in and has id
        if ( SessionManager.Instance == null || string.IsNullOrEmpty(SessionManager.Instance.UserId))
        {
            Debug.LogError("[ArtifactAnalysisManager] No session userId found. Cannot load user artifacts.");
            if (StatusText != null) SetStatus(ErrorLoadingList);
            yield break;
        }

        // use node app JS code
        string userId = SessionManager.Instance.UserId;
        string url = $"{apiUrl}/mine/{UnityWebRequest.EscapeURL(userId)}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            // error handling
            if (request.result != UnityWebRequest.Result.Success)
            {
                string serverText = request.downloadHandler != null
                    ? request.downloadHandler.text
                    : "";

                Debug.LogError(
                    $"Error fetching artifact list: " +
                    $"{request.responseCode} - {request.error} - {serverText}");


                if (StatusText != null)
                {
                    SetStatus(ErrorLoadingList);
                }

                yield break;
            }

            string json = request.downloadHandler.text;

        #if UNITY_EDITOR
            Debug.Log($"Artifact list JSON: {json}");
        #endif

            ArtifactListResponse response =
                JsonUtility.FromJson<ArtifactListResponse>(json);

            if (response == null || !response.ok || response.artifacts == null)
            {

                if (response != null && !string.IsNullOrEmpty(response.error))
                {
                    SetStatusServerError(response.error);
                }

                else
                {
                    SetStatus(NoArtifactData);
                }

                yield break;
            }

            // get artifact list
            _artifacts = new List<ArtifactRecord>(response.artifacts);

            if( _artifacts.Count == 0 )
            {
                if( StatusText != null )
                {
                    SetStatus(NoArtifactsInDb);
                }

                if ( SelectArtifact != null )
                {
                    SelectArtifact.ClearOptions();
                }

                yield break;
            }

            // success, populate dropdown
            PopulateDropdownOptions();

            if( StatusText != null )
            {
                SetStatus(SelectAndAnalyze);
            }
        }
    }

    // populate dropdown function
    private void PopulateDropdownOptions()
    {
        if (SelectArtifact == null)
        {
            Debug.LogWarning("SelectArtifact is not assigned.");
            return;
        }

        var options = new List<TMP_Dropdown.OptionData>();

        // localized placeholder
        options.Add(new TMP_Dropdown.OptionData(lsDropdownPlaceholder.GetLocalizedString()));

        foreach (var artifact in _artifacts)
        {
            string rawDate = artifact.date_discovered ?? "";
            string shortDate = rawDate.Length >= 10 ? rawDate.Substring(0, 10) : rawDate;

            // Smart String arguments (named placeholders)
            lsDropdownLabel.Arguments = new object[]
            {
                new
                {
                    id = artifact.artifact_id ?? "",
                    excavator = artifact.investigator ?? "",
                    date = shortDate
                }
            };

            string label = lsDropdownLabel.GetLocalizedString();
            Debug.Log($"[ArtifactAnalysisManager] Dropdown option added: {label}");
            options.Add(new TMP_Dropdown.OptionData(label));
        }

        SelectArtifact.ClearOptions();
        SelectArtifact.AddOptions(options);
        SelectArtifact.SetValueWithoutNotify(0);
        SelectArtifact.RefreshShownValue();

        ClearUI();
    }

    // populate input fields
    private void PopulateUI(ArtifactRecord artifact)
    {
        if (artifact == null)
        {
            ClearUI();
            SetStatus(NoArtifactData);
            return;
        }

        // format date
        if (!string.IsNullOrEmpty(artifact.date_discovered))
        {
            string[] parts = artifact.date_discovered.Split('T');
            DateDiscoveredInput.text = parts[0].Trim();
        }
        
        else
        {
            DateDiscoveredInput.text = "";
        }

        InvestigatorInput.text = artifact.investigator;
        AreaInput.text = artifact.area;
        UnitInput.text = artifact.unit;
        LayerInput.text = artifact.layer;
        SiteInput.text = artifact.site;
        AssociatedFeaturesInput.text = artifact.associated_features;
        DecorativeTechInput.text = artifact.decorative_tech;
        MaterialInput.text = artifact.material;
        FiringInput.text = artifact.firing;
        PaintInput.text = artifact.paint;
        CulturalAffiliationInput.text = artifact.cultural_affiliation;
        ObjectClassInput.text = artifact.object_class;
        BagNumberInput.text = artifact.bag_number;

        SetArtifactIdDropdown(artifact.artifact_id);
        ShowModelForArtifact(artifact);

        if (BackgroundPanel != null)
        {
            BackgroundPanel.SetActive(false);
        }
    }

    private void ShowModelForArtifact(ArtifactRecord artifact)
    {
        // turn off all models referenced by the registry (so only one shows)
        if (modelRegistry != null)
            modelRegistry.HideAll();

        // hide default model (optional if we use one later)
        if (defaultModel != null)
            defaultModel.SetActive(false);

        CurrentActiveModel = null;

        if (artifact == null)
        {
            Debug.LogWarning("[ArtifactAnalysisManager] Artifact is null in ShowModelForArtifact");
            return;
        }

        string id = (artifact.artifact_id ?? "").Trim();
        Debug.Log($"[ArtifactAnalysisManager] ShowModelForArtifact called for artifact_id: '{id}'");

        // look up the model in the registry
        GameObject model = (modelRegistry != null) ? modelRegistry.GetModel(id) : null;

        if (model != null)
        {
            model.SetActive(true);

            Debug.Log(
                $"[ArtifactAnalysisManager] Activated model '{model.name}' " +
                $"activeSelf={model.activeSelf}, activeInHierarchy={model.activeInHierarchy}, " +
                $"parent={(model.transform.parent != null ? model.transform.parent.name : "none")}"
            );

            CurrentActiveModel = model;
            SetupPivotWithoutMovingModel(CurrentActiveModel);
            return;
        }

        // fallback if no match
        Debug.LogWarning($"[ArtifactAnalysisManager] No model found for artifact_id '{id}'. Using default model.");
        
        if (defaultModel != null)
        {
            defaultModel.SetActive(true);
            CurrentActiveModel = defaultModel;
            SetupPivotWithoutMovingModel(CurrentActiveModel);
        }
    }

    private void SetupPivotWithoutMovingModel(GameObject model)
    {
        if (artifactPivot == null || model == null) return;

        // find visual center of model in world space
        var renderers = model.GetComponentsInChildren<Renderer>();
        
        if (renderers.Length == 0) return;

        Bounds bound = renderers[0].bounds;

        for (int index = 1; index < renderers.Length; index++)
        {
            bound.Encapsulate(renderers[index].bounds);
        }

        // put pivot at model center
        artifactPivot.position = bound.center;

        // reset artifact rotation so each artifact starts upright
        artifactPivot.rotation = Quaternion.identity;

        // parent the model to the pivot
        model.transform.SetParent(artifactPivot, worldPositionStays: true);
    }

    // setup analysis scene if launched from excavation
    private void SetupFromExcavationLaunch()
    {
        Debug.Log(
            $"[ArtifactAnalysisManager] SetupFromExcavationLaunch() called. " +
            $"ArtifactId={ArtifactAnalysisLaunchContext.ArtifactId}, " +
            $"HasUserSubmission={ArtifactAnalysisLaunchContext.UserSubmission != null}"
        );

        currentUserSubmission = ArtifactAnalysisLaunchContext.UserSubmission;
        currentAnswerKey = null;
        currentArtifactId = ArtifactAnalysisLaunchContext.ArtifactId;

        if (SelectArtifact != null)
        {
            SelectArtifact.gameObject.SetActive(false);
        }

        bool hasUserSubmission = currentUserSubmission != null;

        if (AnswerKeyButton != null)
        {
            AnswerKeyButton.SetActive(hasUserSubmission);
        }

        if (UserSubmissionButton != null)
        {
            UserSubmissionButton.SetActive(hasUserSubmission);
        }

        if (ArtifactAnalysisLaunchContext.StartMode == ArtifactAnalysisStart.UserSubmissionFromExcavation &&
            currentUserSubmission != null)
        {
            PopulateUI(currentUserSubmission);
            SetViewMode("User Submission");
            SetStatus(DataLoaded);
        }

        else
        {
            StartCoroutine(LoadAnswerKeyCoroutine(currentArtifactId));
        }
    }

    // answer key button logic to switch from user submission to reference
    public void OnAnswerKeyButtonClicked()
    {
        if (string.IsNullOrWhiteSpace(currentArtifactId))
        {
            SetStatus(InvalidSelection);
            return;
        }

        if (currentAnswerKey != null)
        {
            PopulateUI(currentAnswerKey);
            SetViewMode("Answer Key");
            SetStatus(DataLoaded);
            return;
        }

        StartCoroutine(LoadAnswerKeyCoroutine(currentArtifactId));
    }

    // user submission button logic to switch from reference to user submission
    public void OnUserSubmissionButtonClicked()
    {
        if (currentUserSubmission == null)
        {
            SetStatus(NoArtifactData);
            return;
        }

        PopulateUI(currentUserSubmission);
        SetViewMode("User Submission");
        SetStatus(DataLoaded);
    }

    // continue button to return to excavation scene
    // ONLY VISIBLE WHEN ANALYSIS IS LAUNCHED FROM EXCAVATION
    public void OnContinueToExcavationButtonClicked()
    {
        ArtifactFormManager formManager = FindFirstObjectByType<ArtifactFormManager>();

        if (formManager != null)
        {
            formManager.RestoreAfterAnalysisScene();
        }

        ArtifactAnalysisLaunchContext.Clear();

        SceneManager.UnloadSceneAsync(gameObject.scene);
    }

    // coroutine to load answer keys based on user submission artifactId
    // similar to the coroutine to auto-fill collection form
    private IEnumerator LoadAnswerKeyCoroutine(string artifactId)
    {
        Debug.Log($"[ArtifactAnalysisManager] LoadAnswerKeyCoroutine() started for artifactId='{artifactId}'");

        if (string.IsNullOrWhiteSpace(artifactId))
        {
            SetStatus(InvalidSelection);
            yield break;
        }

        string url = $"{apiUrl}/reference/{UnityWebRequest.EscapeURL(artifactId)}";

        Debug.Log($"[ArtifactAnalysisManager] Loading answer key from: {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 10;

            yield return request.SendWebRequest();

            Debug.Log(
                $"[ArtifactAnalysisManager] Answer key request finished. " +
                $"Result={request.result}, Code={request.responseCode}, Text={request.downloadHandler.text}"
            );

            if (request.result != UnityWebRequest.Result.Success)
            {
                string serverText = request.downloadHandler != null
                    ? request.downloadHandler.text
                    : "";

                Debug.LogError(
                    $"Error fetching answer key: " +
                    $"{request.responseCode} - {request.error} - {serverText}");

                SetStatus(ErrorLoadingList);
                yield break;
            }

            SingleArtifactResponse response =
                JsonUtility.FromJson<SingleArtifactResponse>(request.downloadHandler.text);

            if (response == null || !response.ok || response.artifact == null)
            {
                SetStatus(NoArtifactData);
                yield break;
            }

            currentAnswerKey = response.artifact;
            PopulateUI(currentAnswerKey);
            SetViewMode("Answer Key");
            SetStatus(DataLoaded);
        }
    }

    private void SetViewMode(string label)
    {
        if (ViewModeText != null)
        {
            ViewModeText.text = label;
        }
    }
}
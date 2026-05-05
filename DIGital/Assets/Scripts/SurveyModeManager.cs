using UnityEngine;

// handles entering and exiting survey mode
public class SurveyModeManager : MonoBehaviour
{
    public static SurveyModeManager Instance { get; private set; }

    [Header("Camera Roots")]
    [SerializeField] private GameObject playerCameraRoot;               // first person camera
    [SerializeField] private GameObject surveyRigRoot;                  // drone camera
    [SerializeField] private SurveyCameraController surveyController;   // controller for drone camera
    [SerializeField] private Transform surveyStartFocusPoint;           // game object at center of site

    [Header("Disable while survey mode is active")]
    [SerializeField] private MonoBehaviour[] playerScriptsToDisable;    // look/movement scripts

    // cursor states
    [Header("Cursor")]
    [SerializeField] private bool unlockCursorInSurvey = true;
    [SerializeField] private bool showCursorInSurvey = true;
    [SerializeField] private bool relockCursorOnExit = true;
    [SerializeField] private bool hideCursorOnExit = true;

    // instruction bar
    [Header("UI")]
    [SerializeField] private GameObject droneInstructionBar;

    // reference to artifact collection form
    [SerializeField] private ArtifactFormManager artifactFormManager;

    public bool IsSurveyModeActive { get; private set; }

    public static bool IsSurveyActive
    {
        get
        {
            return Instance != null && Instance.IsSurveyModeActive;
        }
    }

    private CursorLockMode previousLockState;
    private bool previousCursorVisible;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (surveyRigRoot != null)
        {
            surveyRigRoot.SetActive(false);
        }

        if (droneInstructionBar != null)
        {
            droneInstructionBar.SetActive(false);
        }
    }

    // enter survey mode
    public void EnterSurveyMode()
    {
        if (IsSurveyModeActive)
        {
            return;
        }

        // safeguard so survey mode cannot start while intro is still up.
        if (WalkExcavateIntroController.IsIntroOpen)
        {
            return;
        }

        IsSurveyModeActive = true;

        // set cursor states
        previousLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        // disable player scripts
        SetScriptsEnabled(playerScriptsToDisable, false);

        if (playerCameraRoot != null)
        {
            playerCameraRoot.SetActive(false);
        }

        if (surveyStartFocusPoint != null && surveyController != null)
        {
            surveyController.SetFocusPoint(surveyStartFocusPoint.position);
        }

        if (surveyRigRoot != null)
        {
            surveyRigRoot.SetActive(true);
        }

        if (droneInstructionBar != null)
        {
            droneInstructionBar.SetActive(true);
        }

        if (surveyController != null)
        {
            surveyController.BeginSurveyMode(this);
        }

        if (unlockCursorInSurvey)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        Cursor.visible = showCursorInSurvey;
    }

    // exit survey mode
    public void ExitSurveyMode()
    {
        if (!IsSurveyModeActive)
        {
            return;
        }

        if (artifactFormManager != null && artifactFormManager.IsFormOpen)
        {
            return;
        }

        IsSurveyModeActive = false;

        if (surveyRigRoot != null)
        {
            surveyRigRoot.SetActive(false);
        }

        if (droneInstructionBar != null)
        {
            droneInstructionBar.SetActive(false);
        }

        if (playerCameraRoot != null)
        {
            playerCameraRoot.SetActive(true);
        }

        // enable scripts
        SetScriptsEnabled(playerScriptsToDisable, true);

        // set cursor states
        if (relockCursorOnExit)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = previousLockState;
        }

        Cursor.visible = hideCursorOnExit ? false : previousCursorVisible;
    }

    // enable/disable scripts
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
}

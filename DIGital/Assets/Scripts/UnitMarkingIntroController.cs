using UnityEngine;
using UnityEngine.UI;

public class UnitMarkingIntroController : MonoBehaviour
{
    public static bool IsIntroOpen { get; private set; }
    public static UnitMarkingIntroController Instance { get; private set; }

    [Header("Popup")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private Button continueButton;

    [Header("Disable while popup is open")]
    [SerializeField] private MonoBehaviour[] gameplayScriptsToDisable;
    [SerializeField] private MonoBehaviour[] uiScriptsToDisable;

    [Header("Cursor")]
    [SerializeField] private bool relockCursorOnClose = true;
    [SerializeField] private bool hideCursorOnClose = true;

    private void Awake()
    {
        Instance = this;
        if (continueButton != null)
            continueButton.onClick.AddListener(CloseIntro);
    }

    public void OpenIntro()
    {
        IsIntroOpen = true;

        GameObject introCanvas = GameObject.Find("IntroCanvas");
        if (introCanvas != null) introCanvas.SetActive(false);

        // Disable both the controller AND the input handler
        var fpc = FindFirstObjectByType<FirstPersonController>();
        if (fpc != null) fpc.enabled = false;

        var ph = FindFirstObjectByType<PlayerHandler>();
        if (ph != null) ph.enabled = false;

        if (popupRoot != null) popupRoot.SetActive(true);
        SetScriptsEnabled(gameplayScriptsToDisable, false);
        SetScriptsEnabled(uiScriptsToDisable, false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseIntro()
    {
        IsIntroOpen = false;
        if (popupRoot != null) popupRoot.SetActive(false);
        SetScriptsEnabled(gameplayScriptsToDisable, true);
        SetScriptsEnabled(uiScriptsToDisable, true);
        if (relockCursorOnClose) Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = !hideCursorOnClose;

        // Re-enable both
        var fpc = FindFirstObjectByType<FirstPersonController>();
        if (fpc != null) fpc.enabled = true;

        var ph = FindFirstObjectByType<PlayerHandler>();
        if (ph != null) ph.enabled = true;

        if (UnitMarkerSystem.Instance != null)
            UnitMarkerSystem.Instance.EnterStakePlacingMode();
    }

    private void SetScriptsEnabled(MonoBehaviour[] scripts, bool enabledState)
    {
        if (scripts == null) return;
        foreach (MonoBehaviour script in scripts)
            if (script != null) script.enabled = enabledState;
    }

    private void Update()
    {
        if (!IsIntroOpen) return;
    
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            CloseIntro();
    }
}
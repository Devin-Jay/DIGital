using UnityEngine;
using UnityEngine.EventSystems;

// class
public class ArtifactClickManager : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera surveyCamera;
    [SerializeField] private SurveyModeManager surveyModeManager;

    [Header("Raycast")]
    [SerializeField] private float maxDistance = 100f;

    [Tooltip("Layers that contain clickable artifacts only.")]
    [SerializeField] private LayerMask artifactLayerMask;

    [Tooltip("Layers that represent dirt/soil that should block clicking artifacts.")]
    [SerializeField] private LayerMask dirtLayerMask;

    [Header("Tooltip")]
    [SerializeField] private bool showTooltipFromThisManager = true;

    private void Update()
    {
        // block recording in drone POV
        if (SurveyModeManager.IsSurveyActive)
        {
            return;
        }

        // block when intro is open
        if (WalkExcavateIntroController.IsIntroOpen)
        {
            return;
        }

        // change between person and drone camera
        Camera activeCamera = GetActiveCamera();

        if (activeCamera == null)
        {
            return;
        }

        Ray ray = BuildRay(activeCamera);

        // find closest artifact hit
        if (!Physics.Raycast(ray, out RaycastHit artifactHit, maxDistance, artifactLayerMask)) return;

        // check if dirt blocks artifact
        if (Physics.Raycast(ray, out RaycastHit dirtHit, maxDistance, dirtLayerMask))
        {
            if (dirtHit.distance < artifactHit.distance)
            {
                return;
            }
        }

        ArtifactInteractable interactable = artifactHit.collider.GetComponentInParent<ArtifactInteractable>();

        // interact
        if (interactable == null)
        {
            return;
        }

        // tooltip while hovering
        if (showTooltipFromThisManager)
        {
            interactable.displayTooltip();
        }

        // don't click thru UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // if left click is down
        if (Input.GetMouseButtonDown(0))
        {
            interactable.Interact();
        }
    }

    // decide which camera to use based on SurveyModeManager
    private Camera GetActiveCamera()
    {
        if (surveyModeManager != null && surveyModeManager.IsSurveyModeActive)
        {
            return surveyCamera;
        }

        return playerCamera;
    }

    // decide where camera is centered (mouse vs. center)
    private Ray BuildRay(Camera activeCamera)
    {
        bool usingSurveyMode = surveyModeManager != null && surveyModeManager.IsSurveyModeActive;

        if (usingSurveyMode)
        {
            // drone view: cursor is unlocked, so click from mouse position.
            return activeCamera.ScreenPointToRay(Input.mousePosition);
        }

        // player view: cast from the center of the screen.
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        return activeCamera.ScreenPointToRay(screenCenter);
    }
}

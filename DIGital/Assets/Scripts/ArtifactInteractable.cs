using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class ArtifactInteractable : MonoBehaviour, Interactable
{
    [Header("UI / Form")]
    [SerializeField] private TMP_Text textBox;
    [SerializeField] private ArtifactFormManager formManager;

    [Header("Artifact Data")]
    [SerializeField] private string artifactId;
    public string ArtifactId => artifactId;

    [Header("Artifact Visuals")]
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private Collider[] collidersToDisable;

    [Header("Recorded Marker")]
    [SerializeField] private GameObject redFlagPrefab;
    [SerializeField] private Transform flagSpawnPoint;

    [Header("Localization")]
    [SerializeField] private string table = "UI";
    [SerializeField] private string tooltipKey = "walk_and_excavate_record_artifact";

    [Header("Survey Progress")]
    [SerializeField] private GameManager manager;
    [SerializeField] private bool countsTowardSurfaceSurvey = true;

    private float textDisplayedTime = 0f;
    private bool checkForText = false;
    private bool isRecorded = false;
    private GameObject spawnedFlag = null;

    public void Interact()
    {
        if (isRecorded)
        {
            return;
        }

        if (formManager != null)
        {
            Debug.Log("[ArtifactInteractable] Interact() called, opening form.");

            if (textBox != null)
            {
                textBox.text = "";
            }

            // pass artifact into form manager
            formManager.OpenForm(this);
        }

        else
        {
            Debug.LogWarning("[ArtifactInteractable] formManager is not assigned on "
                             + gameObject.name);
        }
    }

    public void displayTooltip()
    {
        if (isRecorded)
        {
            return;
        }

        // Called by FirstPersonController when this is the target
        textDisplayedTime = 0.03f;
        checkForText = true;
    }

    private void Update()
    {
        if (!checkForText)
        {
            return;
        }

        if (textDisplayedTime > 0f)
        {
            if (textBox != null)
            {
                textBox.text = LocalizationSettings.StringDatabase.GetLocalizedString(table, tooltipKey);
            }
        }

        else
        {
            if (textBox != null)
            {
                textBox.text = "";
            }
            checkForText = false;
        }

        textDisplayedTime -= Time.deltaTime;
    }

    // hide artifact after data collection
    public void MarkAsRecorded()
    {
        if (isRecorded)
        {
            return;
        }

        isRecorded = true;

        if (textBox != null )
        {
            textBox.text = "";
        }

        checkForText = false;

        // spawn pawn red flag first
        if (redFlagPrefab != null && spawnedFlag == null)
        {
            // Vector3 spawnPosition = flagSpawnPoint != null ? flagSpawnPoint.position : transform.position;
            // Quaternion spawnRotation = flagSpawnPoint != null ? flagSpawnPoint.rotation : Quaternion.identity;

            // spawnedFlag = Instantiate(redFlagPrefab,flagSpawnPoint);
            spawnedFlag = Instantiate(redFlagPrefab, flagSpawnPoint.position, redFlagPrefab.transform.rotation);


            if (manager != null)
            {
                manager.RegisterSpawnedSurveyFlag(spawnedFlag);
            }
        }

        // then hide artifact visuals
        if (visualRoot != null)
        {
            visualRoot.SetActive(false);
        }

        else
        {
            // hide all renderers if no visualRoot assigned
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer rend in renderers)
            {
                rend.enabled = false;
            }
        }

        // disable colliders so it cannot be clicked again
        if (collidersToDisable != null && collidersToDisable.Length > 0)
        {
            foreach (Collider col in collidersToDisable)
            {
                if (col != null)
                {
                    col.enabled = false;
                }
            }
        }

        else
        {
            Collider[] allColliders = GetComponentsInChildren<Collider>(true);
            foreach (Collider col in allColliders)
            {
                col.enabled = false;
            }
        }

        // record artifact last
        if (countsTowardSurfaceSurvey && manager != null)
        {
            manager.RegisterSurfaceArtifactRecorded();
        }

        // TODO: @TATE any reason we don't destroy the artifact?
        // Destroy(gameObject);
    }
}
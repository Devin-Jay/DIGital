using UnityEngine;
using UnityEngine.Rendering;

public class ArtifactOutline : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private float outlineWidth = 0.03f;
    [SerializeField] private Color outlineColor = Color.red;

    [Header("Setup")]
    [SerializeField] private bool createOnStart = true;

    // add outline on start
    private void Start()
    {
        if (createOnStart)
        {
            CreateOutline();
        }
    }

    // create outline
    private void CreateOutline()
    {
        // error handle
        if (outlineMaterial == null)
        {
            Debug.LogWarning($"{name}: No outline material assigned.");
            return;
        }

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter sourceMeshFilter in meshFilters)
        {
            // don't create outlines for copies
            if (sourceMeshFilter.gameObject.name.Contains("_Outline"))
            {
                continue;
            }

            MeshRenderer sourceRenderer = sourceMeshFilter.GetComponent<MeshRenderer>();

            if (sourceRenderer == null)
            {
                continue;
            }

            GameObject outlineObject = new GameObject(sourceMeshFilter.gameObject.name + "_Outline");

            // parent the outline to the same object as the original mesh
            outlineObject.transform.SetParent(sourceMeshFilter.transform);

            // match the original mesh transform
            outlineObject.transform.localPosition = Vector3.zero;
            outlineObject.transform.localRotation = Quaternion.identity;
            outlineObject.transform.localScale = Vector3.one;

            // Copcopyy the mesh
            MeshFilter outlineMeshFilter = outlineObject.AddComponent<MeshFilter>();
            outlineMeshFilter.sharedMesh = sourceMeshFilter.sharedMesh;

            MeshRenderer outlineRenderer = outlineObject.AddComponent<MeshRenderer>();

            // give  copy the outline material
            Material outlineInstance = new Material(outlineMaterial);
            outlineInstance.SetColor("_OutlineColor", outlineColor);
            outlineInstance.SetFloat("_OutlineWidth", outlineWidth);

            outlineRenderer.sharedMaterial = outlineInstance;

            outlineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            outlineRenderer.receiveShadows = false;
        }
    }
}
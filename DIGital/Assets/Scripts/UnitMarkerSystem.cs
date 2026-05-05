using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class UnitMarkerSystem : MonoBehaviour
{
    public static UnitMarkerSystem Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] private float gridSize = 5f;
    [SerializeField] private float snapRadius = 3f;
    [SerializeField] private float interactDistance = 10f;
    [SerializeField] private float surfaceYOffset = 0f;

    [Header("Stake Visuals")]
    [SerializeField] private float stakeHeight = 1.2f;
    [SerializeField] private float stakeBaseRadius = 0.07f;
    [SerializeField] private Color stakeColor = new Color(0.35f, 0.20f, 0.05f);

    [Header("Rope Visuals")]
    [SerializeField] private float ropeYOffset = 1.0f;
    [SerializeField] private float ropeWidth = 0.05f;
    [SerializeField] private Color ropeColor = new Color(1f, 0.45f, 0f);

    [Header("Layer Mask")]
    [SerializeField] private LayerMask dirtLayerMask;

    [Header("References")]
    [SerializeField] private FirstPersonController firstPersonController;
    [SerializeField] private GameObject stakePrefab;

    [Header("Unit Label UI")]
    [SerializeField] private GameObject unitLabelPanel;
    [SerializeField] private TextMeshProUGUI unitLabelText;

    [Header("Intro Popup")]
    [SerializeField] private UnitMarkingIntroController introController;

    // ── State ────────────────────────────────────────────────────────────────
    public bool IsActive { get; private set; } = false;
    public bool CanPlayerDig => IsActive && activeUnits.Count > 0;
    public bool IsPlacingStakes { get; private set; } = false;

    private readonly List<Vector2Int> pendingCorners = new List<Vector2Int>();
    private readonly List<GameObject> pendingStakes  = new List<GameObject>();
    private readonly List<ExcavationUnit> activeUnits = new List<ExcavationUnit>();

    private GameObject previewStake;
    private Material ropeMaterial;
    private Material stakeMaterial;

    private Vector3 gridOrigin;
    private bool gridOriginSet = false;
    private readonly HashSet<Vector2Int> validCorners = new HashSet<Vector2Int>();

    private int unitCounter = 0;
    private bool hasUnstartedUnit = false;  

    // ── Unity ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        stakeMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        stakeMaterial.color = stakeColor;

        ropeMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        ropeMaterial.color = ropeColor;
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!IsPlacingStakes) return;
        if (UnitMarkingIntroController.IsIntroOpen) return;
        UpdatePreviewStake();
        if (Input.GetMouseButtonDown(0)) TryPlaceStake();
    }

    private void LateUpdate()
    {
        if (!IsActive) return;
    
        if (UnitMarkingIntroController.IsIntroOpen) return;
    
        if (!IsPlacingStakes && Input.GetKeyDown(KeyCode.U))
        {
            DiggableEarth[] remaining = FindObjectsByType<DiggableEarth>(FindObjectsSortMode.None);
            if (remaining.Length == 0) return;

            if (hasUnstartedUnit)
            {
                Debug.Log($"[UnitMarkerSystem] Dig Unit {unitCounter} before marking a new one.");
                return;
            }

            EnterStakePlacingMode();
        }
    }
   

    // ── Public API ───────────────────────────────────────────────────────────

    public void BeginUnitMarkingPhase()
    {
        gameObject.SetActive(true);
        IsActive = true;

        if (firstPersonController == null)
            firstPersonController = FindFirstObjectByType<FirstPersonController>();

        ScanGridFromScene();

        Debug.Log($"[UnitMarkerSystem] introController is: {(introController == null ? "NULL" : introController.name)}");

        if (introController != null)
        {
            introController.OpenIntro();
        }
        else
        {
            EnterStakePlacingMode();
        }

        Debug.Log($"[UnitMarkerSystem] Phase started. Grid origin: {gridOrigin}, Corners: {validCorners.Count}");
    }
    

    public void EnterStakePlacingMode()
    {
        IsPlacingStakes = true;

        if (firstPersonController != null)
            firstPersonController.IsUnitMarkingMode = true;

        if (previewStake == null && stakePrefab != null)
            previewStake = Instantiate(stakePrefab);
        previewStake.SetActive(false);

        Debug.Log("[UnitMarkerSystem] Stake placing mode ON. Aim at a grid corner and click.");
    }

    private void ExitStakePlacingMode()
    {
        IsPlacingStakes = false;

        if (firstPersonController != null)
            firstPersonController.IsUnitMarkingMode = false;

        if (previewStake != null) previewStake.SetActive(false);

        Debug.Log("[UnitMarkerSystem] Stake placing mode OFF. Dig your marked unit!");
    }

    private System.Collections.IEnumerator ExitStakePlacingNextFrame()
    {
        yield return null;
        ExitStakePlacingMode();
    }

    public bool IsSectionMarked(Vector3 sectionWorldPos)
    {
        Vector2Int cell = WorldToCell(sectionWorldPos);
        foreach (var unit in activeUnits)
            if (unit.ContainsCell(cell)) return true;
        return false;
    }

    public Vector2Int WorldToCellPublic(Vector3 worldPos) => WorldToCell(worldPos);

    public void NotifySectionFullyExcavated(Vector3 sectionWorldPos, int layerIndex)
    {
        Vector2Int cell = WorldToCell(sectionWorldPos);

        for (int i = activeUnits.Count - 1; i >= 0; i--)
        {
            ExcavationUnit unit = activeUnits[i];
            if (!unit.ContainsCell(cell)) continue;

            bool anyColumnRemaining = AnyColumnRemainingInEntireUnit(unit, sectionWorldPos);

            Debug.Log($"[UnitMarkerSystem] Cell {cell} dug at layer {layerIndex}. Any column remaining in unit: {anyColumnRemaining}");

            if (!anyColumnRemaining)
            {   
            int deepestLayer = unit.GetDeepestLayerIndex();
                unit.Despawn();
                activeUnits.RemoveAt(i);
                hasUnstartedUnit = false;
                if (unitLabelPanel != null) unitLabelPanel.SetActive(false);
                Debug.Log($"[UnitMarkerSystem] Unit fully excavated — deepest layer was {deepestLayer}. Stakes removed.");

                // Ask a random knowledge check question
                KnowledgeCheck.instance.AskRandomQuestion();
            }
            break;
        }
    }

    private bool AnyColumnRemainingInEntireUnit(ExcavationUnit unit, Vector3 excludeWorldPos)
    {
        DiggableEarth[] allSections = FindObjectsByType<DiggableEarth>(FindObjectsSortMode.None);
        foreach (var cell in unit.Cells)
        {
            foreach (var section in allSections)
            {
                if (Vector3.Distance(section.transform.position, excludeWorldPos) < 0.01f) continue;
                if (WorldToCell(section.transform.position) == cell) return true;
            }
        }
        return false;
    }
    
    // ── Grid scanning ─────────────────────────────────────────────────────────

    private void ScanGridFromScene()
    {
        validCorners.Clear();
        gridOriginSet = false;

        DiggableEarthLayer[] layers = FindObjectsByType<DiggableEarthLayer>(FindObjectsSortMode.None);

        float highestY = float.MinValue;
        DiggableEarthLayer topLayer = null;
        foreach (var layer in layers)
        {
            if (layer.transform.position.y > highestY)
            {
                highestY = layer.transform.position.y;
                topLayer = layer;
            }
        }

        if (topLayer == null || topLayer.transform.childCount == 0)
        {
            Debug.LogWarning("[UnitMarkerSystem] Could not find top layer!");
            return;
        }

        Vector3 first = topLayer.transform.GetChild(0).position;
        gridOrigin = new Vector3(
            first.x - gridSize * 0.5f,
            highestY + surfaceYOffset,
            first.z - gridSize * 0.5f
        );
        gridOriginSet = true;

        DiggableEarth[] allSections = FindObjectsByType<DiggableEarth>(FindObjectsSortMode.None);
        if (allSections.Length == 0)
        {
            Debug.LogWarning("[UnitMarkerSystem] No DiggableEarth found!");
            return;
        }

        foreach (var section in allSections)
        {
            Vector2Int cell = WorldToCell(section.transform.position);
            validCorners.Add(new Vector2Int(cell.x,     cell.y));
            validCorners.Add(new Vector2Int(cell.x + 1, cell.y));
            validCorners.Add(new Vector2Int(cell.x,     cell.y + 1));
            validCorners.Add(new Vector2Int(cell.x + 1, cell.y + 1));
        }

        Debug.Log($"[UnitMarkerSystem] Scanned {allSections.Length} sections, {validCorners.Count} corners. Surface Y={gridOrigin.y}, Origin={gridOrigin}");
    }

    // ── Stake placement ──────────────────────────────────────────────────────

    private void UpdatePreviewStake()
    {
        if (previewStake == null) return;
        Vector2Int? corner = GetNearestCornerInCrosshair();
        if (corner.HasValue && !IsDuplicateCorner(corner.Value))
        {
            previewStake.SetActive(true);
            previewStake.transform.position = CellCornerToWorld(corner.Value);
        }
        else
        {
            previewStake.SetActive(false);
        }
    }

    private void TryPlaceStake()
    {
        Vector2Int? corner = GetNearestCornerInCrosshair();
        if (!corner.HasValue || IsDuplicateCorner(corner.Value)) return;
        if (pendingCorners.Count >= 4) return;

        pendingCorners.Add(corner.Value);
        GameObject stake = Instantiate(stakePrefab, CellCornerToWorld(corner.Value), Quaternion.identity);
        pendingStakes.Add(stake);

        Debug.Log($"[UnitMarkerSystem] Stake {pendingCorners.Count}/4 at corner {corner.Value} = world {CellCornerToWorld(corner.Value)}");
        TryConfirmUnit();
    }

    private void TryConfirmUnit()
    {
        if (pendingCorners.Count < 2) return;

        // Only use the first 2 corners placed
        Vector2Int c0 = pendingCorners[0];
        Vector2Int c1 = pendingCorners[1];

        int minX = Mathf.Min(c0.x, c1.x);
        int maxX = Mathf.Max(c0.x, c1.x);
        int minZ = Mathf.Min(c0.y, c1.y);
        int maxZ = Mathf.Max(c0.y, c1.y);

        // Must be diagonally opposite 
        if (minX == maxX || minZ == maxZ)
        {
            Debug.Log("[UnitMarkerSystem] Stakes are in the same row or column — cancelling. Place a diagonal corner.");
            CancelPending();
            return;
        } 

        // Collect cells in the rectangle
        List<Vector2Int> cells = new List<Vector2Int>();
        for (int x = minX; x < maxX; x++)
            for (int z = minZ; z < maxZ; z++)
                cells.Add(new Vector2Int(x, z));

    if (cells.Count == 0) return;

        // Overlap check
        foreach (var cell in cells)
            foreach (var existing in activeUnits)
                if (existing.ContainsCell(cell)) { CancelPending(); return; }

        ConfirmUnit(cells, minX, maxX, minZ, maxZ);
    }

    private void ConfirmUnit(List<Vector2Int> cells, int minX, int maxX, int minZ, int maxZ)
    {
        ClearPendingStakes();

        var corners = new Vector2Int[]
        {
            new Vector2Int(minX, minZ),
            new Vector2Int(maxX, minZ),
            new Vector2Int(maxX, maxZ),
            new Vector2Int(minX, maxZ),
        };

        Vector3 unitCenter = new Vector3(
            gridOrigin.x + (minX + maxX) * 0.5f * gridSize,
            gridOrigin.y,
            gridOrigin.z + (minZ + maxZ) * 0.5f * gridSize
        );

        float stakeInset = 0.25f; // tweak this in code if needed

        List<GameObject> stakes = new List<GameObject>();
        foreach (var c in corners)
        {
            Vector3 cornerWorld = CellCornerToWorld(c);
            Vector3 toCenter = (unitCenter - cornerWorld);
            toCenter.y = 0f;
            Vector3 stakePos = cornerWorld - toCenter.normalized * stakeInset;
            GameObject s = Instantiate(stakePrefab, stakePos, Quaternion.identity);
            stakes.Add(s);
        }

        Vector3[] ropePoints = new Vector3[5];
        for (int i = 0; i < 4; i++)
            ropePoints[i] = stakes[i].transform.position + Vector3.up * ropeYOffset;
        ropePoints[4] = ropePoints[0];

        GameObject ropeGO = new GameObject("UnitRope");
        LineRenderer lr = ropeGO.AddComponent<LineRenderer>();
        lr.positionCount = 5;
        lr.SetPositions(ropePoints);
        lr.startWidth = ropeWidth;
        lr.endWidth   = ropeWidth;
        lr.material   = ropeMaterial;
        lr.useWorldSpace = true;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Dictionary<Vector2Int, int> maxDepths = BuildMaxDepthMap(cells);
        activeUnits.Add(new ExcavationUnit(cells, stakes, ropeGO, maxDepths));
        pendingCorners.Clear();

        unitCounter++;
        hasUnstartedUnit = true;
        if (unitLabelPanel != null) unitLabelPanel.SetActive(true);
        if (unitLabelText != null) unitLabelText.text = $"Unit {unitCounter}";

        StartCoroutine(ExitStakePlacingNextFrame());

        Debug.Log($"[UnitMarkerSystem] Unit confirmed — {cells.Count} cell(s). Dig it! Press U to mark another.");
    }

    private void CancelPending()
    {
        foreach (var s in pendingStakes) if (s) Destroy(s);
        pendingStakes.Clear();
        pendingCorners.Clear();
    }

    private void ClearPendingStakes()
    {
        foreach (var s in pendingStakes) if (s) Destroy(s);
        pendingStakes.Clear();
    }

    // ── Grid helpers ─────────────────────────────────────────────────────────

    private Vector2Int WorldToCell(Vector3 worldPos)
    {
        if (!gridOriginSet)
            return new Vector2Int(Mathf.FloorToInt(worldPos.x / gridSize), Mathf.FloorToInt(worldPos.z / gridSize));
        int cx = Mathf.FloorToInt((worldPos.x - gridOrigin.x) / gridSize);
        int cz = Mathf.FloorToInt((worldPos.z - gridOrigin.z) / gridSize);
        return new Vector2Int(cx, cz);
    }

    private Vector3 CellCornerToWorld(Vector2Int corner)
    {
        return new Vector3(
            gridOrigin.x + corner.x * gridSize,
            gridOrigin.y,
            gridOrigin.z + corner.y * gridSize);
    }

    private Vector2Int? GetNearestCornerInCrosshair()
    {
        Camera cam = Camera.main;
        if (cam == null || !gridOriginSet) return null;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, dirtLayerMask)) return null;

        Vector3 hp = hit.point;
        Vector2Int? best = null;
        float bestDist = float.MaxValue;

        foreach (var corner in validCorners)
        {
            Vector3 cWorld = CellCornerToWorld(corner);
            float dist = Vector2.Distance(new Vector2(hp.x, hp.z), new Vector2(cWorld.x, cWorld.z));
            if (dist < snapRadius && dist < bestDist) { bestDist = dist; best = corner; }
        }
        return best;
    }

    private bool IsDuplicateCorner(Vector2Int corner) => pendingCorners.Contains(corner);

    private Dictionary<Vector2Int, int> BuildMaxDepthMap(List<Vector2Int> cells)
    {
        var map = new Dictionary<Vector2Int, int>();
        DiggableEarthLayer[] allLayers = FindObjectsByType<DiggableEarthLayer>(FindObjectsSortMode.None);
        foreach (var layer in allLayers)
            foreach (Transform child in layer.transform)
            {
                Vector2Int cell = WorldToCell(child.position);
                if (!cells.Contains(cell)) continue;
                if (!map.ContainsKey(cell) || layer.digLayer < map[cell])
                    map[cell] = layer.digLayer;
            }
        foreach (var cell in cells)
            if (!map.ContainsKey(cell)) map[cell] = 0;
        return map;
    }
}
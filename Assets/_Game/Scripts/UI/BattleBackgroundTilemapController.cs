using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

/// <summary>
/// Positions the active authored battle-environment prefab in world space.
/// Tile cell size and prefab transform scale remain authored at 1:1.
/// </summary>
[ExecuteAlways]
[DefaultExecutionOrder(100)]
[DisallowMultipleComponent]
public sealed class BattleBackgroundTilemapController : MonoBehaviour
{
    private const string DefaultEnvironmentResourcePath =
        "BattleEnvironments/Dungeon_Default";

    [SerializeField] private RectTransform battleFloorAnchor;
    [SerializeField] private RectTransform battleArea;
    [SerializeField] private Camera worldCamera;

    private SpriteMask viewportMask;
    private Sprite maskSprite;
    private Texture2D maskTexture;
    private BattleEnvironmentRoot activeEnvironment;

    private Tilemap[] cachedTilemaps = System.Array.Empty<Tilemap>();
    private TilemapRenderer[] cachedRenderers = System.Array.Empty<TilemapRenderer>();
    private bool[] cachedValidTileContent = System.Array.Empty<bool>();
    private bool tileContentCacheDirty = true;

    public BattleEnvironmentRoot ActiveEnvironment => activeEnvironment;

    private void Awake()
    {
        EnsureEnvironmentInstance();
        CacheTilemapHierarchy();
    }

    private void Start()
    {
        // Register after PPC's OnEnable so its final projection is authoritative.
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RegisterTileChangeListener();
    }

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RegisterTileChangeListener();
        EnsureEnvironmentInstance();
        CacheTilemapHierarchy();
        MarkTileContentDirty();
        Align();
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        Tilemap.tilemapTileChanged -= OnTilemapTileChanged;
        SetRenderingSuppressed(true);
        if (viewportMask != null) viewportMask.enabled = false;
    }

    private void OnValidate()
    {
        ResolveEnvironmentInstance();
        CacheTilemapHierarchy();
        MarkTileContentDirty();
    }

    private void OnTransformChildrenChanged()
    {
        ResolveEnvironmentInstance();
        CacheTilemapHierarchy();
        MarkTileContentDirty();
    }

    /// <summary>
    /// Uses whichever BattleEnvironmentRoot is currently placed beneath this
    /// controller. If a scene has none, the default Resources prefab is created
    /// once. In the editor it is created as a real prefab instance so artists can
    /// open the source prefab in Prefab Mode and paint at natural Tilemap scale.
    /// </summary>
    private void EnsureEnvironmentInstance()
    {
        ResolveEnvironmentInstance();
        if (activeEnvironment != null) return;

        GameObject prefab = Resources.Load<GameObject>(DefaultEnvironmentResourcePath);
        if (prefab == null)
        {
            Debug.LogWarning(
                $"Battle background default environment was not found at Resources/{DefaultEnvironmentResourcePath}.",
                this);
            return;
        }

        if (prefab.GetComponent<BattleEnvironmentRoot>() == null)
        {
            Debug.LogError(
                $"Battle environment prefab '{prefab.name}' is missing {nameof(BattleEnvironmentRoot)}.",
                prefab);
            return;
        }

        GameObject instance;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            instance = UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform)
                as GameObject;
        }
        else
#endif
        {
            instance = Instantiate(prefab, transform);
        }

        if (instance == null)
        {
            Debug.LogError("Failed to instantiate the default battle environment prefab.", this);
            return;
        }

        Transform instanceTransform = instance.transform;
        instanceTransform.localPosition = Vector3.zero;
        instanceTransform.localRotation = Quaternion.identity;
        instanceTransform.localScale = Vector3.one;
        activeEnvironment = instance.GetComponent<BattleEnvironmentRoot>();
        MarkTileContentDirty();
    }

    private void ResolveEnvironmentInstance()
    {
        BattleEnvironmentRoot[] environments =
            GetComponentsInChildren<BattleEnvironmentRoot>(true);

        activeEnvironment = null;
        BattleEnvironmentRoot inactiveFallback = null;

        foreach (BattleEnvironmentRoot environment in environments)
        {
            if (environment == null) continue;
            inactiveFallback ??= environment;
            if (!environment.gameObject.activeInHierarchy) continue;
            activeEnvironment = environment;
            return;
        }

        activeEnvironment = inactiveFallback;
    }

    // Render only when an active layer contains an actual 64-PPU sprite.
    // Tile contents are cached and invalidated by Tilemap's change event, so the
    // normal LateUpdate availability check never walks every painted cell.
    public bool TryUseBackground(RectTransform arena)
    {
        bool available = isActiveAndEnabled && battleArea != null && battleArea == arena &&
            battleFloorAnchor != null && worldCamera != null &&
            worldCamera.isActiveAndEnabled &&
            battleArea.GetComponentInParent<Canvas>() != null &&
            HasRenderableTiles();
        SetRenderingSuppressed(!available);
        return available;
    }

    private bool HasRenderableTiles()
    {
        RefreshTileContentCacheIfNeeded();

        for (int index = 0; index < cachedTilemaps.Length; index++)
        {
            Tilemap map = cachedTilemaps[index];
            TilemapRenderer renderer =
                index < cachedRenderers.Length
                    ? cachedRenderers[index]
                    : null;

            if (map == null || renderer == null ||
                !map.gameObject.activeInHierarchy ||
                !renderer.enabled || map.color.a <= 0f ||
                (worldCamera.cullingMask & (1 << map.gameObject.layer)) == 0)
            {
                continue;
            }

            if (index < cachedValidTileContent.Length &&
                cachedValidTileContent[index])
            {
                return true;
            }
        }

        return false;
    }

    private void RefreshTileContentCacheIfNeeded()
    {
        if (!tileContentCacheDirty &&
            cachedValidTileContent.Length == cachedTilemaps.Length)
        {
            return;
        }

        if (cachedValidTileContent.Length != cachedTilemaps.Length)
        {
            cachedValidTileContent = new bool[cachedTilemaps.Length];
        }

        for (int index = 0; index < cachedTilemaps.Length; index++)
        {
            cachedValidTileContent[index] =
                HasValid64PpuTile(cachedTilemaps[index]);
        }

        tileContentCacheDirty = false;
    }

    private static bool HasValid64PpuTile(Tilemap map)
    {
        if (map == null)
        {
            return false;
        }

        foreach (Vector3Int cell in map.cellBounds.allPositionsWithin)
        {
            Sprite sprite = map.GetSprite(cell);
            if (sprite != null && map.GetColor(cell).a > 0f &&
                Mathf.Approximately(
                    sprite.pixelsPerUnit,
                    GameplayPixelLayoutController.AssetsPPU))
            {
                return true;
            }
        }

        return false;
    }

    private void CacheTilemapHierarchy()
    {
        ResolveEnvironmentInstance();
        cachedTilemaps = activeEnvironment != null
            ? activeEnvironment.GetComponentsInChildren<Tilemap>(true)
            : GetComponentsInChildren<Tilemap>(true);
        cachedRenderers = new TilemapRenderer[cachedTilemaps.Length];

        for (int index = 0; index < cachedTilemaps.Length; index++)
        {
            if (cachedTilemaps[index] != null)
            {
                cachedTilemaps[index].TryGetComponent(
                    out cachedRenderers[index]);
            }
        }

        cachedValidTileContent = new bool[cachedTilemaps.Length];
        tileContentCacheDirty = true;
    }

    private void RegisterTileChangeListener()
    {
        Tilemap.tilemapTileChanged -= OnTilemapTileChanged;
        Tilemap.tilemapTileChanged += OnTilemapTileChanged;
    }

    private void OnTilemapTileChanged(
        Tilemap changedTilemap,
        Tilemap.SyncTile[] changes)
    {
        if (changedTilemap == null) return;

        Transform authoredRoot = activeEnvironment != null
            ? activeEnvironment.transform
            : transform;
        if (changedTilemap.transform == authoredRoot ||
            changedTilemap.transform.IsChildOf(authoredRoot))
        {
            MarkTileContentDirty();
        }
    }

    private void MarkTileContentDirty()
    {
        tileContentCacheDirty = true;
    }

    private void SetRenderingSuppressed(bool suppressed)
    {
        if (cachedRenderers.Length == 0 && transform.childCount > 0)
        {
            CacheTilemapHierarchy();
        }

        foreach (TilemapRenderer renderer in cachedRenderers)
        {
            if (renderer != null)
            {
                renderer.forceRenderingOff = suppressed;
            }
        }
    }

    private void LateUpdate() => Align();

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        // Re-project after the Pixel Perfect Camera has updated its projection.
        if (camera == worldCamera) Align();
    }

    private void Align()
    {
        if (battleFloorAnchor == null || battleArea == null || worldCamera == null) return;
        Canvas canvas = battleArea.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        canvas = canvas.rootCanvas;
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector3 floor = battleArea.InverseTransformPoint(battleFloorAnchor.position);
        // Map origin is the battle area's horizontal center, on the floor line.
        Vector3 uiPoint = battleArea.TransformPoint(new Vector3(battleArea.rect.center.x, floor.y, floor.z));
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, uiPoint);
        Ray ray = worldCamera.ScreenPointToRay(screenPoint);
        Plane mapPlane = new Plane(Vector3.forward, transform.position);
        if (!mapPlane.Raycast(ray, out float distance)) return;
        Vector3 position = ray.GetPoint(distance);
        const float ppu = GameplayPixelLayoutController.AssetsPPU;
        position.x = Mathf.Round(position.x * ppu) / ppu;
        position.y = Mathf.Round(position.y * ppu) / ppu;
        position.z = transform.position.z;
        if (transform.position != position) transform.position = position;
        // Share the snapped baseline with UI consumers instead of leaving a
        // sub-texel discrepancy between the marker and cell Y=0.
        Vector2 snappedScreen = worldCamera.WorldToScreenPoint(position);
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            battleArea, snappedScreen, uiCamera, out Vector3 snappedFloor))
        {
            Vector3 local = battleArea.InverseTransformPoint(battleFloorAnchor.position);
            local.y = battleArea.InverseTransformPoint(snappedFloor).y;
            battleFloorAnchor.position = battleArea.TransformPoint(local);
        }

        EnsureMask();
        Rect interior = battleArea.rect;
        float inset = GameplayPixelLayoutController.NativeFrameThickness;
        interior.min += Vector2.one * inset;
        interior.max -= Vector2.one * inset;
        Vector3 min = Project(interior.min, uiCamera, mapPlane);
        Vector3 max = Project(interior.max, uiCamera, mapPlane);
        viewportMask.transform.position = (min + max) * 0.5f;
        viewportMask.transform.localScale = new Vector3(
            Mathf.Max(0, max.x - min.x), Mathf.Max(0, max.y - min.y), 1);
        viewportMask.enabled = interior.width > 0 && interior.height > 0;
        foreach (TilemapRenderer renderer in cachedRenderers)
        {
            if (renderer != null)
            {
                renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            }
        }
    }

    private Vector3 Project(Vector2 point, Camera uiCamera, Plane plane)
    {
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCamera, battleArea.TransformPoint(point));
        Ray ray = worldCamera.ScreenPointToRay(screen);
        return plane.Raycast(ray, out float distance) ? ray.GetPoint(distance) : transform.position;
    }

    private void EnsureMask()
    {
        if (viewportMask != null) return;
        maskTexture = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave, filterMode = FilterMode.Point };
        maskTexture.SetPixel(0, 0, Color.white);
        maskTexture.Apply();
        maskSprite = Sprite.Create(maskTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1,
            0, SpriteMeshType.FullRect);
        maskSprite.hideFlags = HideFlags.HideAndDontSave;
        var child = new GameObject("BattleViewportMask") { hideFlags = HideFlags.HideAndDontSave };
        child.transform.SetParent(transform, false);
        viewportMask = child.AddComponent<SpriteMask>();
        viewportMask.sprite = maskSprite;
        viewportMask.alphaCutoff = 0.01f;
        // All four authored layers occupy Default/-100 through -70. Keep this
        // mask entirely below board layers without changing any renderer order.
        viewportMask.isCustomRangeActive = true;
        viewportMask.backSortingLayerID = SortingLayer.NameToID("Default");
        viewportMask.frontSortingLayerID = SortingLayer.NameToID("Default");
        viewportMask.backSortingOrder = -101;
        viewportMask.frontSortingOrder = -69;
    }

    private void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        Tilemap.tilemapTileChanged -= OnTilemapTileChanged;
        Release(viewportMask != null ? viewportMask.gameObject : null);
        Release(maskSprite);
        Release(maskTexture);
    }

    private static void Release(Object value)
    {
        if (value == null) return;
        if (Application.isPlaying) Destroy(value);
        else DestroyImmediate(value);
    }
}

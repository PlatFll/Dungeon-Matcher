using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

/// <summary>Positions a world-space map; cell size and transform scale stay authored.</summary>
[ExecuteAlways]
[DefaultExecutionOrder(100)]
[DisallowMultipleComponent]
public sealed class BattleBackgroundTilemapController : MonoBehaviour
{
    [SerializeField] private RectTransform battleFloorAnchor;
    [SerializeField] private RectTransform battleArea;
    [SerializeField] private Camera worldCamera;
    private SpriteMask viewportMask;
    private Sprite maskSprite;
    private Texture2D maskTexture;

    private void Start()
    {
        // Register after PPC's OnEnable so its final projection is authoritative.
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        Align();
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        SetRenderingSuppressed(true);
        if (viewportMask != null) viewportMask.enabled = false;
    }

    // The empty Phase-1 hierarchy is not an available background. Keep the
    // legacy UI fallback until an active layer contains an actual 64-PPU sprite.
    // This selects presentation only: no tiles, cell sizes or scales are changed.
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
        foreach (Tilemap map in GetComponentsInChildren<Tilemap>())
        {
            if (!map.TryGetComponent(out TilemapRenderer renderer) ||
                !renderer.enabled || map.color.a <= 0f ||
                (worldCamera.cullingMask & (1 << map.gameObject.layer)) == 0)
                continue;

            foreach (Vector3Int cell in map.cellBounds.allPositionsWithin)
            {
                Sprite sprite = map.GetSprite(cell);
                if (sprite != null && map.GetColor(cell).a > 0f &&
                    Mathf.Approximately(sprite.pixelsPerUnit, GameplayPixelLayoutController.AssetsPPU))
                    return true;
            }
        }
        return false;
    }

    private void SetRenderingSuppressed(bool suppressed)
    {
        foreach (TilemapRenderer renderer in GetComponentsInChildren<TilemapRenderer>(true))
            renderer.forceRenderingOff = suppressed;
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
        foreach (TilemapRenderer renderer in GetComponentsInChildren<TilemapRenderer>(true))
            renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
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

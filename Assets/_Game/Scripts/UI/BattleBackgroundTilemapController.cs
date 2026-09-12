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

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        Align();
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        SetRenderingSuppressed(true);
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
    }
}

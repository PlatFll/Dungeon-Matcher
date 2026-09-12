using UnityEngine;
using UnityEngine.Rendering;

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

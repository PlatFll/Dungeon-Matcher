using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(BoardController))]
public sealed class BoardLayoutController : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;
    [SerializeField] private RectTransform boardArea;
    private BoardVisuals visuals;
    private GameplayPixelLayoutController layout;
    public float PhysicalTexelRatio { get; private set; }
    public Vector2 PhysicalOrigin { get; private set; }

    private void Start()
    {
        if (worldCamera == null) worldCamera = Camera.main;
        visuals = GetComponent<BoardVisuals>();
        if (boardArea == null) return;
        layout = boardArea.GetComponentInParent<GameplayPixelLayoutController>();
        // PPC subscribes in OnEnable. Register in Start so our projection query
        // runs after PPC updates its matrix, including the first resized frame.
        RenderPipelineManager.beginCameraRendering += BeforeCamera;
    }

    private void OnDestroy() => RenderPipelineManager.beginCameraRendering -= BeforeCamera;

    private void BeforeCamera(ScriptableRenderContext context, Camera camera)
    {
        if (!isActiveAndEnabled || camera != worldCamera || boardArea == null || visuals == null || layout == null) return;
        if (!layout.Current.Fits) return;
        Rect area = GameplayPixelLayoutController.ScreenRect(boardArea);
        Vector3 origin = worldCamera.WorldToScreenPoint(Vector3.zero);
        float pixelsPerUnit = Vector3.Distance(origin, worldCamera.WorldToScreenPoint(Vector3.right));
        if (pixelsPerUnit <= 0) return;
        float sourcePPU = worldCamera.TryGetComponent(out PixelPerfectCamera ppc) ? ppc.assetsPPU : 64;
        float fit = Mathf.Min(area.width / (visuals.OuterLocalWidth * sourcePPU),
            area.height / (visuals.OuterLocalHeight * sourcePPU));
        float ratio = Mathf.Min(layout.Current.BoardTexelRatio, fit);
        if (ratio <= 0) return;
        PhysicalTexelRatio = ratio;
        float scale = ratio * sourcePPU / pixelsPerUnit;
        transform.localScale = new Vector3(scale, scale, 1);
        Vector2 center = area.center;
        // Board art has even native dimensions and centered sprite pivots.
        // Snap in physical screen coordinates, after PPC's camera phase is known.
        center = new Vector2(Mathf.Round(center.x), Mathf.Round(center.y));
        Vector3 position = worldCamera.ScreenToWorldPoint(new Vector3(center.x, center.y,
            Mathf.Abs(transform.position.z - worldCamera.transform.position.z)));
        position.z = transform.position.z;
        transform.position = position;
        PhysicalOrigin = worldCamera.WorldToScreenPoint(transform.position);
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class DungeonPresentationArt
{
    private ApprovedDungeonBattleArt approved;
    private GameplayPixelLayoutController approvedLayout;
    private RectTransform approvedScenery;
    private GameObject approvedWorld;
    private RectTransform approvedSpawnAnchor;
    private EnemyActor approvedActor;
    private float approvedGroundOffset;
    private readonly List<(ApprovedDungeonBattleArt.Placement placement, SpriteRenderer renderer)> approvedSides = new();

    private void BuildApprovedDungeon(Transform player)
    {
        approved = ApprovedDungeonBattleArt.Current;
        approvedLayout = FindFirstObjectByType<GameplayPixelLayoutController>();
        approvedSpawnAnchor = approvedLayout.TopHud.GetComponentInChildren<EnemySlotUI>(true)?.EnemySpawnAnchor;
        approvedScenery = GameUi.Rect("ApprovedDungeonScenery", approvedLayout.TopHud, Vector2.zero, Vector2.zero);
        approvedScenery.anchorMin = Vector2.zero;
        approvedScenery.anchorMax = Vector2.one;
        approvedScenery.offsetMin = approvedScenery.offsetMax = Vector2.zero;
        approvedScenery.SetAsFirstSibling();
        foreach (var placement in approved.Data.placements.OrderBy(p => p.layer))
        {
            if (placement.y < 210 && placement.layer > 0 && placement.layer <= 30 &&
                placement.name != "LowerLeftConnector" && placement.name != "LowerRightConnector")
                approved.Ui(approvedScenery, placement, new Rect(0, 0, 360, 209));
        }

        var backing = approved.Ui((RectTransform)player, "PortraitWall", new Rect(9, 10, 80, 167), new Vector2(99, 190));
        backing.GetComponent<Image>().type = Image.Type.Tiled;
        backing.SetAsFirstSibling();
        approved.Ui((RectTransform)player, approved.Find("BannerPlayer"), new Rect(9, 20, 99, 190)).SetSiblingIndex(1);

        // Full padded sprites may extend under the board. Render them below its
        // world sorting layers, rather than on the overlay Canvas above it.
        approvedWorld = new GameObject("ApprovedDungeonSurround");
        foreach (var placement in approved.Data.placements.Where(p =>
                     p.y >= 210 && (approved.Group(p.name).StartsWith("Props_") || p.name.StartsWith("Lower"))))
        {
            var piece = new GameObject("ApprovedDungeon" + placement.name);
            piece.transform.SetParent(approvedWorld.transform, false);
            var renderer = piece.AddComponent<SpriteRenderer>();
            renderer.sprite = approved.Sprite(placement.name);
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = -180 + placement.layer;
            renderer.maskInteraction = SpriteMaskInteraction.None;
            approvedSides.Add((placement, renderer));
        }

        // One wall continues through every gutter and behind the board.
        backdrop.sprite = approved.Sprite("BackWallReference");
        if (!TryGetComponent<BattleAmbientPresentation>(out _)) gameObject.AddComponent<BattleAmbientPresentation>();
        panelsReady = true;
        UpdateApprovedDungeon();
    }

    private void UpdateApprovedDungeon()
    {
        if (approvedLayout == null || worldCamera == null) return;
        Rect top = GameplayPixelLayoutController.ScreenRect(approvedLayout.TopHud);
        Rect board = GameplayPixelLayoutController.ScreenRect(approvedLayout.BoardArea);
        Rect viewport = approvedLayout.Current.Viewport;

        // Ground the shallow stage using the existing combat-floor anchor.
        // Actor and UI owners retain exactly their existing positions.
        var floor = (RectTransform)approvedLayout.TopHud.Find("BattleFloorAnchor");
        float floorY = GameplayPixelLayoutController.ScreenRect(floor).center.y;
        if (approvedSpawnAnchor != null)
        {
            if (approvedActor == null)
                approvedActor = approvedSpawnAnchor.GetComponentInChildren<EnemyActor>();
            if (approvedActor != null && approvedActor.Definition != null)
            {
                // Match the stable 64px reference fit used by the pixel presenter.
                // Read definition/slot geometry, never animated sprite dimensions
                // or opaque bounds, so attacks and death cannot move the scenery.
                Vector2 box = approvedActor.Definition.VisualSize;
                box.x = Mathf.Min(box.x, ((RectTransform)approvedSpawnAnchor.parent).rect.width);
                float scale = approvedLayout.GetComponentInParent<Canvas>().rootCanvas.scaleFactor;
                int pixelRatio = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(box.x, box.y) / 64 * scale + .00001f));
                approvedGroundOffset = GameplayPixelLayoutController.ScreenRect(approvedSpawnAnchor).center.y -
                                       32 * pixelRatio - floorY;
            }
        }
        floorY += approvedGroundOffset;
        float sceneryHeight = Mathf.Clamp((top.yMax - floorY) / 184f * 209f, 1, top.height);
        float canvasScale = approvedLayout.GetComponentInParent<Canvas>().rootCanvas.scaleFactor;
        GameplayPixelLayoutController.SetRect(approvedScenery,
            new Rect(0, (top.height - sceneryHeight) / canvasScale, top.width / canvasScale, sceneryHeight / canvasScale));
        float scaleX = top.width / 360;
        float scaleY = sceneryHeight / 209;
        float unitsPerPixel = worldCamera.orthographicSize * 2 / worldCamera.pixelHeight;

        // Whole repeating courses share the top scenery's phase and density.
        float tileWidth = 64 * scaleX;
        float tileHeight = 32 * scaleY;
        float left = top.xMin - Mathf.Ceil(top.xMin / tileWidth) * tileWidth;
        float upper = top.yMax + Mathf.Ceil((worldCamera.pixelHeight - top.yMax) / tileHeight) * tileHeight;
        float width = Mathf.Ceil((worldCamera.pixelWidth - left) / tileWidth) * tileWidth;
        float height = Mathf.Ceil(upper / tileHeight) * tileHeight;
        backdrop.transform.localScale = new Vector3(scaleX * unitsPerPixel * 64, scaleY * unitsPerPixel * 64, 1);
        backdrop.size = new Vector2(width / (scaleX * 64), height / (scaleY * 64));
        backdrop.transform.position = ApprovedWorldPoint(new Vector2(left + width / 2, upper - height / 2));

        foreach (var pair in approvedSides)
        {
            var placement = pair.placement;
            var sprite = pair.renderer.sprite;
            bool lower = placement.name.StartsWith("Lower");
            float x = viewport.xMin + (placement.x + sprite.rect.width / 2) * scaleX;
            float y = lower
                ? viewport.yMin + (640 - placement.y - sprite.rect.height / 2) * scaleX
                : board.yMax - (placement.y + sprite.rect.height / 2 - 224) / 305 * board.height;
            pair.renderer.transform.position = ApprovedWorldPoint(new Vector2(Mathf.Round(x), Mathf.Round(y)));
            pair.renderer.transform.localScale = Vector3.one * (scaleX * unitsPerPixel * 64);
        }
    }

    private Vector3 ApprovedWorldPoint(Vector2 point)
    {
        var world = worldCamera.ScreenToWorldPoint(new Vector3(point.x, point.y, 10));
        world.z = 10;
        return world;
    }
}

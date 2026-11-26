using UnityEngine;

/// <summary>A non-interactive footprint. RunSession and BoardController own acceptance.</summary>
public sealed class BombTargetPreview : MonoBehaviour
{
    private SpriteRenderer[] cells;
    private int columns, radius;
    private bool wholeBoard, wasVisible;
    private Texture2D texture;
    private Sprite sprite;

    private void Start()
    {
        texture = new Texture2D(1, 1) { filterMode = FilterMode.Point };
        texture.SetPixel(0, 0, Color.white); texture.Apply();
        sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * .5f, 1);
        var run = RunSession.Current;
        radius = BalanceV1.Current.consumableBombRadius;
        wholeBoard = run.BombKind == ConsumableKind.ColorBomb;
        columns = wholeBoard ? run.Board.Width : 2 * radius + 1;
        cells = new SpriteRenderer[columns * (wholeBoard ? run.Board.Height : columns)];
        for (int i = 0; i < cells.Length; i++)
        {
            var cell = new GameObject("Bomb footprint " + i);
            cell.transform.SetParent(transform, false);
            cells[i] = cell.AddComponent<SpriteRenderer>();
            cells[i].enabled = false;
            cells[i].sprite = sprite; cells[i].color = new Color(1, .65f, .1f, .32f);
            cells[i].sortingLayerName="Gems"; cells[i].sortingOrder = 30;
        }
    }

    private void LateUpdate()
    {
        var run = RunSession.Current;
        if (cells == null) return;
        bool visible = run != null && run.HasBombPreview && run.Board != null && run.Board.IsSelectingTarget;
        if (!visible && !wasVisible) return;
        wasVisible = visible;
        var center = visible ? run.Board.GetGem(run.BombPreviewCell.x, run.BombPreviewCell.y) : null;
        int left = visible && !wholeBoard ? run.BombPreviewCell.x - radius : 0;
        int bottom = visible && !wholeBoard ? run.BombPreviewCell.y - radius : 0;
        for (int i = 0; i < cells.Length; i++)
        {
            int x = left + i % columns;
            int y = bottom + i / columns;
            cells[i].enabled = visible && x >= 0 && y >= 0 && x < run.Board.Width && y < run.Board.Height &&
                run.Board.IsSupplyTarget(center, run.Board.GetGem(x, y), radius, wholeBoard);
            if (!cells[i].enabled) continue;
            cells[i].transform.position = run.Board.transform.TransformPoint(run.Board.GetCellLocalPosition(x, y));
            cells[i].transform.localScale = run.Board.transform.lossyScale * run.Board.CellSize * .94f;
        }
    }

    private void OnDestroy() { if (sprite != null) Destroy(sprite); if (texture != null) Destroy(texture); }
}

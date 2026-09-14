using UnityEngine;

/// <summary>A non-interactive footprint. RunSession and BoardController own acceptance.</summary>
public sealed class BombTargetPreview : MonoBehaviour
{
    private SpriteRenderer[] cells;
    private Texture2D texture;
    private Sprite sprite;

    private void Start()
    {
        texture = new Texture2D(1, 1) { filterMode = FilterMode.Point };
        texture.SetPixel(0, 0, Color.white); texture.Apply();
        sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * .5f, 1);
        cells = new SpriteRenderer[9];
        for (int i = 0; i < cells.Length; i++)
        {
            var cell = new GameObject("Bomb footprint " + i);
            cell.transform.SetParent(transform, false);
            cells[i] = cell.AddComponent<SpriteRenderer>();
            cells[i].sprite = sprite; cells[i].color = new Color(1, .65f, .1f, .32f);
            cells[i].sortingLayerName="Gems"; cells[i].sortingOrder = 30;
        }
    }

    private void LateUpdate()
    {
        var run = RunSession.Current;
        if (cells == null) return;
        bool visible = run != null && run.HasBombPreview && run.Board != null && run.Board.IsSelectingTarget;
        for (int i = 0; i < cells.Length; i++)
        {
            int x = visible ? run.BombPreviewCell.x + i % 3 - 1 : -1;
            int y = visible ? run.BombPreviewCell.y + i / 3 - 1 : -1;
            cells[i].enabled = visible && x >= 0 && y >= 0 && x < run.Board.Width && y < run.Board.Height;
            if (!cells[i].enabled) continue;
            cells[i].transform.position = run.Board.transform.TransformPoint(run.Board.GetCellLocalPosition(x, y));
            cells[i].transform.localScale = run.Board.transform.lossyScale * run.Board.CellSize * .94f;
        }
    }

    private void OnDestroy() { if (sprite != null) Destroy(sprite); if (texture != null) Destroy(texture); }
}

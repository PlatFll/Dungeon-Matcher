using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Compact status rows inside the existing player combat panel.</summary>
[DefaultExecutionOrder(10010)]
public sealed class PlayerStatusPanel : MonoBehaviour
{
    private PlayerPanelUI panel;
    private PlayerActor player;
    private RectTransform root;
    private readonly Dictionary<PlayerStatusKind, PlayerStatusDefinition> definitions = new Dictionary<PlayerStatusKind, PlayerStatusDefinition>();
    private readonly List<RectTransform> cells = new List<RectTransform>();
    public RectTransform StatusRoot => root;

    private void Awake()
    {
        panel = GetComponent<PlayerPanelUI>();
        foreach (var data in Resources.LoadAll<PlayerStatusDefinition>("PlayerStatuses")) definitions[data.kind] = data;
        root = GameUi.Rect("PlayerStatuses", transform, Vector2.zero, Vector2.zero);
        root.gameObject.SetActive(false);
    }
    private void LateUpdate()
    {
        var bound = panel != null ? panel.BoundPlayer : null;
        if (player != bound)
        {
            if (player != null) player.Statuses.Changed -= Refresh;
            player = bound;
            if (player != null) player.Statuses.Changed += Refresh;
            Refresh();
        }
        Layout();
    }
    private void Refresh()
    {
        foreach (var cell in cells) Destroy(cell.gameObject);
        cells.Clear();
        if (player != null)
            foreach (var kind in player.Statuses.Effects.Select(e => e.kind).Distinct().OrderBy(k => k))
            {
                var cell = GameUi.Rect(kind.ToString(), root, new Vector2(22, 18), Vector2.zero);
                var icon = GameUi.Rect("Icon", cell, new Vector2(16, 16), new Vector2(-2, 1)).gameObject.AddComponent<Image>();
                icon.sprite = definitions.TryGetValue(kind, out var data) ? data.icon : null;
                icon.raycastTarget = true;
                var button = icon.gameObject.AddComponent<Button>(); button.targetGraphic = icon;
                button.navigation = new Navigation { mode = Navigation.Mode.None };
                button.onClick.AddListener(() => RunSession.Current?.GetComponent<RunControlsUI>()?.OpenGuide(Describe()));
                var label = GameUi.Label("Moves", cell, player.Statuses.Remaining(kind).ToString(), new Vector2(10, 10), new Vector2(6, -5), 10);
                label.color = new Color32(242, 233, 208, 255); label.fontStyle = FontStyles.Normal;
                cells.Add(cell);
            }
        root.gameObject.SetActive(cells.Count > 0);
        Layout();
    }
    private void Layout()
    {
        if (root == null || cells.Count == 0) return;
        var rect = (RectTransform)transform;
        int columns = Mathf.Clamp(Mathf.FloorToInt((rect.rect.width - 8) / 22), 1, 7);
        int rows = Mathf.CeilToInt(cells.Count / (float)columns);
        root.sizeDelta = new Vector2(Mathf.Min(columns, cells.Count) * 22, rows * 18);
        for (int i = 0; i < cells.Count; i++)
        {
            int row = i / columns, count = Mathf.Min(columns, cells.Count - row * columns);
            cells[i].anchoredPosition = new Vector2((i % columns - (count - 1) * .5f) * 22, ((rows - 1) * .5f - row) * 18);
        }
    }
    public string Describe()
    {
        var text = new StringBuilder("PLAYER STATUSES\n\n");
        if (player == null) return text.ToString();
        foreach (var entry in player.Statuses.Effects.OrderBy(e => e.kind).ThenBy(e => e.sourceId))
        {
            definitions.TryGetValue(entry.kind, out var data);
            text.Append(data != null ? data.displayName : entry.kind.ToString());
            if (entry.kind == PlayerStatusKind.Fear)
            {
                var source = RunSession.Current?.Waves?.ActiveEnemies.FirstOrDefault(e => e != null && e.PersistentId == entry.sourceId);
                if (source != null) text.Append(" (" + source.Definition.DisplayName + ")");
            }
            text.Append(": " + entry.remainingMoves + " moves\n");
            if (data != null) text.AppendLine(data.description);
            text.AppendLine();
        }
        return text.ToString();
    }
    private void OnDestroy() { if (player != null) player.Statuses.Changed -= Refresh; }
}

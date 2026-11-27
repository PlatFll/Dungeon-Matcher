using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class CharacterSelectMenuController
{
    [SerializeField] private Sprite[] boardGems;
    [SerializeField] private Sprite colorCrystalSprite, poisonBombSprite, healingBombSprite, shieldBombSprite;
    [SerializeField] private Sprite sourceAmethyst;
    private RectTransform masteryPage, shapePanel, resultPanel, summaryPanel;
    private Image setupReward, setupStone;
    private TMP_Text buildOwner, setupShapeName, setupName;
    private GemMasteryShape shape;
    private static readonly GemMasteryShape[] Shapes =
        { GemMasteryShape.StraightFive, GemMasteryShape.TShape, GemMasteryShape.LShape, GemMasteryShape.CrossShape };
    private static readonly string[] ShapeNames = { "Straight 5", "T", "L", "Cross" };
    private static readonly string[][] Diagrams =
    {
        new[] { "00000", "11111", "00000" }, new[] { "01110", "00100", "00100" },
        new[] { "01000", "01000", "01110" }, new[] { "00100", "01110", "00100" }
    };
    private static readonly GemMasteryReward[] Rewards =
        { GemMasteryReward.ColorCrystal, GemMasteryReward.PoisonBomb, GemMasteryReward.HealBomb, GemMasteryReward.ShieldBomb };
    private static readonly string[] RewardNames = { "Color Crystal", "Poison Bomb", "Healing Bomb", "Shield Bomb" };
    private static readonly string[] RewardEffects =
        { "Clears gems of one color.", "Poisons enemies.", "Restores health.", "Grants shield." };
    private readonly Button[] shapeTiles = new Button[4], rewardRows = new Button[4];
    private readonly Image[] rewardBadges = new Image[4];
    private readonly TMP_Text[] rewardActions = new TMP_Text[4];
    private readonly RectTransform[] setupShapes = new RectTransform[4];

    private Sprite RewardSprite(GemSpecialType type) => type switch
    {
        GemSpecialType.ColorCrystal => colorCrystalSprite,
        GemSpecialType.PoisonBomb => poisonBombSprite,
        GemSpecialType.HealingBomb => healingBombSprite,
        GemSpecialType.ShieldBomb => shieldBombSprite,
        GemSpecialType.RowBomb or GemSpecialType.ColumnBomb => BombIdleArt.DirectionalStill(type),
        _ => null
    };

    private Sprite RewardSprite(GemMasteryReward reward) =>
        GemMasteryRuntimeResolver.TryGetSpecialType(reward, out var type) ? RewardSprite(type) : null;

    private Image FillBombSocket(Image shell, GemSpecialType type, Image stone = null)
    {
        bool hasSocket = type == GemSpecialType.PoisonBomb || type == GemSpecialType.HealingBomb || type == GemSpecialType.ShieldBomb;
        if (hasSocket)
        {
            if (stone == null) stone = Picture("SourceAmethyst", shell.transform, sourceAmethyst, 0, 0, 0, 0, true);
            var bounds = shell.sprite.bounds.size;
            var box = shell.rectTransform.rect.size;
            float units = Mathf.Min(box.x / bounds.x, box.y / bounds.y);
            var offset = (Vector2)BombIdleArt.SocketOffset(type) * units;
            var size = (Vector2)sourceAmethyst.bounds.size * units;
            var rect = stone.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one * .5f;
            rect.anchoredPosition = offset;
            rect.sizeDelta = size;
        }
        if (stone != null) stone.gameObject.SetActive(hasSocket);
        return stone;
    }

    private void BuildMastery()
    {
        masteryPage = Box("SpecialGemBuild", page, 0, 86, 360, 500);
        var paper = Picture("Instructions", masteryPage, Art("Parchment"), 10, 0, 340, 51);
        buildOwner = Text("BuildOwner", paper.transform, "", 16, 6, 308, 20, 23);
        buildOwner.color = Ink;
        Text("ShapeInstructions", paper.transform, "Choose a five-gem shape, then assign its reward.",
            16, 26, 308, 18, 19).color = Ink;

        shapePanel = Picture("Shapes", masteryPage, Art("Stats"), 8, 72, 344, 96).rectTransform;
        MasteryHeading(shapePanel, "1. SELECT MATCH SHAPE", 218);
        for (int i = 0; i < Shapes.Length; i++)
        {
            int index = i;
            var tile = Control(Shapes[i].ToString(), shapePanel, Art("Roster"), 11 + i * 82, 20, 76, 68, () =>
            {
                if (shape != Shapes[index]) MenuSoundPlayer.Play(MenuCue.Press);
                shape = Shapes[index];
                RefreshMastery();
            }, MenuCue.None);
            shapeTiles[i] = tile;
            Diagram(tile.transform, i, 38, 26, 11);
            Text("ShapeName", tile.transform, ShapeNames[i].ToUpperInvariant(), 8, 44, 60, 16, 16);
        }

        resultPanel = Picture("Results", masteryPage, Art("Stats"), 8, 188, 344, 224).rectTransform;
        MasteryHeading(resultPanel, "2. SELECT RESULTING GEM", 234);
        for (int i = 0; i < Rewards.Length; i++)
        {
            int index = i;
            var row = Control("Assign" + Rewards[i], resultPanel, ShopArt("Card"), 10, 21 + i * 49, 324, 44, () =>
            {
                if (GemMasterySettings.GetReward(shape) == Rewards[index]) return;
                if (!GemMasterySettings.SetReward(shape, Rewards[index]))
                {
                    if (account.LastError != null) setupName.text = "Couldn’t save. Try again.";
                    MenuSoundPlayer.Play(MenuCue.Denied);
                    return;
                }
                RefreshMastery();
                MenuSoundPlayer.Play(MenuCue.Socket);
            }, MenuCue.None);
            rewardRows[i] = row;
            var shell = Picture("RewardIcon", row.transform, RewardSprite(Rewards[i]), 15, 5, 34, 34, true);
            GemMasteryRuntimeResolver.TryGetSpecialType(Rewards[i], out var type);
            FillBombSocket(shell, type);
            Text("RewardName", row.transform, RewardNames[i].ToUpperInvariant(), 58, 5, 156, 17, 21);
            Text("Effect", row.transform, RewardEffects[i], 58, 24, 156, 15, 17);
            var badge = Picture("Selection", row.transform, ShopArt("Wallet"), 226, 7, 86, 30);
            rewardBadges[i] = badge;
            rewardActions[i] = Text("Action", badge.transform, "", 8, 5, 70, 20, 17);
        }

        summaryPanel = Picture("CurrentSetup", masteryPage, Art("Stats"), 8, 434, 344, 66).rectTransform;
        MasteryHeading(summaryPanel, "CURRENT SETUP", 156);
        const int shapeCenter = 87;
        const int rewardCenter = 257;
        for (int i = 0; i < Shapes.Length; i++)
            setupShapes[i] = Diagram(summaryPanel, i, shapeCenter, 30, 9);

        var arrow = Picture("ResultArrow", summaryPanel, ShopArt("Arrow"), 172, 33, 18, 16, true);
        arrow.rectTransform.pivot = Vector2.one * .5f;
        arrow.rectTransform.localScale = new Vector3(-1, 1, 1);
        setupReward = Picture("EquippedGem", summaryPanel, null, rewardCenter - 15, 15, 30, 30, true);
        setupShapeName = Text("SetupShape", summaryPanel, "", shapeCenter - 65, 45, 130, 15, 17);
        setupName = Text("SetupName", summaryPanel, "", rewardCenter - 65, 45, 130, 15, 17);
    }

    private void MasteryHeading(RectTransform panel, string caption, int width)
    {
        var heading = Picture("Heading", panel, Art("Plaque"), (panel.rect.width - width) / 2, -14, width, 28);
        Text("Caption", heading.transform, caption, 13, 8, width - 26, 15, 20);
    }

    private RectTransform Diagram(Transform parent, int index, int centerX, int centerY, int pitch)
    {
        // The last cell has no trailing gutter. Position the complete grid around its center.
        int width = 5 * pitch - 1, height = 3 * pitch - 1;
        var grid = Box("MatchDiagram", parent, centerX - width / 2f, centerY - height / 2f, width, height);
        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 5; x++)
        {
            var cell = Picture("Cell", grid, null, x * pitch, y * pitch, pitch - 1, pitch - 1);
            cell.color = new Color32(63, 46, 76, 255);
            if (Diagrams[index][y][x] == '1')
                Picture("Gem", cell.transform, boardGems[(int)GemType.Amethyst], 0, 0, pitch - 1, pitch - 1, true);
        }
        return grid;
    }

    private void LayoutMastery(float extra)
    {
        masteryPage.anchoredPosition = new Vector2(0, -86 - Mathf.Round(extra * .1f));
        shapePanel.anchoredPosition = new Vector2(8, -72 - Mathf.Round(extra * .19f));
        resultPanel.anchoredPosition = new Vector2(8, -188 - Mathf.Round(extra * .39f));
        summaryPanel.anchoredPosition = new Vector2(8, -434 - Mathf.Round(extra * .71f));
    }

    private void RefreshMastery()
    {
        buildOwner.text = selected.title.ToUpperInvariant() + "’S BUILD";
        var equipped = GemMasterySettings.GetReward(shape);
        int shapeIndex = System.Array.IndexOf(Shapes, shape);
        for (int i = 0; i < Shapes.Length; i++)
        {
            shapeTiles[i].image.sprite = Art(i == shapeIndex ? "RosterSelected" : "Roster");
            setupShapes[i].gameObject.SetActive(i == shapeIndex);
        }
        for (int i = 0; i < Rewards.Length; i++)
        {
            var reward = Rewards[i];
            bool unlocked = account.IsUnlocked(reward);
            bool active = reward == equipped;
            var row = rewardRows[i];
            row.interactable = unlocked;
            row.image.sprite = ShopArt(active ? "SelectedCard" : "Card");
            GemMasteryRuntimeResolver.TryGetSpecialType(reward, out var type);
            rewardBadges[i].sprite = active ? ShopArt("Buy") : unlocked ? ShopArt("Wallet") : ShopArt("Disabled");
            rewardActions[i].color = active ? Ink : Cream;
            rewardActions[i].text = active ? "EQUIPPED" : unlocked ? "SELECT" : $"ANY LV. {BalanceV1.Current.UnlockLevel(type)}";
        }
        setupReward.sprite = RewardSprite(equipped);
        GemMasteryRuntimeResolver.TryGetSpecialType(equipped, out var equippedType);
        setupStone = FillBombSocket(setupReward, equippedType, setupStone);
        int index = System.Array.IndexOf(Rewards, equipped);
        setupShapeName.text = ShapeNames[shapeIndex].ToUpperInvariant();
        setupName.text = RewardNames[index].ToUpperInvariant();
    }
}

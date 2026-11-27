using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class CharacterSelectMenuController
{
    [SerializeField] private Sprite[] boardGems;
    [SerializeField] private Sprite colorCrystalSprite, poisonBombSprite, healingBombSprite, shieldBombSprite;
    [SerializeField] private Sprite sourceAmethyst;
    private RectTransform masteryPage;
    private Image setupReward, setupStone;
    private TMP_Text buildOwner, setupName;
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
        masteryPage = Box("SpecialGemBuild", page, 0, 84, 360, 495);
        var paper = Picture("Instructions", masteryPage, Art("Parchment"), 12, 0, 336, 62);
        buildOwner = Text("BuildOwner", paper.transform, "", 12, 7, 312, 23, 24);
        buildOwner.color = Ink;
        Text("ShapeInstructions", paper.transform, "Choose a five-gem shape, then assign its reward.",
            23, 29, 290, 24, 20, TextAlignmentOptions.Midline, true).color = Ink;

        var shapes = Picture("Shapes", masteryPage, Art("Stats"), 7, 79, 346, 91);
        Heading(shapes.transform, "1. SELECT MATCH SHAPE", 63, -14, 220);
        for (int i = 0; i < Shapes.Length; i++)
        {
            int index = i;
            var tile = Control(Shapes[i].ToString(), shapes.transform, Art("Roster"), 7 + i * 83, 19, 81, 65, () =>
            {
                shape = Shapes[index];
                RefreshMastery();
            });
            shapeTiles[i] = tile;
            Diagram(tile.transform, i, 11, 5, 12);
            Text("ShapeName", tile.transform, ShapeNames[i].ToUpperInvariant(), 3, 42, 75, 18, 17);
        }

        var results = Picture("Results", masteryPage, Art("Stats"), 7, 189, 346, 224);
        Heading(results.transform, "2. SELECT RESULTING GEM", 57, -14, 232);
        for (int i = 0; i < Rewards.Length; i++)
        {
            int index = i;
            var row = Control("Assign" + Rewards[i], results.transform, ShopArt("Card"), 7, 19+i*49, 332, 47, () =>
            {
                if (!GemMasterySettings.SetReward(shape, Rewards[index]) && account.LastError != null)
                {
                    setupName.text = "Couldn’t save. Try again.";
                    return;
                }
                RefreshMastery();
            });
            rewardRows[i] = row;
            var shell = Picture("RewardIcon", row.transform, RewardSprite(Rewards[i]), 10, 5, 43, 37, true);
            GemMasteryRuntimeResolver.TryGetSpecialType(Rewards[i], out var type);
            FillBombSocket(shell, type);
            Text("RewardName", row.transform, RewardNames[i].ToUpperInvariant(), 60, 3, 164, 23, 23, TextAlignmentOptions.MidlineLeft);
            Text("Effect", row.transform, RewardEffects[i], 60, 23, 168, 19, 18, TextAlignmentOptions.MidlineLeft);
            var badge = Picture("Selection", row.transform, Art("Stats"), 231, 7, 92, 33);
            rewardBadges[i] = badge;
            rewardActions[i] = Text("Action", badge.transform, "", 4, 4, 84, 24, 18);
        }

        var setup = Picture("CurrentSetup", masteryPage, Art("Stats"), 7, 432, 346, 64);
        Heading(setup.transform, "CURRENT SETUP", 97, -14, 152);
        var setupDiagram = Box("MatchDiagram", setup.transform, 14, 18, 61, 37);
        for (int i = 0; i < Shapes.Length; i++)
        {
            setupShapes[i] = Box(Shapes[i].ToString(), setupDiagram, 0, 0, 61, 37);
            Diagram(setupShapes[i], i, 0, 0, 12);
        }
        var arrow = Picture("ResultArrow", setup.transform, ShopArt("Arrow"), 81, 24, 22, 20, true);
        arrow.rectTransform.localScale = new Vector3(-1,1,1);
        arrow.rectTransform.anchoredPosition = new Vector2(103,-24);
        setupReward = Picture("EquippedGem", setup.transform, null, 110, 15, 48, 42, true);
        setupName = Text("SetupName", setup.transform, "", 165, 14, 168, 42, 22, TextAlignmentOptions.MidlineLeft, true);
    }

    private void Diagram(Transform parent, int index, int left, int top, int pitch)
    {
        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 5; x++)
        {
            var cell = Picture("Cell", parent, null, left + x * pitch, top + y * pitch, pitch-1, pitch-1);
            cell.color = new Color32(63, 46, 76, 255);
            if (Diagrams[index][y][x] == '1')
                Picture("Gem", cell.transform, boardGems[(int)GemType.Amethyst], 0, 0, pitch-1, pitch-1, true);
        }
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
            rewardBadges[i].sprite = active ? ShopArt("Buy") : unlocked ? Art("Plaque") : ShopArt("Disabled");
            rewardActions[i].color = active ? Ink : Cream;
            rewardActions[i].text = active ? "EQUIPPED" : unlocked ? "SELECT" : $"ANY LV. {BalanceV1.Current.UnlockLevel(type)}";
        }
        setupReward.sprite = RewardSprite(equipped);
        GemMasteryRuntimeResolver.TryGetSpecialType(equipped, out var equippedType);
        setupStone = FillBombSocket(setupReward, equippedType, setupStone);
        int index = System.Array.IndexOf(Rewards, equipped);
        setupName.text = ShapeNames[shapeIndex] + " match\n" + RewardNames[index];
    }
}

using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class CharacterSelectMenuController
{
    private enum MenuPage { Characters, Cards, Gems }
    private MenuPage currentPage;
    private RunUpgradeDefinition[] catalog;
    private RectTransform libraryScenery, viewport, entries, emptyJournal;
    private ScrollRect journalScroll;
    private Scrollbar scrollBar;
    private TMP_Text discovery, sortLabel;
    private bool alphabetical, masteryBackToRoster;

    private void BuildLibrary()
    {
        catalog = Resources.Load<RunUpgradeCatalog>("RunUpgrades/PrototypeRunUpgradeCatalog").Upgrades
            .Where(card => card != null).ToArray();
        libraryScenery = Box("LibraryArch", page, 0, 0, 360, 118);
        for (int side = 0; side < 2; side++)
        {
            var bay = Box("TorchBay", libraryScenery, side * 180, 0, 180, 116);
            bay.gameObject.AddComponent<RectMask2D>();
            var wall = Picture("ChamberMasonry", bay, Art("Chamber"), side * 180, -38, 240, 192);
            if (side == 1) wall.rectTransform.localScale = new Vector3(-1, 1, 1);
        }
        Picture("Lintel", libraryScenery, ShopArt("Footer"), 0, 114, 360, 5).type = Image.Type.Tiled;
        cardPage = Box("CardLibrary", page, 0, 79, 360, 506);
        var counter = Picture("DiscoveryCounter", cardPage, ShopArt("Wallet"), 12, 0, 164, 31);
        discovery = Text("Discovered", counter.transform, "", 6, 4, 152, 22, 21);
        var sort = Control("SortCards", cardPage, Art("Stats"), 184, 0, 164, 31, () =>
        {
            alphabetical = !alphabetical;
            RefreshJournal();
        });
        sortLabel = Text("Sort", sort.transform, "", 5, 4, 154, 22, 21);
        var paper = Picture("JournalIntroduction", cardPage, Art("Parchment"), 9, 45, 342, 65);
        Heading(paper.transform, "RUN UPGRADE CARDS", 74, -10, 194);
        Text("HowCardsWork", paper.transform, "Upgrades you’ve seen on your runs. Their bonuses last until the run ends.",
            19, 17, 304, 40, 22, TextAlignmentOptions.Midline, true).color = Ink;

        viewport = Box("JournalViewport", cardPage, 9, 119, 330, 383);
        var touch = viewport.gameObject.AddComponent<Image>();
        touch.color = Color.clear;
        viewport.gameObject.AddComponent<RectMask2D>();
        entries = Box("DiscoveredCards", viewport, 0, 0, 330, 0);
        journalScroll = viewport.gameObject.AddComponent<ScrollRect>();
        journalScroll.viewport = viewport;
        journalScroll.content = entries;
        journalScroll.horizontal = false;
        journalScroll.movementType = ScrollRect.MovementType.Clamped;
        journalScroll.scrollSensitivity = 35;
        var track = Picture("JournalRail", cardPage, ShopArt("Rail"), 342, 120, 10, 381);
        track.raycastTarget = true;
        var thumb = Picture("Thumb", track.transform, ShopArt("Thumb"), 0, 0, 10, 25);
        thumb.raycastTarget = true;
        thumb.rectTransform.offsetMin = thumb.rectTransform.offsetMax = Vector2.zero;
        scrollBar = track.gameObject.AddComponent<Scrollbar>();
        scrollBar.handleRect = thumb.rectTransform;
        scrollBar.targetGraphic = thumb;
        scrollBar.direction = Scrollbar.Direction.BottomToTop;
        journalScroll.verticalScrollbar = scrollBar;

        emptyJournal = Picture("EmptyJournal", viewport, Art("Stats"), 14, 22, 302, 110).rectTransform;
        Picture("CardEmblem", emptyJournal, GlobalMenuSkin.Load("Challenges"), 133, 10, 36, 32, true);
        Text("FirstDiscovery", emptyJournal, "Reach a Resonance card choice to discover your first cards.",
            17, 45, 268, 50, 22, TextAlignmentOptions.Midline, true);
    }

    private void Heading(Transform parent, string caption, float x, float y, float width)
    {
        var plaque = Picture("Heading", parent, Art("Plaque"), x, y, width, 29);
        Text("Caption", plaque.transform, caption, 9, 6, width - 18, 19, 21);
    }

    private void GoBack()
    {
        if (currentPage == MenuPage.Gems && masteryBackToRoster) ShowPage(MenuPage.Characters);
        else backRequested?.Invoke();
    }

    public void OpenMasteryFromHome()
    {
        masteryBackToRoster = false;
        ShowPage(MenuPage.Gems);
    }

    private void ShowPage(MenuPage target)
    {
        currentPage = target;
        bool rosterOpen = target == MenuPage.Characters;
        chamber.gameObject.SetActive(rosterOpen);
        figures.gameObject.SetActive(rosterOpen);
        details.gameObject.SetActive(rosterOpen);
        collection.gameObject.SetActive(rosterOpen);
        libraryScenery.gameObject.SetActive(!rosterOpen);
        cardPage.gameObject.SetActive(target == MenuPage.Cards);
        masteryPage.gameObject.SetActive(target == MenuPage.Gems);
        titleLabel.text = rosterOpen ? "CHARACTERS" : target == MenuPage.Cards ? "CARDS" : "SPECIAL GEMS";
        header.sprite = rosterOpen ? Art("Plaque") : ShopArt("Title");
        header.rectTransform.anchoredPosition = rosterOpen ? new Vector2(52,-6) : new Vector2(72,-27);
        header.rectTransform.sizeDelta = rosterOpen ? new Vector2(184,47) : new Vector2(216,50);
        titleLabel.rectTransform.anchoredPosition = rosterOpen ? new Vector2(11,-10) : new Vector2(27,-14);
        cardsTab.image.sprite = Art(target == MenuPage.Cards ? "RosterSelected" : "Stats");
        charactersTab.image.sprite = Art(rosterOpen ? "RosterSelected" : "Stats");
        gemsTab.image.sprite = Art(target == MenuPage.Gems ? "RosterSelected" : "Stats");
        if (target == MenuPage.Cards) RefreshJournal();
        if (target == MenuPage.Gems) RefreshMastery();
    }

    private void RefreshJournal()
    {
        foreach (Transform row in entries)
        {
            row.gameObject.SetActive(false);
            Destroy(row.gameObject);
        }
        var available = catalog.Where(card => string.IsNullOrEmpty(card.RequiredPlayerId) ||
            CharacterSelectionSettings.IsSelectableCharacter(card.RequiredPlayerId) || account.HasDiscoveredCard(card.UpgradeId)).ToArray();
        var seen = available.Where(card => account.HasDiscoveredCard(card.UpgradeId));
        var sorted = alphabetical ? seen.OrderBy(card => card.DisplayTitle, StringComparer.Ordinal) :
            seen.OrderBy(card => card.Rarity);
        int count = 0;
        foreach (var card in sorted) BuildEntry(card, count++);
        entries.sizeDelta = new Vector2(330, Mathf.Max(0, count * 68 - 4));
        discovery.text = $"{count} / {available.Length} DISCOVERED";
        sortLabel.text = alphabetical ? "SORT: NAME" : "SORT: RARITY";
        emptyJournal.gameObject.SetActive(count == 0);
        journalScroll.StopMovement();
        journalScroll.verticalNormalizedPosition = 1;
        UpdateRail();
    }

    private void BuildEntry(RunUpgradeDefinition card, int index)
    {
        var row = Picture(card.UpgradeId, entries, ShopArt("Card"), 0, index * 68, 330, 64);
        Color accent = card.Rarity switch
        {
            RunUpgradeRarity.Uncommon => new Color32(177, 232, 169, 255),
            RunUpgradeRarity.Rare => new Color32(158, 212, 255, 255),
            RunUpgradeRarity.Epic => new Color32(235, 175, 255, 255),
            _ => Cream
        };
        var socket = Picture("Socket", row.transform, ShopArt("Socket"), 3, 2, 61, 60);
        socket.color = accent;
        if (card.Artwork == null && card.MechanicGrants.Any(grant => grant.Mechanic == RunUpgradeMechanic.CascadeCatalyst))
        {
            var gem = boardGems[(int)GemType.Sapphire];
            Picture("Artwork", socket.transform, gem, 20, 7, 21, 21, true);
            Picture("CascadeLeft", socket.transform, gem, 9, 30, 20, 20, true);
            Picture("CascadeRight", socket.transform, gem, 32, 30, 20, 20, true);
        }
        else
        {
            var art = Picture("Artwork", socket.transform, CardArt(card), 10, 10, 41, 40, true);
            if (card.Artwork == null && art.sprite == RewardSprite(card.RequiredSpecial))
                FillBombSocket(art, card.RequiredSpecial);
        }
        Text("CardTitle", row.transform, card.DisplayTitle.ToUpperInvariant(), 69, 6, 168, 23, 24, TextAlignmentOptions.MidlineLeft);
        Text("Description", row.transform, card.JournalDescription, 70, 28, 249, 31, 19, TextAlignmentOptions.MidlineLeft, true);
        var badge = Picture("RarityBadge", row.transform, Art("Stats"), 240, 6, 82, 22);
        badge.color = accent;
        Text("Rarity", badge.transform, card.Rarity.ToString().ToUpperInvariant(), 4, 2, 74, 18, 16).color = accent;
    }

    private Sprite CardArt(RunUpgradeDefinition card)
    {
        if (card.Artwork != null) return card.Artwork;
        var owner = roster.FirstOrDefault(entry => entry.definition.PlayerId == card.RequiredPlayerId);
        if (owner?.definition.ActiveAbility != null) return owner.definition.ActiveAbility.ReadyStoneEngraving ?? owner.definition.ActiveAbility.Icon;
        var reward = RewardSprite(card.RequiredSpecial);
        if (reward != null) return reward;
        if (card.MechanicGrants.Count > 0)
        {
            switch (card.MechanicGrants[0].Mechanic)
            {
                case RunUpgradeMechanic.BossHunter: return GlobalMenuSkin.Load("CrownedSkull");
                case RunUpgradeMechanic.Executioner: return GlobalMenuSkin.Load("Skull");
                case RunUpgradeMechanic.Bombsmith: return BombIdleArt.DirectionalStill(GemSpecialType.RowBomb);
                case RunUpgradeMechanic.ChainReaction: return ShopArt("Bomb");
                case RunUpgradeMechanic.PreparedCasting: return Art("Energy");
                case RunUpgradeMechanic.OpeningVolley: return BombIdleArt.DirectionalStill(GemSpecialType.ColumnBomb);
            }
        }
        if (card.Modifiers.Count > 0)
        {
            switch (card.Modifiers[0].Stat)
            {
                case RunUpgradeStat.MaximumHealth: return Art("Heart");
                case RunUpgradeStat.Healing: return ShopArt("HealthPotion");
                case RunUpgradeStat.MaximumShield:
                case RunUpgradeStat.ShieldGranted: return Art("Shield");
                case RunUpgradeStat.AbilityEnergyCost:
                case RunUpgradeStat.AbilityEnergyGain: return Art("Energy");
            }
        }
        return card.Theme switch
        {
            RunUpgradeTheme.Survival => Art("Shield"),
            RunUpgradeTheme.Ability => Art("Energy"),
            _ => boardGems[(int)GemType.Ruby]
        };
    }

    private void LayoutLibrary(float extra)
    {
        viewport.sizeDelta = new Vector2(330, 383 + extra);
        ((RectTransform)scrollBar.transform).sizeDelta = new Vector2(10, 381 + extra);
        masteryPage.anchoredPosition = new Vector2(0, -84 - extra * .35f);
        UpdateRail();
    }

    private void UpdateRail()
    {
        scrollBar.gameObject.SetActive(entries.rect.height > viewport.rect.height);
    }
}

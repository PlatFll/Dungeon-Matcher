using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class CharacterSelectMenuController
{
    private RunUpgradeCatalog catalog;
    private RunUpgradeDefinition[] cards = Array.Empty<RunUpgradeDefinition>();
    private const int CardsPerPage = 4;
    private readonly RectTransform[] cardRows = new RectTransform[CardsPerPage];
    private readonly Image[] illustrations = new Image[CardsPerPage];
    private readonly TMP_Text[] cardNames = new TMP_Text[CardsPerPage], descriptions = new TMP_Text[CardsPerPage];
    private Button previousCards, nextCards;
    private TMP_Text pageNumber;
    private int cardIndex;

    private void BuildCodex()
    {
        catalog = Resources.Load<RunUpgradeCatalog>("RunUpgrades/PrototypeRunUpgradeCatalog");
        cardPage = Box("CardLibrary",page,0,69,360,507);
        for (int i=0;i<CardsPerPage;i++)
        {
            var row = Picture("Card"+i,cardPage,Art("Stats"),11,i*103,338,98);
            cardRows[i] = row.rectTransform;
            var socket = Picture("Socket",row.transform,Art("Roster"),10,16,60,63);
            illustrations[i] = Picture("Artwork",socket.transform,null,7,7,46,49,true);
            cardNames[i] = Text("CardTitle",row.transform,"",80,10,245,28,27,TextAlignmentOptions.MidlineLeft);
            descriptions[i] = Text("Description",row.transform,"",80,39,245,49,20,TextAlignmentOptions.MidlineLeft,true);
        }
        previousCards = Control("PreviousCards",cardPage,Art("Stats"),32,428,81,35,() => ChangeCards(-1));
        Text("Label",previousCards.transform,"PREV",5,4,71,27,21);
        nextCards = Control("NextCards",cardPage,Art("Stats"),247,428,81,35,() => ChangeCards(1));
        Text("Label",nextCards.transform,"NEXT",5,4,71,27,21);
        pageNumber = Text("Page",cardPage,"",115,430,130,30,22);
    }

    private void ShowCards(bool show)
    {
        figures.gameObject.SetActive(!show);
        details.gameObject.SetActive(!show);
        collection.gameObject.SetActive(!show);
        cardPage.gameObject.SetActive(show);
        titleLabel.text = show ? "CARDS" : "CHARACTERS";
        cardsTab.image.sprite = Art(show ? "RosterSelected" : "Stats");
        charactersTab.image.sprite = Art(show ? "Stats" : "RosterSelected");
        if (!show || selected == null) return;
        cards = catalog.Upgrades.Where(card => card != null &&
            (string.IsNullOrEmpty(card.RequiredPlayerId) || card.RequiredPlayerId == selected.definition.PlayerId) &&
            (string.IsNullOrEmpty(card.RequiredAbilityId) || card.RequiredAbilityId == selected.definition.ActiveAbility.AbilityId)).ToArray();
        cardIndex = 0;
        ChangeCards(0);
    }

    private void ChangeCards(int direction)
    {
        int count = Mathf.Max(1, Mathf.CeilToInt(cards.Length / (float)CardsPerPage));
        cardIndex = Mathf.Clamp(cardIndex + direction,0,count-1);
        pageNumber.text = $"{cardIndex+1} / {count}";
        previousCards.interactable = cardIndex > 0;
        nextCards.interactable = cardIndex + 1 < count;
        for (int i=0;i<CardsPerPage;i++)
        {
            int index = cardIndex * CardsPerPage + i;
            cardRows[i].gameObject.SetActive(index < cards.Length);
            if (index >= cards.Length) continue;
            var card = cards[index];
            illustrations[i].sprite = card.Artwork != null ? card.Artwork : card.Theme switch
            {
                RunUpgradeTheme.Survival => Art("Heart"),
                RunUpgradeTheme.Ability => abilityIcon.sprite,
                _ => affinity.sprite
            };
            cardNames[i].text = card.DisplayTitle.ToUpperInvariant();
            descriptions[i].text = card.Description;
        }
    }
}

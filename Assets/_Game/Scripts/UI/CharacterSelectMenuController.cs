using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class CharacterSelectMenuController : MonoBehaviour
{
    private static readonly Color SelectedColor =
        new Color32(151, 91, 170, 255);

    private static readonly Color UnselectedColor =
        new Color32(91, 46, 105, 255);

    private static readonly Color UnavailableColor =
        new Color32(55, 39, 63, 255);

    [Header("Character Options")]
    [SerializeField]
    private Button rattlebonesButton;

    [SerializeField]
    private Button bardleyButton;

    [Header("Selection Presentation")]
    [SerializeField]
    private Image characterPreview;

    [SerializeField]
    private Text statusText;

    [Header("Navigation")]
    [SerializeField]
    private Button startButton;

    [SerializeField]
    private Button backButton;

    private Action startRequested;
    private Action backRequested;
    private string selectedPlayerId;
    private bool initialized;
    private Button levelUpButton;
    private Text progressionFeedback;
    private Coroutine levelFlash;
    private Color previewColor = Color.white;

    public void Initialize(
        Action onStartRequested,
        Action onBackRequested)
    {
        startRequested = onStartRequested;
        backRequested = onBackRequested;

        if (!HasRequiredReferences())
        {
            enabled = false;
            return;
        }

        if (!initialized)
        {
            rattlebonesButton.onClick.AddListener(
                SelectRattlebones
            );

            bardleyButton.onClick.AddListener(
                SelectBardley
            );

            startButton.onClick.AddListener(
                HandleStartRequested
            );

            backButton.onClick.AddListener(
                HandleBackRequested
            );

            initialized = true;
            ConfigureProgression();
        }

        selectedPlayerId =
            CharacterSelectionSettings.SelectedPlayerId;

        Refresh();
    }

    private void OnEnable()
    {
        if (!initialized)
        {
            return;
        }

        selectedPlayerId =
            CharacterSelectionSettings.SelectedPlayerId;

        Refresh();
    }

    private void OnDisable()
    {
        if(levelFlash!=null)StopCoroutine(levelFlash);
        levelFlash=null;
        if(characterPreview!=null)characterPreview.color=previewColor;
    }

    private void OnDestroy()
    {
        if (!initialized)
        {
            return;
        }

        rattlebonesButton.onClick.RemoveListener(
            SelectRattlebones
        );

        bardleyButton.onClick.RemoveListener(
            SelectBardley
        );

        startButton.onClick.RemoveListener(
            HandleStartRequested
        );

        backButton.onClick.RemoveListener(
            HandleBackRequested
        );
    }

    private void SelectRattlebones()
    {
        SelectCharacter(
            CharacterSelectionSettings.RattlebonesPlayerId
        );
    }

    private void SelectBardley()
    {
        SelectCharacter(
            CharacterSelectionSettings.BardleyPlayerId
        );
    }

    private void SelectCharacter(
        string playerId)
    {
        selectedPlayerId = playerId;

        CharacterSelectionSettings.SetSelectedPlayerId(
            playerId
        );

        Refresh();
    }

    private void HandleStartRequested()
    {
        if (!PlayerDefinitionRegistry.IsAvailable(
                selectedPlayerId
            ))
        {
            Refresh();
            return;
        }

        startRequested?.Invoke();
    }

    private void HandleBackRequested()
    {
        backRequested?.Invoke();
    }

    private void Refresh()
    {
        bool rattlebonesAvailable =
            PlayerDefinitionRegistry.IsAvailable(
                CharacterSelectionSettings.RattlebonesPlayerId
            );

        bool bardleyAvailable =
            PlayerDefinitionRegistry.IsAvailable(
                CharacterSelectionSettings.BardleyPlayerId
            );

        bool selectedAvailable =
            PlayerDefinitionRegistry.TryGetDefinition(
                selectedPlayerId,
                out PlayerDefinition selectedDefinition
            );

        SetButtonColor(
            rattlebonesButton,
            CharacterSelectionSettings.RattlebonesPlayerId,
            rattlebonesAvailable
        );

        SetButtonColor(
            bardleyButton,
            CharacterSelectionSettings.BardleyPlayerId,
            bardleyAvailable
        );

        if (startButton != null)
        {
            startButton.interactable =
                selectedAvailable;
        }

        RefreshCharacterPreview(
            selectedDefinition
        );

        if (statusText == null)
        {
            return;
        }

        if (!selectedAvailable)
        {
            statusText.text =
                "BARDLEY\nCHARACTER DATA COMING NEXT\nSTART LOCKED UNTIL BARDLEY IS BUILT";
            return;
        }

        string activeAbilityName =
            selectedDefinition.ActiveAbility != null
                ? selectedDefinition.ActiveAbility.DisplayName
                : "NONE";

        string passiveAbilityName =
            selectedDefinition.PassiveAbility != null
                ? selectedDefinition.PassiveAbility.DisplayName
                : "NONE";

        int level=AccountProgression.Current.Level(selectedPlayerId);
        int next=Mathf.Min(BalanceV1.Current.levelCap,level+1);
        int cost=BalanceV1.Current.UpgradeCost(level);
        int baseAbility=selectedDefinition.ActiveAbility is CrackedGemsAbilityDefinition cracks?cracks.CrackedGemDamage:
            selectedDefinition.ActiveAbility is RoyalDecreeAbilityDefinition decree?decree.DamagePerGem:0;
        statusText.text=$"{GetMenuDisplayName(selectedPlayerId)} - Level {level}\n"+
            $"HP  {selectedDefinition.HealthAtLevel(level)}  >  {selectedDefinition.HealthAtLevel(next)}\n"+
            $"Gem damage  {selectedDefinition.GemDamageAtLevel(level):0.##}  >  {selectedDefinition.GemDamageAtLevel(next):0.##}\n"+
            $"Ability damage  {Mathf.RoundToInt(baseAbility*selectedDefinition.AbilityMultiplierAtLevel(level))}  >  {Mathf.RoundToInt(baseAbility*selectedDefinition.AbilityMultiplierAtLevel(next))}\n"+
            $"Shield cap  {selectedDefinition.ShieldCapAtLevel(level)}  >  {selectedDefinition.ShieldCapAtLevel(next)}\n"+
            $"Gold Coins: {AccountProgression.Current.Gold}";
        if(levelUpButton!=null)
        {
            levelUpButton.GetComponentInChildren<Text>().text=level>=BalanceV1.Current.levelCap?"Maximum Level":$"Level Up - {cost} gold";
            levelUpButton.interactable=level<BalanceV1.Current.levelCap&&AccountProgression.Current.Gold>=cost;
        }
    }

    private void RefreshCharacterPreview(
        PlayerDefinition definition)
    {
        if (characterPreview == null)
        {
            return;
        }

        Sprite previewSprite = null;

        if (definition != null)
        {
            previewSprite =
                definition.MenuPortrait != null
                    ? definition.MenuPortrait
                    : definition.BattleCharacterSprite;
        }

        characterPreview.sprite =
            previewSprite;

        characterPreview.enabled =
            previewSprite != null;
        GameplayPixelGrid.FitImage(characterPreview,new Vector2(112,112));
    }

    private void ConfigureProgression()
    {
        previewColor=characterPreview.color;
        Place(rattlebonesButton.transform,new Vector2(-110,210),new Vector2(205,50));
        Place(bardleyButton.transform,new Vector2(110,210),new Vector2(205,50));
        Place(characterPreview.transform,new Vector2(0,117),new Vector2(112,112));characterPreview.preserveAspect=true;
        Place(statusText.transform,new Vector2(0,-30),new Vector2(430,180));statusText.fontSize=21;statusText.font=GameUi.Font;
        Place(startButton.transform,new Vector2(-110,-245),new Vector2(200,48));
        Place(backButton.transform,new Vector2(110,-245),new Vector2(200,48));
        levelUpButton=GameUi.Button("LevelUp",transform,"",new Vector2(330,48),new Vector2(0,-150),LevelUp);
        progressionFeedback=GameUi.Label("UnlockFeedback",transform,"Shared bombs unlock at levels 2, 3, 4 and 5.",new Vector2(440,55),new Vector2(0,-200),17);
    }
    private static void Place(Transform target,Vector2 position,Vector2 size)
    {
        var rect=target as RectTransform;if(rect==null)return;
        rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0.5f,0.5f);rect.anchoredPosition=position;rect.sizeDelta=size;
    }
    private void LevelUp()
    {
        int highest=AccountProgression.Current.HighestLevel;
        if(!AccountProgression.Current.TryLevelUp(selectedPlayerId))
        {if(AccountProgression.Current.LastError!=null)progressionFeedback.text="Save failed. Check storage, then try again.";return;}
        Refresh();
        string unlocked="";
        foreach(GemSpecialType type in Enum.GetValues(typeof(GemSpecialType)))
            if(BalanceV1.Current.UnlockLevel(type)>highest&&AccountProgression.Current.IsUnlocked(type))
            {
                if(type==GemSpecialType.ColumnBomb)continue;
                unlocked+=type==GemSpecialType.RowBomb?"Directional Bombs":type==GemSpecialType.PoisonBomb?"Poison Bomb":type==GemSpecialType.HealingBomb?"Healing Bomb":"Shield Bomb";
            }
        progressionFeedback.text=unlocked.Length>0?$"Unlocked for every character: {unlocked}":"Level up! Permanent stats increased.";
        if(levelFlash!=null)StopCoroutine(levelFlash);
        levelFlash=StartCoroutine(LevelFlash());
    }
    private IEnumerator LevelFlash()
    {
        var original=previewColor;
        for(float time=0;time<0.3f;time+=Time.unscaledDeltaTime)
        {
            characterPreview.color=Color.Lerp(new Color(1.8f,1.8f,1.8f),original,time/0.3f);
            yield return null;
        }
        characterPreview.color=original;
        levelFlash=null;
    }

    private void SetButtonColor(
        Button button,
        string playerId,
        bool available)
    {
        if (button == null ||
            button.targetGraphic == null)
        {
            return;
        }

        if (string.Equals(
                selectedPlayerId,
                playerId,
                StringComparison.Ordinal
            ))
        {
            button.targetGraphic.color =
                SelectedColor;
            return;
        }

        button.targetGraphic.color =
            available
                ? UnselectedColor
                : UnavailableColor;
    }

    private bool HasRequiredReferences()
    {
        bool hasAllReferences =
            rattlebonesButton != null &&
            bardleyButton != null &&
            characterPreview != null &&
            statusText != null &&
            startButton != null &&
            backButton != null;

        if (hasAllReferences)
        {
            return true;
        }

        Debug.LogError(
            "CharacterSelectMenuController requires both character buttons, " +
            "preview/status presentation, and Start/Back buttons.",
            this
        );

        return false;
    }

    private static string GetMenuDisplayName(
        string playerId)
    {
        return string.Equals(
                playerId,
                CharacterSelectionSettings.BardleyPlayerId,
                StringComparison.Ordinal
            )
            ? "BARDLEY"
            : "RATTLEBONES";
    }
}

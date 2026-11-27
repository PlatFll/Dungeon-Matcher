using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed partial class CharacterSelectMenuController : MonoBehaviour
{
    [Serializable]
    private sealed class RosterEntry
    {
        public PlayerDefinition definition;
        public string title, cardName;
        [NonSerialized] public Image frame;
        [NonSerialized] public TMP_Text levelLabel;
    }

    [SerializeField] private RosterEntry[] roster;
    private RosterEntry selected;
    private AccountProgression account;
    private EnemyWeaknessIndicatorConfig gemIcons;
    private Action startRequested, backRequested, masteryRequested;
    private Coroutine flash;
    private bool showingCards;
    private string resetId;

    public void Initialize(Action start, Action back, Action mastery)
    {
        startRequested = start;
        backRequested = back;
        masteryRequested = mastery;
        if (page == null)
        {
            account = AccountProgression.Current;
            gemIcons = Resources.Load<EnemyWeaknessIndicatorConfig>("UI/EnemyWeaknessIndicatorConfig");
            roster = roster.Where(e => e.definition != null &&
                CharacterSelectionSettings.IsSelectableCharacter(e.definition.PlayerId)).ToArray();
            BuildScreen();
        }
        OpenCharacters();
    }

    private void OnEnable()
    {
        AccountProgression.Current.Changed += Refresh;
        if (page != null) OpenCharacters();
    }

    private void OnDisable()
    {
        AccountProgression.Current.Changed -= Refresh;
        CloseReset();
        if (flash != null) StopCoroutine(flash);
        flash = null;
        if (hero != null) hero.color = Color.white;
    }

    private void OpenCharacters()
    {
        selected = Array.Find(roster, e => e.definition.PlayerId == CharacterSelectionSettings.SelectedPlayerId) ??
            roster.FirstOrDefault();
        ShowCards(false);
        feedback.text = "";
        Refresh();
        Layout();
    }

    private void Select(RosterEntry entry)
    {
        CloseReset();
        CharacterSelectionSettings.SetSelectedPlayerId(entry.definition.PlayerId);
        selected = entry;
        feedback.text = "";
        Refresh();
    }

    private void Refresh()
    {
        if (selected == null) return;
        var definition = selected.definition;
        var ability = definition.ActiveAbility;
        int level = account.Level(definition.PlayerId);
        bool capped = level >= BalanceV1.Current.levelCap;
        int next = capped ? level : level + 1;
        int cost = BalanceV1.Current.UpgradeCost(level);
        bool savedRun = account.ActiveRun != null;
        wallet.text = account.Gold.ToString("N0");
        SetPreview(definition);
        nameLabel.text = selected.title.ToUpperInvariant();
        affinity.sprite = gemIcons.GetSprite(definition.AffinityGemType);
        var engraving = ability.ReadyStoneEngraving != null ? ability.ReadyStoneEngraving : ability.Icon;
        abilityIcon.sprite = engraving;
        statIcons[3].sprite = engraving;
        Stat(0, level, next, capped);
        Stat(1, CombatAmounts.Health(definition.HealthAtLevel(level)), CombatAmounts.Health(definition.HealthAtLevel(next)), capped);
        Stat(2, definition.GemDamageAtLevel(level), definition.GemDamageAtLevel(next), capped);
        int damage = AbilityHit(definition, level);
        Stat(3, damage, AbilityHit(definition, next), capped);
        Stat(4, ability.EnergyCost, ability.EnergyCost, capped);
        Stat(5, CombatAmounts.Round(definition.ShieldCapAtLevel(level)), CombatAmounts.Round(definition.ShieldCapAtLevel(next)), capped);
        abilityName.text = ability is RoyalDecreeAbilityDefinition ? "ROYAL DECREE" : ability.DisplayName.ToUpperInvariant();
        abilityBody.text = Describe(ability, damage);
        progress.text = capped ? $"LEVEL {level}  /  MAX" : $"LEVEL {level}  >  <color=#B6E889>LEVEL {next}</color>";
        costLabel.text = capped ? "MAX LEVEL" : cost.ToString("N0");
        costCoin.enabled = !capped;
        costLabel.rectTransform.anchoredPosition = new Vector2(capped ? 0 : 8, -22);
        levelUp.interactable = !savedRun && !capped && account.Gold >= cost;
        levelUp.image.sprite = levelUp.interactable ? Art("LevelUp") : ShopArt("Disabled");
        reset.interactable = !savedRun && level > 1;
        startLabel.text = savedRun ? "CONTINUE" : "PLAY";
        if (savedRun) feedback.text = "Finish your saved run to change levels.";
        else if (!capped && account.Gold < cost && string.IsNullOrEmpty(feedback.text))
            feedback.text = $"Need {(cost-account.Gold):N0} more gold to level up.";
        foreach (var entry in roster)
        {
            entry.frame.sprite = Art(entry == selected ? "RosterSelected" : "Roster");
            entry.levelLabel.text = $"LV. {account.Level(entry.definition.PlayerId)}";
        }
    }

    private static int AbilityHit(PlayerDefinition def, int level)
    {
        int hit = def.ActiveAbility switch
        {
            RoyalDecreeAbilityDefinition mark => mark.DamagePerGem,
            CrackedGemsAbilityDefinition cracks => cracks.CrackedGemDamage,
            RicochetAbilityDefinition shots => shots.BaseEnemyDamage,
            _ => 0
        };
        return CombatAmounts.Round(Mathf.RoundToInt(hit * def.AbilityMultiplierAtLevel(level)));
    }

    private static string Describe(CharacterAbilityDefinition ability, int hit)
    {
        switch (ability)
        {
            case RoyalDecreeAbilityDefinition mark:
                return $"Mark a foe for <b>{mark.UnifiedDurationMoves} moves</b>. Each gem cleared deals <b>{hit} bonus damage</b> to it, whatever its color.";
            case CrackedGemsAbilityDefinition cracks:
                return $"Crack up to <b>{cracks.TargetGemCount} gems</b>. Each bursts and deals <b>{hit} damage</b> to foes weak to its color.";
            case RicochetAbilityDefinition shots:
                return $"Fire <b>{shots.Shots} quick shots</b> at the board. Ricochets hit for <b>{hit} damage</b> and grow stronger on repeat hits.";
            case DungeonAttenderAbilityDefinition charge:
                return $"Charge up to <b>{charge.MovementBudget} spaces</b>, smashing obstructions and clearing gems along the way.";
            default:
                return ability.Description;
        }
    }

    private void Stat(int index, float current, float next, bool capped)
    {
        currents[index].text = current.ToString("0.##");
        nexts[index].text = capped ? "" : next.ToString("0.##");
        nexts[index].color = (index == 4 ? next < current : next > current) ? Growth : Cream;
        arrows[index].enabled = !capped;
    }

    private void LevelUp()
    {
        string id = selected.definition.PlayerId;
        var locked = Enum.GetValues(typeof(GemSpecialType)).Cast<GemSpecialType>()
            .Where(type => !account.IsUnlocked(type)).ToArray();
        if (!account.TryLevelUp(id))
        {
            if (account.LastError != null) feedback.text = "Save failed. Check storage and try again.";
            return;
        }
        var unlocks = locked.Where(account.IsUnlocked).Select(type => type.ToString().Replace("Bomb", " Bomb"));
        string names = string.Join(", ", unlocks);
        feedback.text = names.Length > 0 ? $"Unlocked for every character: {names}" : $"Level {account.Level(id)} reached!";
        Refresh();
        if (flash != null) StopCoroutine(flash);
        if (!PresentationPreferences.ReducedMotion) flash = StartCoroutine(UpgradeFlash());
    }

    private IEnumerator UpgradeFlash()
    {
        for (float t = 0; t < .32f; t += Time.unscaledDeltaTime)
        {
            hero.color = Color.Lerp(new Color(1f,.82f,.45f), Color.white, t / .32f);
            yield return null;
        }
        hero.color = Color.white;
        flash = null;
    }

    private void OpenReset()
    {
        if (!reset.interactable) return;
        resetId = selected.definition.PlayerId;
        resetMessage.text = $"Reset {selected.title} to level 1?\n\nEarned bomb unlocks and inventory stay. Upgrade gold is not refunded.";
        modal.gameObject.SetActive(true);
    }

    private void CloseReset()
    {
        resetId = null;
        if (modal != null) modal.gameObject.SetActive(false);
    }

    private void ConfirmReset()
    {
        string id = resetId;
        CloseReset();
        if (id == null) return;
        feedback.text = account.TryResetLevel(id) ? "Level reset. Earned bomb unlocks kept." : "Level could not be reset.";
        Refresh();
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UpgradeCardView : MonoBehaviour
{
    public const float CardWidth = 152f;
    public const float CardHeight = 280f;

    private static readonly Color32 CommonColor =
        new Color32(238, 238, 238, 255);
    private static readonly Color32 UncommonColor =
        new Color32(82, 198, 89, 255);
    private static readonly Color32 RareColor =
        new Color32(69, 139, 230, 255);
    private static readonly Color32 EpicColor =
        new Color32(180, 83, 225, 255);

    private Button button;
    private TMP_Text titleText;
    private Image artworkImage;
    private TMP_Text descriptionText;
    private RunUpgradeDefinition definition;
    private Action<RunUpgradeDefinition> selected;

    public void Initialize(
        Button cardButton,
        TMP_Text title,
        Image artwork,
        TMP_Text description)
    {
        button = cardButton;
        titleText = title;
        artworkImage = artwork;
        descriptionText = description;
    }

    public void Bind(
        RunUpgradeDefinition upgrade,
        Action<RunUpgradeDefinition> onSelected)
    {
        definition = upgrade;
        selected = onSelected;

        if (titleText != null)
        {
            titleText.text = upgrade != null
                ? upgrade.DisplayTitle
                : string.Empty;
        }

        if (descriptionText != null)
        {
            descriptionText.text = upgrade != null
                ? upgrade.Description
                : string.Empty;
        }

        if (artworkImage != null)
        {
            artworkImage.sprite = upgrade != null
                ? upgrade.Artwork
                : null;
            artworkImage.enabled = upgrade != null && upgrade.Artwork != null;
        }

        ApplyRarityColor(upgrade);

        if (button != null)
        {
            button.onClick.RemoveListener(HandleClicked);
            button.onClick.AddListener(HandleClicked);
            button.interactable = upgrade != null;
        }

        gameObject.SetActive(upgrade != null);
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable && definition != null;
        }
    }

    private void ApplyRarityColor(RunUpgradeDefinition upgrade)
    {
        if (upgrade == null)
        {
            return;
        }

        Color rarityColor = GetRarityColor(upgrade.Rarity);
        Image frame = GetComponent<Image>();

        if (frame != null)
        {
            frame.color = rarityColor;
        }

        if (button == null)
        {
            return;
        }

        ColorBlock colors = button.colors;
        colors.normalColor = rarityColor;
        colors.highlightedColor = Color.Lerp(rarityColor, Color.white, 0.22f);
        colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = Color.Lerp(rarityColor, Color.black, 0.18f);
        colors.disabledColor = Color.Lerp(rarityColor, Color.black, 0.52f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.06f;
        button.colors = colors;
    }

    public static Color GetRarityColor(RunUpgradeRarity rarity)
    {
        switch (rarity)
        {
            case RunUpgradeRarity.Uncommon:
                return UncommonColor;
            case RunUpgradeRarity.Rare:
                return RareColor;
            case RunUpgradeRarity.Epic:
                return EpicColor;
            default:
                return CommonColor;
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClicked);
        }
    }

    private void HandleClicked()
    {
        if (definition != null && button != null && button.interactable)
        {
            selected?.Invoke(definition);
        }
    }
}

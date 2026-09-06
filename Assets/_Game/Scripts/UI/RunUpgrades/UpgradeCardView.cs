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
        new Color32(245, 245, 250, 255);
    private static readonly Color32 UncommonColor =
        new Color32(82, 198, 89, 255);
    private static readonly Color32 RareColor =
        new Color32(69, 139, 230, 255);
    private static readonly Color32 EpicColor =
        new Color32(180, 83, 225, 255);

    private static readonly Color32 LightTextColor =
        new Color32(247, 240, 255, 255);
    private static readonly Color32 DarkTextColor =
        new Color32(39, 29, 48, 255);

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

        ApplyRarityTheme(upgrade);

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

    private void ApplyRarityTheme(RunUpgradeDefinition upgrade)
    {
        if (upgrade == null)
        {
            return;
        }

        Color rarityColor = GetRarityColor(upgrade.Rarity);
        bool isCommon = upgrade.Rarity == RunUpgradeRarity.Common;

        Image frame = GetComponent<Image>();
        Image body = transform.Find("CardBody")?.GetComponent<Image>();
        Image artworkSection = transform
            .Find("CardBody/ArtworkSection")?.GetComponent<Image>();

        if (frame != null)
        {
            frame.color = rarityColor;
        }

        if (isCommon)
        {
            if (body != null)
            {
                body.color = new Color32(226, 226, 234, 255);
            }

            if (artworkSection != null)
            {
                artworkSection.color = new Color32(202, 202, 214, 255);
            }

            if (titleText != null)
            {
                titleText.color = DarkTextColor;
            }

            if (descriptionText != null)
            {
                descriptionText.color = DarkTextColor;
            }
        }
        else
        {
            if (body != null)
            {
                body.color = Color.Lerp(rarityColor, Color.black, 0.66f);
            }

            if (artworkSection != null)
            {
                artworkSection.color = Color.Lerp(
                    rarityColor,
                    Color.black,
                    0.76f
                );
            }

            if (titleText != null)
            {
                titleText.color = LightTextColor;
            }

            if (descriptionText != null)
            {
                descriptionText.color = LightTextColor;
            }
        }

        if (button == null)
        {
            return;
        }

        ColorBlock colors = button.colors;
        colors.normalColor = rarityColor;
        colors.highlightedColor =
            Color.Lerp(rarityColor, Color.white, 0.22f);
        colors.selectedColor = colors.highlightedColor;
        colors.pressedColor =
            Color.Lerp(rarityColor, Color.black, 0.18f);
        colors.disabledColor =
            Color.Lerp(rarityColor, Color.black, 0.52f);
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

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UpgradeCardView : MonoBehaviour
{
    public const float CardWidth = 152f;
    public const float CardHeight = 280f;

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

using TMPro;
using UnityEngine;

/// <summary>Shared family for legacy UI and TextMesh Pro. Replace both references together.</summary>
[CreateAssetMenu(menuName = "Dungeon Matcher/UI Typography")]
public sealed class UiTypography : ScriptableObject
{
    public Font regularFont;
    public TMP_FontAsset textMeshProFont;
}

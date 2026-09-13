using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Presentation-only white silhouette that fills the weakness gem from bottom
/// to top as stagger pressure builds. While staggered, EnemyStagger drains the
/// same normalized meter back to zero, so the white fill visibly empties.
/// </summary>
[DisallowMultipleComponent]
public sealed class EnemyStaggerWeaknessFillUI : MonoBehaviour
{
    private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
    private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");

    private EnemyStagger stagger;
    private Image baseImage;
    private Image fillImage;
    private Material fillMaterial;

    public static bool TryInstall(EnemyStagger staggerSource)
    {
        if (staggerSource == null)
        {
            return false;
        }

        EnemySlotUI slot = staggerSource.GetComponentInParent<EnemySlotUI>();
        if (slot == null)
        {
            return false;
        }

        EnemyWeaknessIndicatorUI indicator =
            slot.GetComponentInChildren<EnemyWeaknessIndicatorUI>(true);
        if (indicator == null)
        {
            return false;
        }

        EnemyStaggerWeaknessFillUI presenter =
            indicator.GetComponent<EnemyStaggerWeaknessFillUI>();
        if (presenter == null)
        {
            presenter = indicator.gameObject.AddComponent<EnemyStaggerWeaknessFillUI>();
        }

        presenter.Bind(staggerSource);
        return true;
    }

    private void Awake()
    {
        baseImage = GetComponent<Image>();
        EnsureFillImage();
    }

    public void Bind(EnemyStagger staggerSource)
    {
        stagger = staggerSource;
        if (baseImage == null)
        {
            baseImage = GetComponent<Image>();
        }

        EnsureFillImage();
        RefreshFill();
    }

    private void LateUpdate()
    {
        RefreshFill();
    }

    private void EnsureFillImage()
    {
        if (baseImage == null)
        {
            baseImage = GetComponent<Image>();
        }

        if (fillImage == null)
        {
            Transform existing = transform.Find("StaggerWhiteFill");
            if (existing != null)
            {
                fillImage = existing.GetComponent<Image>();
            }
        }

        if (fillImage == null)
        {
            GameObject fillObject = new GameObject(
                "StaggerWhiteFill",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );
            fillObject.transform.SetParent(transform, false);
            fillImage = fillObject.GetComponent<Image>();

            RectTransform rect = fillImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        fillImage.raycastTarget = false;
        fillImage.preserveAspect = true;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Vertical;
        fillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
        fillImage.fillClockwise = true;
        fillImage.color = Color.white;
        fillImage.transform.SetAsLastSibling();
    }

    private void EnsureFillMaterial()
    {
        if (fillMaterial != null ||
            baseImage == null ||
            baseImage.material == null ||
            !baseImage.material.HasProperty(FlashAmountId))
        {
            return;
        }

        fillMaterial = new Material(baseImage.material)
        {
            name = "Enemy Weakness Stagger White Fill (Runtime)"
        };

        fillMaterial.SetFloat(FlashAmountId, 1f);
        if (fillMaterial.HasProperty(FlashColorId))
        {
            fillMaterial.SetColor(FlashColorId, Color.white);
        }

        fillImage.material = fillMaterial;
    }

    private void RefreshFill()
    {
        if (fillImage == null || baseImage == null)
        {
            EnsureFillImage();
        }

        if (fillImage == null || baseImage == null)
        {
            return;
        }

        EnsureFillMaterial();

        float amount = stagger != null &&
                       stagger.EnemyActor != null &&
                       stagger.EnemyActor.IsInitialized &&
                       !stagger.EnemyActor.IsDefeated
            ? stagger.StaggerMeterNormalized
            : 0f;

        fillImage.sprite = baseImage.sprite;
        fillImage.fillAmount = Mathf.Clamp01(amount);
        fillImage.enabled = baseImage.enabled &&
                            baseImage.sprite != null &&
                            amount > 0.0001f;
    }

    private void OnDestroy()
    {
        if (fillMaterial != null)
        {
            Destroy(fillMaterial);
            fillMaterial = null;
        }
    }
}
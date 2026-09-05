using UnityEngine;

[DisallowMultipleComponent]
public sealed class FrozenGemOverlayView : MonoBehaviour
{
    private const string OverlayObjectName = "FrozenGemOverlay";

    private GameObject overlayObject;
    private SpriteRenderer overlayRenderer;

    public void Initialize(
        Sprite overlaySprite)
    {
        EnsureOverlay();

        if (overlayRenderer == null)
        {
            return;
        }

        overlayRenderer.sprite = overlaySprite;
        overlayRenderer.enabled = overlaySprite != null;
    }

    public void ReleaseVisual()
    {
        if (overlayObject != null)
        {
            Destroy(overlayObject);
        }

        overlayObject = null;
        overlayRenderer = null;
    }

    private void EnsureOverlay()
    {
        if (overlayObject == null)
        {
            Transform existing =
                transform.Find(OverlayObjectName);

            if (existing != null)
            {
                overlayObject = existing.gameObject;
            }
            else
            {
                overlayObject =
                    new GameObject(OverlayObjectName);

                overlayObject.transform.SetParent(
                    transform,
                    false
                );
            }
        }

        if (overlayRenderer == null &&
            overlayObject != null)
        {
            overlayRenderer =
                overlayObject.GetComponent<SpriteRenderer>();

            if (overlayRenderer == null)
            {
                overlayRenderer =
                    overlayObject.AddComponent<SpriteRenderer>();
            }
        }

        if (overlayRenderer == null)
        {
            return;
        }

        overlayObject.transform.localPosition = Vector3.zero;
        overlayObject.transform.localRotation = Quaternion.identity;
        overlayObject.transform.localScale = Vector3.one;

        SpriteRenderer gemRenderer =
            GetComponent<SpriteRenderer>();

        overlayRenderer.sortingLayerName =
            gemRenderer != null
                ? gemRenderer.sortingLayerName
                : "Gems";

        overlayRenderer.sortingOrder =
            gemRenderer != null
                ? gemRenderer.sortingOrder + 12
                : 12;

        overlayRenderer.maskInteraction =
            SpriteMaskInteraction.VisibleInsideMask;

        overlayRenderer.color = Color.white;
    }

    private void OnDestroy()
    {
        ReleaseVisual();
    }
}

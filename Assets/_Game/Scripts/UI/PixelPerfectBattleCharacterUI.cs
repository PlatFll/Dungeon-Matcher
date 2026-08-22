using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keeps the battle-character presentation physically pixel-aligned without
/// allowing per-character scale rounding to distort the tuned player/enemy
/// size relationship.
///
/// The policy deliberately uses one shared, bounded scale correction for the
/// player/enemy group. If a nearby correction materially improves integer-pixel
/// magnification, everybody receives the same correction. If the closest clean
/// integer step would require a large size jump, the requested layout scale is
/// preserved instead.
/// </summary>
[DefaultExecutionOrder(10000)]
[DisallowMultipleComponent]
public sealed class PixelPerfectBattleCharacterUI : MonoBehaviour
{
    private const string PlayerCharacterName =
        "PlayerCharacter";

    private const string EnemySpawnAnchorName =
        "EnemySpawnAnchor";

    private const string EnemyVisualRootName =
        "VisualRoot";

    private const float MinimumScale = 0.0001f;

    /*
     * A hard safety limit. The pixel policy is never allowed to change the
     * layout-authored character sizes by more than eight percent. This is the
     * guard that prevents the old 20-30% size jumps that independent integer
     * rounding can cause near a resolution threshold.
     */
    private const float MaximumSharedScaleCorrection = 0.08f;

    /*
     * Do not make a visible scale correction for a tiny mathematical win. The
     * nearest-integer score has to improve by a meaningful amount first.
     */
    private const float MinimumScoreImprovement = 0.04f;

    /*
     * Slightly prefer keeping the exact authored presentation size when two
     * candidate corrections produce almost the same pixel quality.
     */
    private const float ScaleChangePenalty = 0.35f;

    private const float CandidateStep = 0.001f;

    private static readonly List<
        PixelPerfectBattleCharacterUI
    > ActiveInstances = new();

    private static readonly Dictionary<int, int>
        LastProcessedFrameByCanvas = new();

    private static readonly Dictionary<int, float>
        SharedCorrectionByCanvas = new();

    private RectTransform scaleRoot;
    private RectTransform movingVisualRoot;
    private Image characterImage;
    private Canvas rootCanvas;

    private float requestedUniformScale = 1f;
    private float lastAppliedUniformScale = float.NaN;

    private bool isPlayerRoot;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetStaticState()
    {
        ActiveInstances.Clear();
        LastProcessedFrameByCanvas.Clear();
        SharedCorrectionByCanvas.Clear();
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad
    )]
    private static void InstallForBattleCharacters()
    {
        RectTransform[] rects =
            Object.FindObjectsByType<RectTransform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (RectTransform rect in rects)
        {
            if (rect == null ||
                (rect.name != PlayerCharacterName &&
                 rect.name != EnemySpawnAnchorName))
            {
                continue;
            }

            if (!rect.TryGetComponent(
                    out PixelPerfectBattleCharacterUI _
                ))
            {
                rect.gameObject.AddComponent<
                    PixelPerfectBattleCharacterUI
                >();
            }
        }
    }

    private void Awake()
    {
        ResolveReferences();
        CaptureRequestedScaleFromLayout();
        EnsurePixelPerfectCanvas();
    }

    private void OnEnable()
    {
        if (!ActiveInstances.Contains(this))
        {
            ActiveInstances.Add(this);
        }

        ResolveReferences();
        CaptureRequestedScaleFromLayout();
        EnsurePixelPerfectCanvas();
    }

    private void OnDisable()
    {
        ActiveInstances.Remove(this);
    }

    private void OnDestroy()
    {
        ActiveInstances.Remove(this);
    }

    private void LateUpdate()
    {
        if (scaleRoot == null)
        {
            ResolveReferences();
        }

        if (scaleRoot == null)
        {
            return;
        }

        CaptureExternalScaleRequest();
        ResolveDynamicVisualReferences();
        EnsurePixelPerfectCanvas();

        SnapRectToPhysicalPixelGrid(
            scaleRoot
        );

        if (movingVisualRoot != null &&
            movingVisualRoot != scaleRoot)
        {
            SnapRectToPhysicalPixelGrid(
                movingVisualRoot
            );
        }

        ApplySharedScalePolicyOncePerCanvas();
    }

    private void ResolveReferences()
    {
        scaleRoot =
            transform as RectTransform;

        isPlayerRoot =
            scaleRoot != null &&
            scaleRoot.name == PlayerCharacterName;

        ResolveDynamicVisualReferences();
    }

    private void ResolveDynamicVisualReferences()
    {
        if (scaleRoot == null)
        {
            return;
        }

        if (isPlayerRoot)
        {
            movingVisualRoot = scaleRoot;
            characterImage =
                scaleRoot.GetComponent<Image>();
        }
        else
        {
            Transform visualTransform =
                FindDescendantByName(
                    scaleRoot,
                    EnemyVisualRootName
                );

            movingVisualRoot =
                visualTransform as RectTransform;

            characterImage =
                visualTransform != null
                    ? visualTransform.GetComponent<Image>()
                    : null;
        }

        rootCanvas =
            characterImage != null
                ? characterImage.canvas
                : GetComponentInParent<Canvas>();
    }

    private void EnsurePixelPerfectCanvas()
    {
        if (rootCanvas == null ||
            rootCanvas.renderMode ==
            RenderMode.WorldSpace)
        {
            return;
        }

        if (!rootCanvas.pixelPerfect)
        {
            rootCanvas.pixelPerfect = true;
        }
    }

    private void CaptureRequestedScaleFromLayout()
    {
        if (scaleRoot == null)
        {
            return;
        }

        requestedUniformScale =
            GetUniformMagnitude(
                scaleRoot.localScale
            );
    }

    private void CaptureExternalScaleRequest()
    {
        if (scaleRoot == null)
        {
            return;
        }

        float currentScale =
            GetUniformMagnitude(
                scaleRoot.localScale
            );

        /*
         * TopBattlePresentationController is the authority for the requested
         * responsive scale. Ignore our own previously-applied shared correction,
         * but immediately accept a genuinely new scale written by the layout.
         */
        if (float.IsNaN(
                lastAppliedUniformScale
            ) ||
            !Mathf.Approximately(
                currentScale,
                lastAppliedUniformScale
            ))
        {
            requestedUniformScale =
                currentScale;
        }
    }

    private void ApplySharedScalePolicyOncePerCanvas()
    {
        if (rootCanvas == null)
        {
            return;
        }

        int canvasId =
            rootCanvas.GetInstanceID();

        if (LastProcessedFrameByCanvas.TryGetValue(
                canvasId,
                out int processedFrame
            ) &&
            processedFrame == Time.frameCount)
        {
            return;
        }

        LastProcessedFrameByCanvas[canvasId] =
            Time.frameCount;

        float playerMagnitudeTotal = 0f;
        int playerMagnitudeCount = 0;

        float enemyMagnitudeTotal = 0f;
        int enemyMagnitudeCount = 0;

        for (int index =
                 ActiveInstances.Count - 1;
             index >= 0;
             index--)
        {
            PixelPerfectBattleCharacterUI instance =
                ActiveInstances[index];

            if (instance == null)
            {
                ActiveInstances.RemoveAt(index);
                continue;
            }

            instance.ResolveDynamicVisualReferences();

            /*
             * The first PixelPerfectBattleCharacterUI that reaches LateUpdate
             * evaluates the entire Canvas. Other roots may not have executed
             * their own LateUpdate yet, so capture every layout-authored scale
             * here before scoring the shared correction.
             */
            instance.CaptureExternalScaleRequest();

            if (instance.rootCanvas != rootCanvas ||
                !instance.TryGetRequestedPhysicalMagnification(
                    out float magnitude
                ))
            {
                continue;
            }

            if (instance.isPlayerRoot)
            {
                playerMagnitudeTotal += magnitude;
                playerMagnitudeCount++;
            }
            else
            {
                enemyMagnitudeTotal += magnitude;
                enemyMagnitudeCount++;
            }
        }

        if (playerMagnitudeCount == 0)
        {
            ApplyCorrectionToCanvasGroup(
                rootCanvas,
                1f
            );

            SharedCorrectionByCanvas[canvasId] =
                1f;

            return;
        }

        /*
         * Empty enemy slots are normal between waves. Keep the last established
         * shared correction during that gap instead of shrinking/growing the
         * player back to 1x and then changing size again on the next spawn.
         */
        if (enemyMagnitudeCount == 0)
        {
            float heldCorrection =
                SharedCorrectionByCanvas.TryGetValue(
                    canvasId,
                    out float storedCorrection
                )
                    ? storedCorrection
                    : 1f;

            ApplyCorrectionToCanvasGroup(
                rootCanvas,
                heldCorrection
            );

            return;
        }

        /*
         * Once both roles are available, evaluate them as two equal presentation
         * groups no matter how many enemies are alive. This prevents a
         * three-enemy wave from overpowering the player in the scale calculation.
         */
        float playerMagnitude =
            playerMagnitudeTotal /
            playerMagnitudeCount;

        float enemyMagnitude =
            enemyMagnitudeTotal /
            enemyMagnitudeCount;

        float previousCorrection =
            SharedCorrectionByCanvas.TryGetValue(
                canvasId,
                out float previousStoredCorrection
            )
                ? previousStoredCorrection
                : 1f;

        float correction =
            CalculateSharedCorrection(
                playerMagnitude,
                enemyMagnitude,
                previousCorrection
            );

        SharedCorrectionByCanvas[canvasId] =
            correction;

        ApplyCorrectionToCanvasGroup(
            rootCanvas,
            correction
        );
    }

    private static float CalculateSharedCorrection(
        float playerMagnitude,
        float enemyMagnitude,
        float previousCorrection)
    {
        float minimumCorrection =
            1f - MaximumSharedScaleCorrection;

        float maximumCorrection =
            1f + MaximumSharedScaleCorrection;

        previousCorrection =
            Mathf.Clamp(
                previousCorrection,
                minimumCorrection,
                maximumCorrection
            );

        float uncorrectedScore =
            ScoreCorrection(
                playerMagnitude,
                enemyMagnitude,
                1f
            );

        float bestCorrection = 1f;
        float bestScore = uncorrectedScore;

        for (float candidate = minimumCorrection;
             candidate <=
             maximumCorrection +
             CandidateStep * 0.5f;
             candidate += CandidateStep)
        {
            float score =
                ScoreCorrection(
                    playerMagnitude,
                    enemyMagnitude,
                    candidate
                );

            if (score < bestScore)
            {
                bestScore = score;
                bestCorrection = candidate;
            }
        }

        float improvement =
            uncorrectedScore -
            bestScore;

        if (improvement <
            MinimumScoreImprovement)
        {
            bestCorrection = 1f;
            bestScore = uncorrectedScore;
        }

        /*
         * Hysteresis: if the previous shared correction is effectively as good
         * as the new optimum, retain it. This prevents repeated +/- one-pixel
         * presentation toggles around a resolution boundary.
         */
        float previousScore =
            ScoreCorrection(
                playerMagnitude,
                enemyMagnitude,
                previousCorrection
            );

        if (Mathf.Abs(
                previousCorrection -
                bestCorrection
            ) <= 0.01f ||
            previousScore <=
            bestScore + 0.004f)
        {
            return previousCorrection;
        }

        return Mathf.Clamp(
            bestCorrection,
            minimumCorrection,
            maximumCorrection
        );
    }

    private static float ScoreCorrection(
        float playerMagnitude,
        float enemyMagnitude,
        float correction)
    {
        float correctedPlayer =
            playerMagnitude * correction;

        float correctedEnemy =
            enemyMagnitude * correction;

        float playerIntegerError =
            DistanceToNearestInteger(
                correctedPlayer
            );

        float enemyIntegerError =
            DistanceToNearestInteger(
                correctedEnemy
            );

        float scaleChange =
            correction - 1f;

        return
            playerIntegerError *
            playerIntegerError +
            enemyIntegerError *
            enemyIntegerError +
            scaleChange *
            scaleChange *
            ScaleChangePenalty;
    }

    private static float DistanceToNearestInteger(
        float value)
    {
        if (value <= 0f)
        {
            return 1f;
        }

        int nearestInteger =
            Mathf.Max(
                1,
                Mathf.RoundToInt(value)
            );

        return Mathf.Abs(
            value -
            nearestInteger
        );
    }

    private static void ApplyCorrectionToCanvasGroup(
        Canvas canvas,
        float correction)
    {
        for (int index =
                 ActiveInstances.Count - 1;
             index >= 0;
             index--)
        {
            PixelPerfectBattleCharacterUI instance =
                ActiveInstances[index];

            if (instance == null)
            {
                ActiveInstances.RemoveAt(index);
                continue;
            }

            if (instance.rootCanvas != canvas ||
                instance.scaleRoot == null)
            {
                continue;
            }

            instance.ApplySharedCorrection(
                correction
            );
        }
    }

    private void ApplySharedCorrection(
        float correction)
    {
        if (scaleRoot == null)
        {
            return;
        }

        float correctedScale =
            Mathf.Max(
                MinimumScale,
                requestedUniformScale *
                correction
            );

        Vector3 currentScale =
            scaleRoot.localScale;

        float signX =
            currentScale.x < 0f
                ? -1f
                : 1f;

        float signY =
            currentScale.y < 0f
                ? -1f
                : 1f;

        scaleRoot.localScale =
            new Vector3(
                correctedScale * signX,
                correctedScale * signY,
                Mathf.Approximately(
                    currentScale.z,
                    0f
                )
                    ? 1f
                    : currentScale.z
            );

        lastAppliedUniformScale =
            correctedScale;

        /*
         * A changed root scale changes the final screen-space presentation.
         * Re-snap immediately so a resolution transition cannot leave one bad
         * frame before this component's next LateUpdate.
         */
        SnapRectToPhysicalPixelGrid(
            scaleRoot
        );

        if (movingVisualRoot != null &&
            movingVisualRoot != scaleRoot)
        {
            SnapRectToPhysicalPixelGrid(
                movingVisualRoot
            );
        }
    }

    private bool TryGetRequestedPhysicalMagnification(
        out float magnitude)
    {
        magnitude = 0f;

        if (scaleRoot == null ||
            characterImage == null ||
            characterImage.sprite == null ||
            rootCanvas == null)
        {
            return false;
        }

        Vector2 imageSize =
            characterImage.rectTransform.rect.size;

        if (imageSize.x <= 0f ||
            imageSize.y <= 0f)
        {
            return false;
        }

        int sourceArtExtent =
            GetSourceArtExtent(
                characterImage.sprite
            );

        if (sourceArtExtent <= 0)
        {
            return false;
        }

        /*
         * Several static enemies are imported from a 64x64 source image but
         * their Sprite rect is tightly trimmed around the opaque pixels. Using
         * the next power-of-two art extent reconstructs the intended 64x64 art
         * cell for scale calculations, so enemy swaps do not each choose a
         * different global scale merely because their transparent margins were
         * sliced differently.
         */
        float logicalSourcePixelMagnification =
            Mathf.Min(
                imageSize.x /
                sourceArtExtent,
                imageSize.y /
                sourceArtExtent
            );

        float hierarchyScaleWithoutRoot =
            GetHierarchyScaleExcludingRoot();

        float canvasScale =
            Mathf.Max(
                MinimumScale,
                rootCanvas.scaleFactor
            );

        magnitude =
            logicalSourcePixelMagnification *
            hierarchyScaleWithoutRoot *
            canvasScale *
            Mathf.Max(
                MinimumScale,
                requestedUniformScale
            );

        return magnitude > MinimumScale;
    }

    private static int GetSourceArtExtent(
        Sprite sprite)
    {
        if (sprite == null)
        {
            return 0;
        }

        int visibleExtent =
            Mathf.Max(
                1,
                Mathf.CeilToInt(
                    Mathf.Max(
                        sprite.rect.width,
                        sprite.rect.height
                    )
                )
            );

        return Mathf.NextPowerOfTwo(
            visibleExtent
        );
    }

    private float GetHierarchyScaleExcludingRoot()
    {
        if (scaleRoot == null ||
            characterImage == null)
        {
            return 1f;
        }

        float scale = 1f;

        Transform current =
            characterImage.rectTransform;

        while (current != null &&
               current != scaleRoot)
        {
            scale *=
                GetUniformMagnitude(
                    current.localScale
                );

            current = current.parent;
        }

        if (current != scaleRoot)
        {
            return 1f;
        }

        current = scaleRoot.parent;

        Transform canvasTransform =
            rootCanvas != null
                ? rootCanvas.transform
                : null;

        while (current != null &&
               current != canvasTransform)
        {
            scale *=
                GetUniformMagnitude(
                    current.localScale
                );

            current = current.parent;
        }

        return Mathf.Max(
            MinimumScale,
            scale
        );
    }

    private void SnapRectToPhysicalPixelGrid(
        RectTransform rect)
    {
        if (rect == null ||
            rootCanvas == null ||
            rootCanvas.renderMode ==
            RenderMode.WorldSpace ||
            rect.parent is not RectTransform parentRect)
        {
            return;
        }

        Camera canvasCamera =
            rootCanvas.renderMode ==
            RenderMode.ScreenSpaceOverlay
                ? null
                : rootCanvas.worldCamera;

        /*
         * Snap the actual rendered pivot in physical screen pixels rather than
         * rounding anchoredPosition. Enemy slots use fractional anchors (thirds
         * of the enemy section), so a locally-integer anchoredPosition can still
         * land on a half/sub-pixel after the parent layout and CanvasScaler are
         * applied. Screen-space snapping removes that entire parent-chain error.
         */
        Vector2 currentScreenPoint =
            RectTransformUtility.WorldToScreenPoint(
                canvasCamera,
                rect.position
            );

        Vector2 snappedScreenPoint =
            new Vector2(
                Mathf.Round(currentScreenPoint.x),
                Mathf.Round(currentScreenPoint.y)
            );

        if (Mathf.Approximately(
                currentScreenPoint.x,
                snappedScreenPoint.x
            ) &&
            Mathf.Approximately(
                currentScreenPoint.y,
                snappedScreenPoint.y
            ))
        {
            return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                currentScreenPoint,
                canvasCamera,
                out Vector2 currentParentPoint
            ) ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                snappedScreenPoint,
                canvasCamera,
                out Vector2 snappedParentPoint
            ))
        {
            return;
        }

        rect.anchoredPosition +=
            snappedParentPoint -
            currentParentPoint;
    }

    private static float GetUniformMagnitude(
        Vector3 scale)
    {
        float x = Mathf.Abs(scale.x);
        float y = Mathf.Abs(scale.y);

        return Mathf.Max(
            MinimumScale,
            Mathf.Min(x, y)
        );
    }

    private static Transform FindDescendantByName(
        Transform root,
        string objectName)
    {
        if (root == null)
        {
            return null;
        }

        for (int index = 0;
             index < root.childCount;
             index++)
        {
            Transform child =
                root.GetChild(index);

            if (child.name == objectName)
            {
                return child;
            }

            Transform nested =
                FindDescendantByName(
                    child,
                    objectName
                );

            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private void OnTransformParentChanged()
    {
        ResolveReferences();
        CaptureRequestedScaleFromLayout();
    }
}

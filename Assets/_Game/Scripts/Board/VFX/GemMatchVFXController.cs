using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GemBurstStrength
{
    Small,
    Medium,
    Large
}

[DisallowMultipleComponent]
public sealed class GemMatchVFXController :
    MonoBehaviour
{
    [Header("Board")]
    [SerializeField]
    private BoardController boardController;

    [SerializeField]
    private Transform particleContainer;

    [Header("Assets")]
    [SerializeField]
    private GemPixelParticle particlePrefab;

    [SerializeField]
    private GemColorPalette gemColorPalette;

    [Header("Rendering")]
    [SerializeField]
    private string sortingLayerName =
        "Gems";

    [SerializeField]
    private int sortingOrder = 3;

    [Header("Particles Per Gem")]
    [SerializeField, Min(1)]
    private int threeMatchParticles = 4;

    [SerializeField, Min(1)]
    private int fourMatchParticles = 5;

    [SerializeField, Min(1)]
    private int fivePlusMatchParticles = 6;

    [Header("Particle Appearance")]
    [SerializeField]
    private Vector2 particleSizeRange =
        new Vector2(
            0.035f,
            0.055f
        );

    [SerializeField, Range(0f, 0.5f)]
    private float maximumWhiteVariation =
        0.3f;

    [SerializeField]
    private Vector2 shardAspectRatioRange =
    new Vector2(
        1.25f,
        2.25f
    );

    [SerializeField]
    private Vector2 shardSpinRange =
        new Vector2(
            -720f,
            720f
        );

    [Header("Particle Motion")]
    [SerializeField]
    private Vector2 speedRange =
        new Vector2(
            2.6f,
            4f
        );

    [SerializeField]
    private Vector2 lifetimeRange =
        new Vector2(
            0.20f,
            0.30f
        );

    [SerializeField, Min(0f)]
    private float spawnRadius = 0.04f;

    [SerializeField, Range(0f, 1f)]
    private float upwardBias = 0.04f;

    [SerializeField, Min(0f)]
    private float shardGravity = 0.15f;

    [SerializeField, Min(0f)]
    private float shardMovementDrag =
    0.65f;

    [Header("Match Intensity")]
    [SerializeField, Min(0f)]
    private float fourMatchIntensityBonus =
        0.12f;

    [SerializeField, Min(0f)]
    private float fiveMatchIntensityBonus =
        0.24f;

    [SerializeField, Min(0f)]
    private float cascadeIntensityPerDepth =
        0.08f;

    [SerializeField, Min(0f)]
    private float maximumCascadeBonus =
        0.32f;

    [Header("Pooling")]
    [SerializeField, Min(0)]
    private int prewarmCount = 72;

    [SerializeField, Min(1)]
    private int maximumParticleCount = 120;

    private readonly Queue<GemPixelParticle>
        availableParticles =
            new Queue<GemPixelParticle>();

    private readonly HashSet<GemPixelParticle>
        activeParticles =
            new HashSet<GemPixelParticle>();

    private int createdParticleCount;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();
    }

    private void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        PrewarmPool();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void ResolveReferences()
    {
        if (boardController == null)
        {
            boardController =
                GetComponent<BoardController>();
        }

        if (particleContainer == null)
        {
            particleContainer =
                transform;
        }
    }

    private void Subscribe()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.GemMatchVFXRequested -=
            HandleGemMatchVFXRequested;

        boardController.GemMatchVFXRequested +=
            HandleGemMatchVFXRequested;
    }

    private void Unsubscribe()
    {
        if (boardController == null)
        {
            return;
        }

        boardController.GemMatchVFXRequested -=
            HandleGemMatchVFXRequested;
    }

    private void HandleGemMatchVFXRequested(
        GemMatchVFXContext context)
    {
        if (context.WorldPositions == null ||
            context.WorldPositions.Length == 0)
        {
            return;
        }

        if (context.BurstDelay <= 0f)
        {
            PlayMatchBurst(
                context
            );

            return;
        }

        StartCoroutine(
            PlayMatchBurstAfterDelay(
                context
            )
        );
    }

    private IEnumerator
        PlayMatchBurstAfterDelay(
            GemMatchVFXContext context)
    {
        yield return new WaitForSeconds(
            context.BurstDelay
        );

        PlayMatchBurst(
            context
        );
    }

    private void PlayMatchBurst(
        GemMatchVFXContext context)
    {
        int particlesPerGem =
            GetParticlesPerGem(
                context.GemCount
            );

        float matchIntensity =
            GetMatchIntensity(
                context.GemCount
            );

        float cascadeBonus =
            Mathf.Min(
                context.CascadeDepth *
                cascadeIntensityPerDepth,
                maximumCascadeBonus
            );

        float totalIntensity =
            matchIntensity +
            cascadeBonus;

        Color baseColor =
            gemColorPalette.GetColor(
                context.GemType
            );

        foreach (
            Vector3 worldPosition
            in context.WorldPositions)
        {
            Vector3 localPosition =
                particleContainer
                    .InverseTransformPoint(
                        worldPosition
                    );

            SpawnBurst(
                localPosition,
                baseColor,
                particlesPerGem,
                totalIntensity
            );
        }
    }

    public void PlayBurstAtWorldPosition(
        GemType gemType,
        Vector3 worldPosition,
        GemBurstStrength strength)
    {
        int particleCount;
        float intensity;

        switch (strength)
        {
            case GemBurstStrength.Medium:
                particleCount = 8;
                intensity = 1.2f;
                break;

            case GemBurstStrength.Large:
                particleCount = 12;
                intensity = 1.4f;
                break;

            default:
                particleCount = 5;
                intensity = 1f;
                break;
        }

        Vector3 localPosition =
            particleContainer
                .InverseTransformPoint(
                    worldPosition
                );

        Color color =
            gemColorPalette.GetColor(
                gemType
            );

        SpawnBurst(
            localPosition,
            color,
            particleCount,
            intensity
        );
    }

    private void SpawnBurst(
        Vector3 localPosition,
        Color baseColor,
        int particleCount,
        float intensity)
    {
        float safeIntensity =
            Mathf.Max(
                0.1f,
                intensity
            );

        float angleOffset =
            Random.Range(
                0f,
                Mathf.PI * 2f
            );

        for (int index = 0;
             index < particleCount;
             index++)
        {
            GemPixelParticle particle =
                GetAvailableParticle();

            if (particle == null)
            {
                return;
            }

            float evenAngle =
                (
                    Mathf.PI *
                    2f *
                    index
                ) /
                particleCount;

            float angleJitter =
                Random.Range(
                    -0.2f,
                    0.2f
                );

            float angle =
                angleOffset +
                evenAngle +
                angleJitter;

            Vector2 direction =
                new Vector2(
                    Mathf.Cos(angle),
                    Mathf.Sin(angle)
                );

            direction.y +=
                upwardBias;

            direction.Normalize();

            float speed =
                Random.Range(
                    speedRange.x,
                    speedRange.y
                ) *
                safeIntensity;

            Vector2 velocity =
                direction *
                speed;

            Vector2 randomOffset =
                Random.insideUnitCircle *
                spawnRadius;

            float lifetime =
                Random.Range(
                    lifetimeRange.x,
                    lifetimeRange.y
                );

            float size =
                Random.Range(
                    particleSizeRange.x,
                    particleSizeRange.y
                ) *
                Mathf.Lerp(
                    1f,
                    1.15f,
                    safeIntensity - 1f
                );
            float aspectRatio =
                Random.Range(
                    shardAspectRatioRange.x,
                    shardAspectRatioRange.y
                );

            float startingRotation =
                angle *
                Mathf.Rad2Deg -
                90f;

            startingRotation +=
                Random.Range(
                    -18f,
                    18f
                );

            float spinSpeed =
                Random.Range(
                    shardSpinRange.x,
                    shardSpinRange.y
                );

            Color particleColor =
                CreateParticleColor(
                    baseColor
                );

            activeParticles.Add(
                particle
            );

            particle.Play(
                localPosition +
                (Vector3)randomOffset,
                velocity,
                lifetime,
                size,
                aspectRatio,
                startingRotation,
                spinSpeed,
                shardGravity,
                shardMovementDrag,
                particleColor,
                ReleaseParticle
            );
        }
    }

    private Color CreateParticleColor(
        Color baseColor)
    {
        float whiteAmount =
            Random.Range(
                0f,
                maximumWhiteVariation
            );

        Color color =
            Color.Lerp(
                baseColor,
                Color.white,
                whiteAmount
            );

        color.a = 1f;

        return color;
    }

    private int GetParticlesPerGem(
        int gemCount)
    {
        if (gemCount >= 5)
        {
            return fivePlusMatchParticles;
        }

        if (gemCount == 4)
        {
            return fourMatchParticles;
        }

        return threeMatchParticles;
    }

    private float GetMatchIntensity(
        int gemCount)
    {
        if (gemCount >= 5)
        {
            return
                1f +
                fiveMatchIntensityBonus;
        }

        if (gemCount == 4)
        {
            return
                1f +
                fourMatchIntensityBonus;
        }

        return 1f;
    }

    private GemPixelParticle
        GetAvailableParticle()
    {
        while (availableParticles.Count > 0)
        {
            GemPixelParticle particle =
                availableParticles.Dequeue();

            if (particle != null)
            {
                return particle;
            }
        }

        if (createdParticleCount >=
            maximumParticleCount)
        {
            return null;
        }

        return CreateParticle();
    }

    private GemPixelParticle CreateParticle()
    {
        GemPixelParticle particle =
            Instantiate(
                particlePrefab,
                particleContainer,
                false
            );

        particle.ConfigureRendering(
            sortingLayerName,
            sortingOrder
        );

        particle.gameObject.SetActive(
            false
        );

        createdParticleCount++;

        return particle;
    }

    private void ReleaseParticle(
        GemPixelParticle particle)
    {
        if (particle == null ||
            !activeParticles.Remove(
                particle))
        {
            return;
        }

        particle.gameObject.SetActive(
            false
        );

        particle.transform.SetParent(
            particleContainer,
            false
        );

        availableParticles.Enqueue(
            particle
        );
    }

    private void PrewarmPool()
    {
        int amountToCreate =
            Mathf.Min(
                prewarmCount,
                maximumParticleCount
            );

        for (int index = 0;
             index < amountToCreate;
             index++)
        {
            GemPixelParticle particle =
                CreateParticle();

            availableParticles.Enqueue(
                particle
            );
        }
    }

    private bool ValidateReferences()
    {
        bool isValid = true;

        if (boardController == null)
        {
            Debug.LogError(
                "GemMatchVFXController requires a BoardController.",
                this
            );

            isValid = false;
        }

        if (particleContainer == null)
        {
            Debug.LogError(
                "GemMatchVFXController requires a particle container.",
                this
            );

            isValid = false;
        }

        if (particlePrefab == null)
        {
            Debug.LogError(
                "GemMatchVFXController requires a particle prefab.",
                this
            );

            isValid = false;
        }

        if (gemColorPalette == null)
        {
            Debug.LogError(
                "GemMatchVFXController requires a GemColorPalette.",
                this
            );

            isValid = false;
        }

        return isValid;
    }

    private void OnValidate()
    {
        threeMatchParticles =
            Mathf.Max(
                1,
                threeMatchParticles
            );

        fourMatchParticles =
            Mathf.Max(
                threeMatchParticles,
                fourMatchParticles
            );

        fivePlusMatchParticles =
            Mathf.Max(
                fourMatchParticles,
                fivePlusMatchParticles
            );

        particleSizeRange.x =
            Mathf.Max(
                0.005f,
                particleSizeRange.x
            );

        particleSizeRange.y =
            Mathf.Max(
                particleSizeRange.x,
                particleSizeRange.y
            );

        speedRange.x =
            Mathf.Max(
                0.01f,
                speedRange.x
            );

        speedRange.y =
            Mathf.Max(
                speedRange.x,
                speedRange.y
            );

        lifetimeRange.x =
            Mathf.Max(
                0.05f,
                lifetimeRange.x
            );

        lifetimeRange.y =
            Mathf.Max(
                lifetimeRange.x,
                lifetimeRange.y
            );

        maximumParticleCount =
            Mathf.Max(
                1,
                maximumParticleCount
            );

        prewarmCount =
            Mathf.Clamp(
                prewarmCount,
                0,
                maximumParticleCount
            );

        shardAspectRatioRange.x =
            Mathf.Max(
                1f,
                shardAspectRatioRange.x
            );

        shardAspectRatioRange.y =
            Mathf.Max(
                shardAspectRatioRange.x,
                shardAspectRatioRange.y
            );

        if (shardSpinRange.y <
            shardSpinRange.x)
        {
            shardSpinRange.y =
                shardSpinRange.x;
        }

        shardGravity =
            Mathf.Max(
                0f,
                shardGravity
            );
    }
}
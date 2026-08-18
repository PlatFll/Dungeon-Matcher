using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyWeaknessIndicatorConfig",
    menuName = "Dungeon Matcher/UI/Enemy Weakness Indicator Config"
)]
public sealed class EnemyWeaknessIndicatorConfig :
    ScriptableObject
{
    [Header("Weakness Sprites")]
    [SerializeField]
    private Sprite rubySprite;

    [SerializeField]
    private Sprite amberSprite;

    [SerializeField]
    private Sprite topazSprite;

    [SerializeField]
    private Sprite emeraldSprite;

    [SerializeField]
    private Sprite sapphireSprite;

    [SerializeField]
    private Sprite amethystSprite;

    [Header("Materialization")]
    [SerializeField]
    private Shader whiteFlashShader;

    [SerializeField, Min(1f)]
    private float iconSize = 16f;

    [SerializeField, Min(0f)]
    private float gapBelowHealthBar = 2f;

    [SerializeField, Min(0.01f)]
    private float materializeDuration = 0.24f;

    [SerializeField, Range(0.05f, 1f)]
    private float materializeStartScale = 0.35f;

    [SerializeField, Min(1f)]
    private float materializeOvershootScale = 1.28f;

    [Header("Death Burst")]
    [SerializeField, Min(0f)]
    private float deathWhiteHoldDuration = 0.055f;

    [SerializeField, Min(1f)]
    private float deathPopScale = 1.35f;

    [SerializeField, Range(1, 24)]
    private int deathParticleCount = 9;

    [SerializeField, Min(0.01f)]
    private float particleMinimumLifetime = 0.20f;

    [SerializeField, Min(0.01f)]
    private float particleMaximumLifetime = 0.30f;

    [SerializeField, Min(0f)]
    private float particleMinimumSpeed = 48f;

    [SerializeField, Min(0f)]
    private float particleMaximumSpeed = 76f;

    [SerializeField, Min(1f)]
    private float particleMinimumSize = 2f;

    [SerializeField, Min(1f)]
    private float particleMaximumSize = 3.5f;

    [SerializeField, Range(0.05f, 1f)]
    private float particleEndScale = 0.2f;

    [SerializeField, Range(-1f, 1f)]
    private float particleUpwardBias = 0.16f;

    public Shader WhiteFlashShader =>
        whiteFlashShader;

    public float IconSize =>
        iconSize;

    public float GapBelowHealthBar =>
        gapBelowHealthBar;

    public float MaterializeDuration =>
        materializeDuration;

    public float MaterializeStartScale =>
        materializeStartScale;

    public float MaterializeOvershootScale =>
        materializeOvershootScale;

    public float DeathWhiteHoldDuration =>
        deathWhiteHoldDuration;

    public float DeathPopScale =>
        deathPopScale;

    public int DeathParticleCount =>
        deathParticleCount;

    public float ParticleMinimumLifetime =>
        Mathf.Min(
            particleMinimumLifetime,
            particleMaximumLifetime
        );

    public float ParticleMaximumLifetime =>
        Mathf.Max(
            particleMinimumLifetime,
            particleMaximumLifetime
        );

    public float ParticleMinimumSpeed =>
        Mathf.Min(
            particleMinimumSpeed,
            particleMaximumSpeed
        );

    public float ParticleMaximumSpeed =>
        Mathf.Max(
            particleMinimumSpeed,
            particleMaximumSpeed
        );

    public float ParticleMinimumSize =>
        Mathf.Min(
            particleMinimumSize,
            particleMaximumSize
        );

    public float ParticleMaximumSize =>
        Mathf.Max(
            particleMinimumSize,
            particleMaximumSize
        );

    public float ParticleEndScale =>
        particleEndScale;

    public float ParticleUpwardBias =>
        particleUpwardBias;

    public Sprite GetSprite(
        GemType gemType)
    {
        return gemType switch
        {
            GemType.Ruby => rubySprite,
            GemType.Amber => amberSprite,
            GemType.Topaz => topazSprite,
            GemType.Emerald => emeraldSprite,
            GemType.Sapphire => sapphireSprite,
            GemType.Amethyst => amethystSprite,
            _ => null
        };
    }

    private void OnValidate()
    {
        particleMaximumLifetime =
            Mathf.Max(
                particleMinimumLifetime,
                particleMaximumLifetime
            );

        particleMaximumSpeed =
            Mathf.Max(
                particleMinimumSpeed,
                particleMaximumSpeed
            );

        particleMaximumSize =
            Mathf.Max(
                particleMinimumSize,
                particleMaximumSize
            );
    }
}

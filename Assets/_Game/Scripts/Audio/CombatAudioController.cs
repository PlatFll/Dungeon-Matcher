using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Observes committed combat/board presentation events. Never owns gameplay timing.</summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(100)]
public sealed class CombatAudioController : MonoBehaviour
{
    private static CombatAudioController instance;
    private BoardController board;
    private WaveController waves;
    private PlayerActor player;
    private PlayerAbilityController ability;
    private readonly HashSet<EnemyActor> enemies = new HashSet<EnemyActor>();
    private readonly Dictionary<CombatSoundCue, Pending> pending = new Dictionary<CombatSoundCue, Pending>();
    private readonly List<CombatSoundCue> ready = new List<CombatSoundCue>();
    private readonly Dictionary<CombatSoundCue, AudioClip> clips = new Dictionary<CombatSoundCue, AudioClip>();
    private readonly Dictionary<CombatSoundCue, FeedbackCooldown> cooldowns = new Dictionary<CombatSoundCue, FeedbackCooldown>();
    private readonly Voice[] voices = new Voice[CombatSoundMix.VoiceCount];
    private readonly HashSet<int> shieldHitActors = new HashSet<int>();
    private bool focused = true;
    private bool appPaused;
    public event Action<CombatSoundCue> CuePlayed;

    private struct Pending { public int Count, Cascade; public float Due; }
    private sealed class Voice { public AudioSource Source; public int Priority; public float Gain; }

    public static bool FeedbackAllowed
    {
        get
        {
            var continuation = RunSession.Current?.Continuation;
            return Application.isPlaying && Time.timeScale > 0 &&
                !(continuation != null && (continuation.IsRestoring || continuation.IsReplaying)) &&
                (instance == null || (instance.focused && !instance.appPaused));
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        instance = null;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        SceneManager.sceneLoaded += SceneLoaded;
    }
    private static void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!Application.isPlaying) return;
        var board = FindFirstObjectByType<BoardController>();
        if (board != null && board.GetComponent<CombatAudioController>() == null)
            board.gameObject.AddComponent<CombatAudioController>();
    }

    // Existing GemBreakAudioController retains match grouping/flash delay and delegates its playback.
    public static bool TryPlayMatch(int count, int cascade)
    {
        if (instance == null || !instance.isActiveAndEnabled || !instance.clips.ContainsKey(CombatSoundCue.GemMatch)) return false;
        instance.Queue(CombatSoundCue.GemMatch, count, cascade);
        return true;
    }

    private void Awake()
    {
        if (instance != null && instance != this) { enabled = false; return; }
        instance = this;
        focused = Application.isFocused;
        foreach (CombatSoundCue cue in Enum.GetValues(typeof(CombatSoundCue)))
        {
            var clip = Resources.Load<AudioClip>("Audio/Combat/" + cue);
            if (clip != null) clips.Add(cue, clip);
            cooldowns.Add(cue, new FeedbackCooldown());
        }
        for (int i = 0; i < voices.Length; i++)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false; source.loop = false; source.spatialBlend = 0;
            source.dopplerLevel = 0; source.priority = 100;
            voices[i] = new Voice { Source = source };
        }
    }
    private void OnEnable()
    {
        if (instance != this) return;
        board = GetComponent<BoardController>();
        waves = FindFirstObjectByType<WaveController>();
        player = FindFirstObjectByType<PlayerActor>();
        ability = player != null ? player.GetComponent<PlayerAbilityController>() : null;
        if (board != null) { board.TileBurstVFXRequested += OnBurst; board.GemsLanded += OnLanding; }
        if (waves != null)
        {
            waves.EnemySpawned += ObserveEnemy;
            foreach (var enemy in waves.ActiveEnemies) ObserveEnemy(enemy);
        }
        if (player != null)
        {
            player.DamageTaken += OnPlayerHit;
            player.ShieldDamaged += OnPlayerShieldHit;
            player.ShieldGranted += OnPlayerShieldGain;
            player.Healed += OnPlayerHeal;
        }
        if (ability != null) ability.AbilityActivated += OnPlayerAbility;
        AudioPreferences.Changed += PreferencesChanged;
        PreferencesChanged();
    }
    private void OnDisable()
    {
        if (board != null) { board.TileBurstVFXRequested -= OnBurst; board.GemsLanded -= OnLanding; }
        if (waves != null) waves.EnemySpawned -= ObserveEnemy;
        if (player != null)
        {
            player.DamageTaken -= OnPlayerHit;
            player.ShieldDamaged -= OnPlayerShieldHit;
            player.ShieldGranted -= OnPlayerShieldGain;
            player.Healed -= OnPlayerHeal;
        }
        if (ability != null) ability.AbilityActivated -= OnPlayerAbility;
        foreach (var enemy in enemies) UnobserveEnemy(enemy);
        enemies.Clear();
        AudioPreferences.Changed -= PreferencesChanged;
        Silence();
    }
    private void OnDestroy() { if (instance == this) instance = null; }
    private void ObserveEnemy(EnemyActor enemy)
    {
        if (enemy == null || !enemies.Add(enemy)) return;
        enemy.DamageReceived += OnEnemyHit;
        enemy.ShieldDamaged += OnEnemyShieldHit;
        enemy.ShieldGranted += OnEnemyShieldGain;
        enemy.Healed += OnEnemyHeal;
        enemy.StatusDamageReceived += OnTick;
        enemy.SpecialAbilityUsed += OnEnemyAbility;
        enemy.SpecialAbilityEffectApplied += OnEnemyAbility;
    }
    private void UnobserveEnemy(EnemyActor enemy)
    {
        if (enemy == null) return;
        enemy.DamageReceived -= OnEnemyHit;
        enemy.ShieldDamaged -= OnEnemyShieldHit;
        enemy.ShieldGranted -= OnEnemyShieldGain;
        enemy.Healed -= OnEnemyHeal;
        enemy.StatusDamageReceived -= OnTick;
        enemy.SpecialAbilityUsed -= OnEnemyAbility;
        enemy.SpecialAbilityEffectApplied -= OnEnemyAbility;
    }
    private void Queue(CombatSoundCue cue, int count = 1, int cascade = 0)
    {
        if (count <= 0 || !FeedbackAllowed) return;
        pending.TryGetValue(cue, out var value);
        value.Count = Mathf.Min(64, value.Count + count);
        value.Cascade = Mathf.Max(value.Cascade, cascade);
        value.Due = Time.time;
        pending[cue] = value;
    }
    private void OnBurst(TileBurstVFXContext context)
    {
        CombatSoundCue cue;
        switch (context.Kind)
        {
            case TileBurstKind.Poison: cue = CombatSoundCue.PoisonBurst; break;
            case TileBurstKind.Shield: cue = CombatSoundCue.ShieldGain; break;
            case TileBurstKind.Healing: cue = CombatSoundCue.Healing; break;
            default: cue = CombatSoundCue.Explosion; break;
        }
        Queue(cue, context.TileCount);
    }
    private void OnLanding(int count) => Queue(CombatSoundCue.GemLand, count);
    private void OnPlayerHit(PlayerActor actor, int amount)
    {
        if (!shieldHitActors.Contains(actor.GetInstanceID())) Queue(CombatSoundCue.PlayerHit);
    }
    private void OnPlayerShieldHit(PlayerActor actor, int amount)
    {
        shieldHitActors.Add(actor.GetInstanceID()); Queue(CombatSoundCue.ShieldHit);
        MobileHaptics.Request(HapticStrength.Impact);
    }
    private void OnEnemyHit(EnemyActor actor, int amount)
    {
        if (!shieldHitActors.Contains(actor.GetInstanceID())) Queue(CombatSoundCue.EnemyHit);
    }
    private void OnEnemyShieldHit(EnemyActor actor, int amount)
    { shieldHitActors.Add(actor.GetInstanceID()); Queue(CombatSoundCue.ShieldHit); }
    private void OnPlayerShieldGain(PlayerActor actor, int amount) => Queue(CombatSoundCue.ShieldGain);
    private void OnEnemyShieldGain(EnemyActor actor, int amount) => Queue(CombatSoundCue.ShieldGain);
    private void OnPlayerHeal(PlayerActor actor, int amount) => Queue(CombatSoundCue.Healing);
    private void OnEnemyHeal(EnemyActor actor, int amount) => Queue(CombatSoundCue.Healing);
    private void OnTick(EnemyActor actor, int amount) => Queue(CombatSoundCue.PoisonTick);
    private void OnEnemyAbility(EnemyActor actor) => Queue(CombatSoundCue.EnemyAbility);
    private void OnPlayerAbility()
    {
        Queue(ability.ActiveAbility is CrackedGemsAbilityDefinition ? CombatSoundCue.BardleyAbility :
            ability.ActiveAbility is RoyalDecreeAbilityDefinition ? CombatSoundCue.RattlebonesAbility :
            CombatSoundCue.EnemyAbility);
    }

    private void LateUpdate()
    {
        if (!FeedbackAllowed) { Silence(); return; }
        enemies.RemoveWhere(enemy => enemy == null);
        ready.Clear();
        foreach (var item in pending) if (item.Value.Due <= Time.time) ready.Add(item.Key);
        ready.Sort((a, b) => CombatSoundMix.Priority(b).CompareTo(CombatSoundMix.Priority(a)));
        foreach (var cue in ready)
        {
            var value = pending[cue]; pending.Remove(cue);
            if (!cooldowns[cue].TryConsume(Time.time, CombatSoundMix.MinimumInterval(cue))) continue;
            RequestHaptic(cue);
            if (!AudioPreferences.SfxMuted) Play(cue, value);
        }
        shieldHitActors.Clear();
        BalanceVoices();
        MobileHaptics.Flush();
    }
    private void Play(CombatSoundCue cue, Pending value)
    {
        if (!clips.TryGetValue(cue, out var clip) || clip == null) return;
        Voice voice = null;
        int priority = CombatSoundMix.Priority(cue);
        foreach (var candidate in voices)
        {
            if (!candidate.Source.isPlaying) { voice = candidate; break; }
            if (candidate.Priority < priority && (voice == null || candidate.Priority < voice.Priority)) voice = candidate;
        }
        if (voice == null) return;
        voice.Source.Stop();
        voice.Priority = priority;
        voice.Gain = CombatSoundMix.Gain(cue);
        voice.Source.clip = clip;
        voice.Source.pitch = cue == CombatSoundCue.GemMatch
            ? Mathf.Pow(2f, Mathf.Min(4, value.Cascade) / 12f) : 1f;
        voice.Source.volume = 0;
        voice.Source.mute = false;
        voice.Source.Play();
        CuePlayed?.Invoke(cue);
    }
    private void BalanceVoices()
    {
        float total = 0;
        foreach (var voice in voices) if (voice != null && voice.Source.isPlaying) total += voice.Gain;
        float scale = CombatSoundMix.HeadroomScale(total);
        foreach (var voice in voices)
            if (voice != null) voice.Source.volume = voice.Gain * scale;
    }
    private static void RequestHaptic(CombatSoundCue cue)
    {
        switch (cue)
        {
            case CombatSoundCue.GemLand: MobileHaptics.Request(HapticStrength.Tick); break;
            case CombatSoundCue.PlayerHit:
            case CombatSoundCue.Explosion: MobileHaptics.Request(HapticStrength.Impact); break;
            case CombatSoundCue.BardleyAbility:
            case CombatSoundCue.RattlebonesAbility: MobileHaptics.Request(HapticStrength.Cast); break;
        }
    }
    private void PreferencesChanged()
    {
        if (AudioPreferences.SfxMuted)
            foreach (var voice in voices) if (voice != null) voice.Source.Stop();
        if (AudioPreferences.VibrationMuted) MobileHaptics.Cancel();
    }
    private void Silence()
    {
        pending.Clear(); shieldHitActors.Clear();
        foreach (var voice in voices) if (voice != null && voice.Source != null) voice.Source.Stop();
        MobileHaptics.Cancel();
    }
    private void OnApplicationFocus(bool value) { focused = value; if (!value) Silence(); }
    private void OnApplicationPause(bool value) { appPaused = value; if (value) Silence(); }
}

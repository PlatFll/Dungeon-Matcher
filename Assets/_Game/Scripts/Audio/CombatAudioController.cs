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
    private readonly Dictionary<CombatSoundCue, AudioClip[]> variants = new Dictionary<CombatSoundCue, AudioClip[]>();
    private readonly Dictionary<CombatSoundCue, SoundVariantSelector> selectors = new Dictionary<CombatSoundCue, SoundVariantSelector>();
    private long lastBoardFeedback;
    private readonly Dictionary<CombatSoundCue, FeedbackCooldown> cooldowns = new Dictionary<CombatSoundCue, FeedbackCooldown>();
    private readonly Voice[] voices = new Voice[CombatSoundMix.VoiceCount];
    private readonly HashSet<int> shieldHitActors = new HashSet<int>();
    private bool focused = true;
    private bool appPaused;
    private int airWarningMove = -1;
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

    // The legacy scene component delegates the board's committed shatter receipt.
    public static bool TryPlayMatch(int count, int cascade)
    {
        if (instance == null || !instance.isActiveAndEnabled) return false;
        instance.Queue(CombatSoundCue.GemMatch, count, cascade);
        return true;
    }

    // Mechanism presentation shares the existing voice budget, pause/restore
    // suppression, cooldowns and independent SFX preference.
    public static void PlayMechanism(CombatSoundCue cue)
    {
        if(instance!=null && instance.isActiveAndEnabled) instance.Queue(cue);
    }

    private void Awake()
    {
        if (instance != null && instance != this) { enabled = false; return; }
        instance = this;
        focused = Application.isFocused;
        foreach (CombatSoundCue cue in Enum.GetValues(typeof(CombatSoundCue)))
        {
            var clip = Resources.Load<AudioClip>("Audio/Combat/" + cue);
            var choices = new List<AudioClip>();
            if (clip != null) choices.Add(clip);
            for (int v = 1; v <= 3; v++)
            {
                var alternative = Resources.Load<AudioClip>("Audio/Combat/" + cue + "_" + v);
                if (alternative != null) choices.Add(alternative);
            }
            if (choices.Count > 0)
            { variants.Add(cue, choices.ToArray()); selectors.Add(cue, new SoundVariantSelector()); }
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
        if (board != null) { board.TileBurstVFXRequested += OnBurst; board.GemsLanded += OnLanding; board.FeedbackPresented += OnBoardFeedback; }
        if(board!=null){board.AquaticAnswer+=OnAirReturn;board.AquaticChanged+=OnTideChanged;}
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
        if (board != null) { board.TileBurstVFXRequested -= OnBurst; board.GemsLanded -= OnLanding; board.FeedbackPresented -= OnBoardFeedback; }
        if(board!=null){board.AquaticAnswer-=OnAirReturn;board.AquaticChanged-=OnTideChanged;}
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
    private void OnAirReturn(int gemId,bool bubble,bool coffer)
    {
        if(coffer)Queue(CombatSoundCue.CofferBreak);
        if(bubble || coffer)Queue(CombatSoundCue.AirPickup);
    }
    private void OnTideChanged()
    {
        var tide=board.Aquatic;
        if(board.IsFlooded && tide.air<=1 && airWarningMove!=tide.lastSettledMove)
        {airWarningMove=tide.lastSettledMove;Queue(CombatSoundCue.AirWarning);}
    }
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
        if (context.Kind != TileBurstKind.Explosion)
            MobileHaptics.Request(HapticStrength.Impact, 2);
    }
    private void OnLanding(int count) => Queue(CombatSoundCue.GemLand, count);
    private void OnBoardFeedback(BoardFeedback feedback)
    {
        if (feedback.Id <= lastBoardFeedback) return;
        lastBoardFeedback = feedback.Id;
        switch (feedback.Kind)
        {
            // GemBreakAudioController observes this same receipt for its optional
            // old-clip fallback. Only one of the two paths plays the match.
            case BoardFeedbackKind.MatchImpact:
                if (board.GetComponent<GemBreakAudioController>()?.isActiveAndEnabled != true)
                    Queue(CombatSoundCue.GemMatch, feedback.Count, feedback.Cascade);
                break;
            case BoardFeedbackKind.Swap: Queue(CombatSoundCue.Swap); break;
            case BoardFeedbackKind.InvalidSwap: Queue(CombatSoundCue.InvalidSwap); break;
            case BoardFeedbackKind.SpecialCreated: Queue(CombatSoundCue.SpecialCreated); break;
            case BoardFeedbackKind.BlockerHit: Queue(CombatSoundCue.BlockerHit); break;
            case BoardFeedbackKind.BlockerBreak: Queue(CombatSoundCue.BlockerBreak); break;
            case BoardFeedbackKind.CrystalAnticipation: Queue(CombatSoundCue.CrystalCharge); break;
            case BoardFeedbackKind.CrystalImpact: Queue(CombatSoundCue.CrystalImpact); break;
        }
    }
    private void OnPlayerHit(PlayerActor actor, int amount)
    {
        if (!shieldHitActors.Contains(actor.GetInstanceID())) Queue(CombatSoundCue.PlayerHit);
    }
    private void OnPlayerShieldHit(PlayerActor actor, int amount)
    {
        shieldHitActors.Add(actor.GetInstanceID()); Queue(CombatSoundCue.ShieldHit);
        MobileHaptics.Request(HapticStrength.Impact, 4);
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
        MobileHaptics.Request(HapticStrength.Cast, 3);
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
            RequestHaptic(cue);
            if (!cooldowns[cue].TryConsume(Time.time, CombatSoundMix.MinimumInterval(cue))) continue;
            if (!AudioPreferences.SfxMuted) Play(cue, value);
        }
        shieldHitActors.Clear();
        BalanceVoices();
        MobileHaptics.Flush();
    }
    private void Play(CombatSoundCue cue, Pending value)
    {
        if (!variants.TryGetValue(cue, out var choices) || choices.Length == 0) return;
        Voice voice = null;
        int priority = CombatSoundMix.Priority(cue);
        foreach (var candidate in voices)
        {
            if (!candidate.Source.isPlaying) { voice = candidate; break; }
            if (candidate.Priority < priority && (voice == null || candidate.Priority < voice.Priority)) voice = candidate;
        }
        if (voice == null) return;
        var clip = choices[selectors[cue].Next(choices.Length)];
        voice.Source.Stop();
        voice.Priority = priority;
        voice.Gain = CombatSoundMix.Gain(cue) * CombatSoundMix.CountWeight(cue, value.Count);
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
            case CombatSoundCue.InvalidSwap: MobileHaptics.Request(HapticStrength.Tick, 0); break;
            case CombatSoundCue.GemMatch: MobileHaptics.Request(HapticStrength.Tick, 1); break;
            case CombatSoundCue.SpecialCreated:
            case CombatSoundCue.BlockerBreak:
            case CombatSoundCue.CofferBreak:
            case CombatSoundCue.MineStoneBreak:
            case CombatSoundCue.Explosion:
            case CombatSoundCue.AttenderCrash: MobileHaptics.Request(HapticStrength.Impact, 2); break;
            case CombatSoundCue.CrystalCharge: MobileHaptics.Request(HapticStrength.Tick, 1); break;
            case CombatSoundCue.CrystalImpact: MobileHaptics.Request(HapticStrength.Cast, 3); break;
            case CombatSoundCue.PlayerHit: MobileHaptics.Request(HapticStrength.Impact, 4); break;
            case CombatSoundCue.BardleyAbility:
            case CombatSoundCue.RattlebonesAbility: MobileHaptics.Request(HapticStrength.Cast, 3); break;
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

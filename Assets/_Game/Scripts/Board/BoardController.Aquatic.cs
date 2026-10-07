using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class BoardController
{
    private AquaticEnvironmentState aquatic;
    private readonly Dictionary<int, bool> aquaticClearReceipts = new Dictionary<int, bool>();
    private int suffocationMove = -1;
    private int pendingRoyalCofferSlide;
    public AquaticEnvironmentState Aquatic => aquatic;
    public bool IsFlooded => aquatic?.phase == TidePhase.Flooded;
    public event Action AquaticChanged;
    // Presentation receives the same receipt as accounting: spend first, then
    // each actual collection. It cannot alter the authoritative AIR value.
    public event Action<int,int,int[]> AirReceipt;
    public event Action<Vector3> CofferShellBroken;
    public event Action<Vector3,Vector3,float> CofferMoving;
    public Vector3? AirCofferVisualPosition
    {
        get
        {
            var c=aquatic?.coffer;
            return c!=null && barricadeCells.TryGetValue(new Vector2Int(c.x,c.y),out var barrier) && barrier.ViewObject!=null
                ? barrier.ViewObject.transform.position : (Vector3?)null;
        }
    }
    // Physical identity is shared by fixed-cell and bubble answers, so a single
    // destroyed gem cannot answer a pressure channel twice.
    public event Action<int, bool, bool> AquaticAnswer;
    private bool AquaticZone => RunSession.Current?.Zone?.Definition?.periodicallyFloods == true;

    public void AcceptAquaticMove(int move)
    {
        if (!AquaticZone) return;
        aquatic ??= new AquaticEnvironmentState();
        aquatic.Accept(move);
    }

    private void RegisterAquaticClear(IEnumerable<Gem> cleared, bool manual)
    {
        if (!AquaticZone || cleared == null) return;
        foreach (var gem in cleared) if (gem != null)
            aquaticClearReceipts[gem.BoardIdentity] = manual ||
                (aquaticClearReceipts.TryGetValue(gem.BoardIdentity, out bool wasManual) && wasManual);
    }

    private void ResolveAquaticDestruction(IEnumerable<Gem> cleared, HashSet<Gem> preserved)
    {
        if (aquatic == null || cleared == null) return;
        foreach (var gem in cleared)
        {
            if (gem == null) continue;
            int id = gem.BoardIdentity;
            bool player = aquaticClearReceipts.TryGetValue(id, out bool manual);
            aquaticClearReceipts.Remove(id);
            if (preserved.Contains(gem)) continue;
            bool bubble = aquatic.bubbles.Remove(id);
            bool snared = aquatic.snares.RemoveAll(s => s.gemId == id) > 0;
            if (!player) continue; // Environmental removal never creates collection rewards.
            if (bubble) CollectAir(2);
            if (snared && manual) aquatic.SnaredManualClear();
            AquaticAnswer?.Invoke(id, bubble, false);
        }
        AquaticChanged?.Invoke();
    }

    public bool FormationCanFlood(IEnumerable<EnemyDefinition> definitions) =>
        definitions != null && definitions.Any() && definitions.All(d => d != null && d.canFightFlooded);

    public static bool FirstFloodLessonSafe(IEnumerable<EnemyDefinition> definitions) =>
        definitions != null && definitions.All(d => d != null &&
            d.SpecialAbilityKind != EnemySpecialAbilityKind.PearlTheft &&
            d.SpecialAbilityKind != EnemySpecialAbilityKind.ThornySnare &&
            d.SpecialAbilityKind != EnemySpecialAbilityKind.SpineGuard &&
            d.SpecialAbilityKind != EnemySpecialAbilityKind.LanternPressure &&
            d.SpecialAbilityKind != EnemySpecialAbilityKind.AbyssalRegent);

    private bool IsAquaticWarningTarget(Gem gem)
    {
        if (!AquaticZone || gem == null) return false;
        var cell = new Vector2Int(gem.Column, gem.Row);
        foreach (var enemy in RunSession.Current.Waves.ActiveEnemies)
        {
            var ability = enemy != null && !enemy.IsDefeated ? enemy.GetComponent<AquaticEnemyAbility>() : null;
            if (ability?.IsPreparing == true && (ability.ResponseCells.Contains(cell) ||
                ability.MarkedBubbles.Contains(gem.BoardIdentity) || ability.CofferTarget == cell)) return true;
        }
        return false;
    }

    private IEnumerator AdvanceAquaticEnvironment(int move)
    {
        var run = RunSession.Current;
        if (run == null || run.IsFinished || run.Player.IsDefeated) yield break;
        aquatic ??= new AquaticEnvironmentState();
        if(move<=aquatic.lastSettledMove)yield break; // Do not replay presentation/supply on an already settled receipt.
        int encounter = run.Travel?.LocalWave ?? run.Waves.CurrentWave;
        aquatic.EncounterStarted(encounter);
        // Old checkpoints lacking this optional owner begin dry at their current action.
        if (aquatic.acceptedMove != move) aquatic.Accept(move);
        var coffer = aquatic.coffer;
        bool drained = aquatic.Settle(move, encounter);
        if(aquatic.acceptedWet && !drained)
            AirReceipt?.Invoke(aquatic.acceptedAir,1+aquatic.snareLoss,aquatic.collectedAmounts.ToArray());
        aquatic.snares.RemoveAll(s => s.expiresMove <= move || FindAquaticGem(s.gemId) == null || !AquaticOwnerLiving(s.ownerId));
        if (drained)
        {
            if (coffer != null) { aquatic.coffer = coffer; yield return RemoveAirCoffer(false); }
            AquaticChanged?.Invoke();
            yield return AquaticTransition(false);
            yield break;
        }
        var living = run.Waves.ActiveEnemies.Where(e => e != null && !e.IsDefeated).ToArray();
        if (run.Waves.IsWaveActive && living.Length > 0 && aquatic.CanFlood(encounter,
            FormationCanFlood(run.Waves.OriginalEncounterDefinitions) && living.All(e => e.Definition.canFightFlooded) &&
            (aquatic.floodCount > 0 || FirstFloodLessonSafe(run.Waves.OriginalEncounterDefinitions))))
        {
            var tuning=run.Zone.Definition;
            aquatic.StartFlood(GameplayRandom.Range(Mathf.Max(1,tuning.floodMinimumMoves),
                Mathf.Max(tuning.floodMinimumMoves,tuning.floodMaximumMoves)+1), move);
            EnsureAquaticSupply(tuning.initialAirBubbles);
            AquaticChanged?.Invoke();
            yield return AquaticTransition(true);
        }
        else if (IsFlooded)
        {
            aquatic.bubbles.RemoveAll(id => FindAquaticGem(id) == null);
            AdvanceEmergencyAir(move);
            if (aquatic.air <= 2) EnsureReachableAir();
        }
        AquaticChanged?.Invoke();
    }

    private IEnumerator AquaticTransition(bool flooded)
    {
        // This runs inside the accepted-action hold. Timed basics and input are
        // already paused by CombatMoveClock; presentation cannot own the state.
        yield return new WaitForSeconds(.35f);
    }

    public void FinishAquaticMove(int move)
    {
        var run = RunSession.Current;
        if (!AquaticZone || aquatic == null || suffocationMove == move || !aquatic.NeedsSuffocation(move) ||
            run.IsFinished || run.Player.IsDefeated || !run.Waves.IsWaveActive ||
            !run.Waves.ActiveEnemies.Any(e => e != null && !e.IsDefeated)) return;
        suffocationMove = move;
        var source = GetComponent<SuffocationDamageSource>() ?? gameObject.AddComponent<SuffocationDamageSource>();
        run.Player.TryTakeDamage(5, source);
        AquaticChanged?.Invoke();
    }

    public Gem FindAquaticGem(int identity)
    {
        if (gems != null) foreach (var gem in gems) if (gem != null && gem.BoardIdentity == identity) return gem;
        return null;
    }

    private bool IsAquaticSnared(Gem gem) => gem != null && aquatic != null &&
        aquatic.snares.Exists(s => s.gemId == gem.BoardIdentity);

    private bool AquaticOwnerLiving(long id) => RunSession.Current?.Waves?.ActiveEnemies.Any(e =>
        e != null && !e.IsDefeated && e.PersistentId == id) == true;

    private List<Gem> AirCandidates() => ImmediatelyClearableOrdinaryGems()
        .Where(g => !IsProtectedWarningTarget(g) && !aquatic.snares.Exists(s => s.gemId == g.BoardIdentity))
        .OrderBy(g => g.Row).ThenBy(g => g.Column).ToList();

    private void EnsureAquaticSupply(int target)
    {
        if (!IsFlooded) return;
        var choices = AirCandidates();
        choices.RemoveAll(g => aquatic.bubbles.Contains(g.BoardIdentity));
        while (aquatic.bubbles.Count < Mathf.Clamp(target,1,8) && choices.Count > 0)
        {
            int index = BoardRandomRange(0, choices.Count);
            aquatic.bubbles.Add(choices[index].BoardIdentity); choices.RemoveAt(index);
        }
    }

    private void EnsureReachableAir()
    {
        if (!IsFlooded || aquatic.bubbles.Count==0) return;
        var available = AirCandidates();
        if (available.Any(g => aquatic.bubbles.Contains(g.BoardIdentity))) return;
        // A filled supply is relocated, never collected. The visual explicitly
        // follows the new physical gem identity; no AIR or damage is awarded.
        if (available.Count > 0)
        {
            int replace=aquatic.bubbles.FindLastIndex(id=>!IsProtectedWarningTarget(FindAquaticGem(id)));
            if(replace<0)return; // Never move a caster's physical response for presentation convenience.
            aquatic.bubbles.RemoveAt(replace);
            aquatic.bubbles.Add(available[0].BoardIdentity);
        }
    }

    public int RemainingOxygenReserve => aquatic==null?0:aquatic.bubbles.Count+Mathf.Max(0,aquatic.coffer?.charges??0);
    private void AdvanceEmergencyAir(int move)
    {
        var tuning=RunSession.Current.Zone.Definition;
        int cadence=Mathf.Max(1,tuning.emergencyAirCadenceMoves);
        if(!aquatic.reserveExhausted)
        {
            if(RemainingOxygenReserve>0) return;
            aquatic.reserveExhausted=true;aquatic.nextSupplyMove=move+cadence;
            return;
        }
        if(move<aquatic.nextSupplyMove) return;
        aquatic.nextSupplyMove=move+cadence;
        if(aquatic.coffer?.charges>0) return;
        int target=aquatic.air<=tuning.criticalAirThreshold?tuning.criticalAirSupply:tuning.emergencyAirSupply;
        EnsureAquaticSupply(Mathf.Clamp(target,1,2));
    }
    private void CollectAir(int amount)
    {
        int before=aquatic.air;
        bool pendingReceipt=aquatic.acceptedWet && aquatic.acceptedMove>aquatic.lastSettledMove;
        aquatic.Collect(amount);
        if(!pendingReceipt) AirReceipt?.Invoke(before,0,new[]{amount});
    }

    public bool TryApplyAquaticSnares(EnemyActor owner, int count = 2)
    {
        if (owner == null || owner.IsDefeated || !AquaticZone || IsBusy) return false;
        aquatic ??= new AquaticEnvironmentState();
        int capacity = Mathf.Min(count, 2 - aquatic.snares.Count(s => s.ownerId == owner.PersistentId),
            BalanceV1.Current.maximumGlobalChains - RestrictionCount);
        var reserved = new HashSet<int>(AirCandidates().Select(g => g.BoardIdentity));
        var choices = new List<Gem>();
        foreach (var g in gems) if (IsOrdinaryGemOnBoard(g) && !IsGemPinned(g) && !IsProtectedWarningTarget(g) &&
            !aquatic.bubbles.Contains(g.BoardIdentity) && !reserved.Contains(g.BoardIdentity) &&
            !aquatic.snares.Exists(s => s.gemId == g.BoardIdentity)) choices.Add(g);
        int placed = 0;
        while (placed < capacity && choices.Count > 0)
        {
            int index = BoardRandomRange(0, choices.Count); var gem = choices[index]; choices.RemoveAt(index);
            var snare = new AquaticSnareState { gemId = gem.BoardIdentity,
                ownerId = owner.PersistentId, expiresMove = completedValidPlayerMoves + 3 };
            aquatic.snares.Add(snare);
            bool safe = RetainsUsefulResponse() && (!IsFlooded || AirCandidates().Any(g =>
                aquatic.bubbles.Contains(g.BoardIdentity)) || aquatic.bubbles.Count == 0);
            if (!safe) { aquatic.snares.Remove(snare); continue; }
            CancelPointerInteraction(gem); placed++;
        }
        if (placed > 0) AquaticChanged?.Invoke();
        return placed > 0;
    }

    public List<Vector2Int> SelectAquaticResponseCells(int count)
    {
        aquatic ??= new AquaticEnvironmentState();
        return AirCandidates().Take(count).Select(g => new Vector2Int(g.Column, g.Row)).ToList();
    }

    public bool TryPlanAirTheft(int maximum, bool royal, out List<int> targets, out Vector2Int site)
    {
        targets = new List<int>(); site = default;
        if (!IsFlooded || aquatic.coffer != null || IsBusy) return false;
        if(minedCellOwners.Count+barricadeCells.Count>=BalanceV1.Current.maximumGlobalStructures) return false;
        var bubbles=AvailableAirTargets(royal);
        int required=royal?bubbles.Count:maximum;
        if(required<1 || bubbles.Count<required) return false;
        targets=bubbles.Take(required).ToList();
        var choices=BuildBarricadableCellList(true).Where(c=>
            !aquatic.bubbles.Contains(GetGem(c.x,c.y).BoardIdentity)).ToList();
        while(choices.Count>0)
        {
            int index=BoardRandomRange(0,choices.Count);var cell=choices[index];choices.RemoveAt(index);
            barricadeCells[cell] = new BarricadeCellState { RemainingDurability = 1 };
            bool safe=HasReachableCofferHit(cell) && RetainsUsefulResponse();
            barricadeCells.Remove(cell);
            if (!safe) continue;
            site = cell; return true;
        }
        targets.Clear(); return false;
    }
    public List<int> AvailableAirTargets(bool includeProtected=false) => !IsFlooded?new List<int>():
        aquatic.bubbles.Where(id=>FindAquaticGem(id)!=null &&
            (includeProtected || !IsProtectedWarningTarget(FindAquaticGem(id))))
            .OrderBy(id=>FindAquaticGem(id).Row).ThenBy(id=>FindAquaticGem(id).Column).ToList();
    private bool HasReachableCofferHit(Vector2Int cell) => ImmediatelyClearableOrdinaryGems().Any(g=>
        Mathf.Abs(g.Column-cell.x)+Mathf.Abs(g.Row-cell.y)==1);

    public bool TryQueueAirTheft(EnemyActor owner, List<int> targets, Vector2Int site, int durability,
        bool royal, Action<bool> completed)
    {
        if (owner == null || owner.IsDefeated || !IsFlooded || aquatic.coffer != null) return false;
        var request = new BoardMutationRequest { Kind = BoardMutationKind.PlaceAirCoffer,
            OwnerActor = owner, OwnerInstanceId = owner.GetInstanceID(), AquaticTargets = new List<int>(targets),
            AquaticSite = site, BarricadeDurability = durability, AquaticRoyal = royal, Completed = completed,
            IsCancelled=()=>owner==null || owner.IsDefeated || owner.GetComponent<EnemyStagger>()?.IsStaggered==true };
        EnqueueBoardMutation(request);
        request.SpecialMotionId = 0; // Caller already owns and presented the release beat.
        TryStartBoardMutationProcessor(); return true;
    }

    public bool TryQueueConsumeAirBubbles(EnemyActor owner,IEnumerable<int> identities,Action<int> completed)
    {
        if(owner==null || owner.IsDefeated || !IsFlooded)return false;
        var request=new BoardMutationRequest{Kind=BoardMutationKind.ConsumeAirBubbles,
            OwnerActor=owner,OwnerInstanceId=owner.GetInstanceID(),AquaticTargets=identities.Distinct().ToList(),
            IsCancelled=()=>owner==null||owner.IsDefeated||owner.GetComponent<EnemyStagger>()?.IsStaggered==true};
        request.Completed=_=>completed?.Invoke(request.ConsumedBubbles);
        EnqueueBoardMutation(request);request.SpecialMotionId=0;TryStartBoardMutationProcessor();return true;
    }
    private void ExecuteConsumeAirBubbles(BoardMutationRequest request)
    {
        if(!IsFlooded)return;
        foreach(int id in request.AquaticTargets)
            if(FindAquaticGem(id)!=null && aquatic.bubbles.Remove(id))request.ConsumedBubbles++;
        request.Succeeded=request.ConsumedBubbles>0;
        // Strip only oxygen overlays. Neither gems, AIR receipts nor clear rewards
        // are produced by converting reserve oxygen into an enemy buff.
        AquaticChanged?.Invoke();
    }

    private IEnumerator ExecuteAirCoffer(BoardMutationRequest request)
    {
        if (!IsFlooded || aquatic.coffer != null || request.OwnerActor == null || request.OwnerActor.IsDefeated) yield break;
        var site = request.AquaticSite; var covered = GetGem(site.x, site.y);
        if (!IsOrdinaryGemOnBoard(covered) || IsGemPinned(covered) || IsProtectedWarningTarget(covered) ||
            aquatic.bubbles.Contains(covered.BoardIdentity) ||
            minedCellOwners.Count+barricadeCells.Count>=BalanceV1.Current.maximumGlobalStructures) yield break;
        var charges = request.AquaticTargets.Where(id => aquatic.bubbles.Contains(id) && FindAquaticGem(id) != null).Distinct().ToList();
        if (charges.Count == 0 || charges.Count!=request.AquaticTargets.Count) yield break;
        var state = new BarricadeCellState { OwnerInstanceId = 0,
            RemainingDurability = request.BarricadeDurability, MaximumDurability = request.BarricadeDurability,
            Style = EnemyBarricadeStyle.AirCoffer };
        barricadeCells[site] = state;
        // The coffer itself is the oxygen answer. Do not mint a free rescue
        // bubble after theft; require a real adjacent clear instead.
        if (!RetainsUsefulResponse() || !HasReachableCofferHit(site))
        { barricadeCells.Remove(site); yield break; }
        foreach (int id in charges) aquatic.bubbles.Remove(id);
        aquatic.coffer = new AquaticCofferState { id = aquatic.nextCofferId++, x = site.x, y = site.y,
            ownerId = request.OwnerActor.PersistentId, charges = charges.Count,
            refillAir=request.BarricadeDurability>1&&!request.AquaticRoyal,royal=request.AquaticRoyal };
        request.Succeeded = true;
        yield return ClearMatches(new HashSet<Gem> { covered }, null);
        MaterializeBarricades(new List<Vector2Int> { site });
        yield return ResolveEnvironmentalBoardChange();
        AquaticChanged?.Invoke();
    }

    private void AquaticCofferBroken(Vector2Int cell)
    {
        if (aquatic?.coffer == null || aquatic.coffer.x != cell.x || aquatic.coffer.y != cell.y) return;
        int restored = aquatic.coffer.refillAir?5:2*aquatic.coffer.charges; aquatic.coffer = null;pendingRoyalCofferSlide=0;
        CollectAir(restored);
        AquaticAnswer?.Invoke(0, false, true);
        AquaticChanged?.Invoke();
    }

    private void AquaticCofferDamaged(Vector2Int cell)
    {
        if(aquatic?.coffer==null || aquatic.coffer.x!=cell.x || aquatic.coffer.y!=cell.y)return;
        if(aquatic.coffer.royal)pendingRoyalCofferSlide=aquatic.coffer.id;
        CofferShellBroken?.Invoke(transform.TransformPoint(GetCellLocalPosition(cell.x,cell.y)));
    }

    private List<List<Vector2Int>> CofferSlideRoutes(Vector2Int origin)
    {
        var routes=new List<List<Vector2Int>>();
        foreach(var direction in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right})
        {
            var route=new List<Vector2Int>{origin};
            for(int distance=1;distance<=3;distance++)
            {
                var cell=origin+direction*distance;var gem=GetGem(cell.x,cell.y);
                if(!IsCellPlayable(cell.x,cell.y) || gem==null || IsGemPinned(gem) || IsProtectedWarningTarget(gem))break;
                route.Add(cell);
                if(distance>=2)routes.Add(new List<Vector2Int>(route));
            }
        }
        return routes;
    }

    private IEnumerator ResolvePendingCofferSlide()
    {
        int requested=pendingRoyalCofferSlide;pendingRoyalCofferSlide=0;
        var c=aquatic?.coffer;
        if(requested==0 || !IsFlooded || c==null || c.id!=requested || !c.royal)yield break;
        var origin=new Vector2Int(c.x,c.y);
        if(!barricadeCells.TryGetValue(origin,out var barrier) || barrier.RemainingDurability!=1)yield break;
        var routes=CofferSlideRoutes(origin);
        if(routes.Count==0)yield break;
        var route=routes[BoardRandomRange(0,routes.Count)];var destination=route[route.Count-1];
        var moves=new List<GemMove>();
        const float duration=.24f;
        // Commit one rotation, preserving every physical identity and special.
        // This runs after clear destruction and before the caller's one settle.
        barricadeCells.Remove(origin);barricadeCells[destination]=barrier;
        for(int i=1;i<route.Count;i++)
        {
            var from=route[i];var to=route[i-1];var gem=gems[from.x,from.y];
            gems[to.x,to.y]=gem;gem.SetGridPosition(to.x,to.y);
            moves.Add(new GemMove{Gem=gem,StartPosition=gem.transform.localPosition,
                TargetPosition=GetCellLocalPosition(to.x,to.y),Duration=duration});
        }
        gems[destination.x,destination.y]=null;c.x=destination.x;c.y=destination.y;
        Vector3 start=GetCellLocalPosition(origin.x,origin.y),end=GetCellLocalPosition(c.x,c.y);
        CancelBarricadeVisualRoutine(barrier);ApplyBarricadeLevelVisual(barrier);SetBarricadeFlashAmount(barrier,0);
        CofferMoving?.Invoke(transform.TransformPoint(start),transform.TransformPoint(end),duration);
        // Drive both visuals from the owned gem animation enumerator. No separate
        // coroutine can release ownership, refill or mutate halfway through it.
        float began=Time.time;var movement=AnimateGemMoves(moves);
        while(movement.MoveNext())
        {
            if(barrier.ViewObject!=null)barrier.ViewObject.transform.localPosition=
                Vector3.Lerp(start,end,SmoothStep(Mathf.Clamp01((Time.time-began)/duration)));
            yield return movement.Current;
        }
        if(barrier.ViewObject!=null)barrier.ViewObject.transform.localPosition=end;
        AquaticChanged?.Invoke();
    }

    public void ReleaseAquaticOwner(long owner)
    {
        if (aquatic == null) return;
        aquatic.snares.RemoveAll(s => s.ownerId == owner);
        // Coffers are independent structures once placed. Death releases snares,
        // never oxygen or occupancy. Flood/zone cleanup still discards without reward.
        AquaticChanged?.Invoke();
    }

    private IEnumerator RemoveAirCoffer(bool payout)
    {
        var coffer = aquatic?.coffer; if (coffer == null) yield break;
        aquatic.coffer = null;pendingRoyalCofferSlide=0;
        if (payout) CollectAir(2 * coffer.charges);
        var cell = new Vector2Int(coffer.x, coffer.y);
        if (barricadeCells.TryGetValue(cell, out var barrier) && barrier.Style == EnemyBarricadeStyle.AirCoffer)
        { barricadeCells.Remove(cell); CancelBarricadeVisualRoutine(barrier); DestroyBarricadeView(barrier); }
        yield return ResolveEnvironmentalBoardChange();
        AquaticChanged?.Invoke();
    }

    private static AquaticEnvironmentState CopyAquatic(AquaticEnvironmentState source)
    {
        if(source==null)return null;
        var result=JsonUtility.FromJson<AquaticEnvironmentState>(JsonUtility.ToJson(source));
        // Unity materializes absent nested classes as empty objects. Positive
        // coffer identity is the persisted occupancy discriminator.
        if(result.coffer?.id<=0)result.coffer=null;
        return result;
    }
    private AquaticEnvironmentState CaptureAquatic() => CopyAquatic(aquatic);
    private void RestoreAquatic(AquaticEnvironmentState saved)
    {
        pendingRoyalCofferSlide=0;
        if (saved != null && (saved.version<1 || saved.version>AquaticEnvironmentState.CurrentVersion))
            throw new InvalidOperationException("Unsupported aquatic rules.");
        aquatic = CopyAquatic(saved);
        if(aquatic!=null)
        {
            // Keep a legacy flood's remaining time/resources; only subsequent
            // supply follows the new finite-reserve rule. Never mint replacement AIR.
            if(aquatic.version<2) {aquatic.reserveExhausted=false;aquatic.nextSupplyMove=0;}
            aquatic.version=AquaticEnvironmentState.CurrentVersion;
            aquatic.collectedAmounts??=new List<int>();
            if(aquatic.coffer!=null && barricadeCells.TryGetValue(new Vector2Int(aquatic.coffer.x,aquatic.coffer.y),out var restored))
                ApplyBarricadeLevelVisual(restored);
        }
        aquaticClearReceipts.Clear(); AquaticChanged?.Invoke();
    }

    private void ReconcileAquaticMemory(BoardCombatSnapshot saved, List<Vector2Int> openings)
    {
        foreach (var cell in saved.cells)
            if (cell.barricade && cell.barricadeStyle == EnemyBarricadeStyle.AirCoffer)
            { cell.barricade = false; openings.Add(new Vector2Int(cell.x, cell.y)); }
        var coffer = aquatic?.coffer;
        if (coffer == null) return;
        var pos = new Vector2Int(coffer.x, coffer.y);
        if (!barricadeCells.TryGetValue(pos, out var current)) return;
        var target = saved.cells.Find(c => c.x == pos.x && c.y == pos.y);
        target.hasGem = target.mined = target.banner = target.pinned = target.frozen = target.movable = false;
        target.barricade = true; target.barricadeStyle = EnemyBarricadeStyle.AirCoffer;
        target.barricadeOwner = -1; target.durability = current.RemainingDurability;
        target.maximumDurability = current.MaximumDurability;
        openings.RemoveAll(p => p == pos);
    }

    private void PruneAquaticAfterMemory()
    {
        if (aquatic == null) return;
        aquatic.bubbles.RemoveAll(id => FindAquaticGem(id) == null);
        aquatic.snares.RemoveAll(s => FindAquaticGem(s.gemId) == null);
        var coffer = aquatic.coffer;
        if (coffer != null && barricadeCells.TryGetValue(new Vector2Int(coffer.x, coffer.y), out var barrier))
            barrier.OwnerInstanceId = 0;
        aquaticClearReceipts.Clear();
        AquaticChanged?.Invoke();
    }
}

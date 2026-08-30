using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class CrackedGemsRuntime :
    MonoBehaviour,
    IPlayerAbilityRuntime
{
    [Header("References")]
    [SerializeField]
    private BoardController boardController;

    [SerializeField]
    private WaveController waveController;

    public event Action StateChanged;

    public bool IsActive
    {
        get;
        private set;
    }

    private void OnEnable()
    {
        ResolveReferences();
        EnsurePresentation();
    }

    private void Start()
    {
        ResolveReferences();

        if (!ValidateReferences())
        {
            enabled = false;
        }
    }

    private void OnDisable()
    {
        Cancel();
    }

    public bool Supports(
        CharacterAbilityDefinition definition)
    {
        return definition is
            CrackedGemsAbilityDefinition;
    }

    public bool CanActivate(
        CharacterAbilityDefinition definition)
    {
        ResolveReferences();

        return
            !IsActive &&
            definition is
                CrackedGemsAbilityDefinition crackedDefinition &&
            boardController != null &&
            waveController != null &&
            waveController.IsWaveActive &&
            HasAliveEnemy() &&
            boardController.CanActivateCrackedGems(
                crackedDefinition.TargetGemCount
            );
    }

    public bool TryActivate(
        CharacterAbilityDefinition definition)
    {
        if (!(definition is
                CrackedGemsAbilityDefinition
                    crackedDefinition) ||
            !CanActivate(definition))
        {
            return false;
        }

        EnsurePresentation(
            crackedDefinition.BubbleSprite
        );

        HashSet<GemType> preferredGemTypes =
            BuildPreferredGemTypes();

        IsActive = true;

        bool accepted =
            boardController.TryActivateCrackedGems(
                preferredGemTypes,
                crackedDefinition.TargetGemCount,
                crackedDefinition.CrackedGemDamage,
                crackedDefinition.BubbleTravelDuration,
                crackedDefinition.BubbleHoverDuration,
                crackedDefinition.CrackedShakeDuration,
                crackedDefinition.CrackedBurstScale,
                crackedDefinition.CrackedWhiteHoldDuration,
                HandleResolutionCompleted
            );

        if (!accepted)
        {
            IsActive = false;
            return false;
        }

        StateChanged?.Invoke();
        return true;
    }

    public void Cancel()
    {
        if (!IsActive)
        {
            return;
        }

        /*
         * BoardController owns an accepted ability resolution. Cancelling the
         * UI/runtime state must not interrupt that authoritative board
         * coroutine midway and risk a partially resolved board.
         */
        IsActive = false;
        StateChanged?.Invoke();
    }

    private void HandleResolutionCompleted()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        StateChanged?.Invoke();
    }

    private HashSet<GemType>
        BuildPreferredGemTypes()
    {
        HashSet<GemType> preferredTypes =
            new HashSet<GemType>();

        if (waveController == null)
        {
            return preferredTypes;
        }

        IReadOnlyList<EnemyActor> enemies =
            waveController.ActiveEnemies;

        for (int index = 0;
             index < enemies.Count;
             index++)
        {
            EnemyActor enemy =
                enemies[index];

            if (enemy == null ||
                !enemy.IsInitialized ||
                !enemy.CanReceiveDamage)
            {
                continue;
            }

            preferredTypes.Add(
                enemy.AssignedGemType
            );
        }

        return preferredTypes;
    }

    private bool HasAliveEnemy()
    {
        if (waveController == null)
        {
            return false;
        }

        IReadOnlyList<EnemyActor> enemies =
            waveController.ActiveEnemies;

        for (int index = 0;
             index < enemies.Count;
             index++)
        {
            EnemyActor enemy =
                enemies[index];

            if (enemy != null &&
                enemy.IsInitialized &&
                enemy.CanReceiveDamage)
            {
                return true;
            }
        }

        return false;
    }

    private void ResolveReferences()
    {
        if (boardController == null)
        {
            boardController =
                UnityEngine.Object
                    .FindObjectOfType<
                        BoardController
                    >();
        }

        if (waveController == null)
        {
            waveController =
                GetComponentInParent<
                    WaveController
                >();
        }

        if (waveController == null)
        {
            waveController =
                UnityEngine.Object
                    .FindObjectOfType<
                        WaveController
                    >();
        }
    }

    private void EnsurePresentation(
        Sprite bubbleSprite = null)
    {
        if (boardController == null)
        {
            return;
        }

        CrackedGemBubbleVFX.EnsureInstalled(
            boardController,
            transform,
            bubbleSprite
        );

        CrackedGemOverlayPresenter.EnsureInstalled(
            boardController
        );
    }

    private bool ValidateReferences()
    {
        bool isValid = true;

        if (boardController == null)
        {
            Debug.LogError(
                "CrackedGemsRuntime requires a BoardController.",
                this
            );

            isValid = false;
        }

        if (waveController == null)
        {
            Debug.LogError(
                "CrackedGemsRuntime requires a WaveController.",
                this
            );

            isValid = false;
        }

        return isValid;
    }
}

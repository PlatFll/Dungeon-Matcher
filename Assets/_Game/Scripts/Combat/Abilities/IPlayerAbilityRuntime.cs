using System;

public interface IPlayerAbilityRuntime
{
    event Action StateChanged;

    bool IsActive { get; }

    bool Supports(
        CharacterAbilityDefinition definition
    );

    bool CanActivate(
        CharacterAbilityDefinition definition
    );

    bool TryActivate(
        CharacterAbilityDefinition definition
    );

    void Cancel();
}

// Optional presentation contract for abilities with persistent authored poses.
public interface IPlayerAbilityPresentation
{
    string AnimationState { get; }
    float AnimationNormalizedTime { get; }
    bool ShowMoveCounter { get; }
    int RemainingMoveCount { get; }
}

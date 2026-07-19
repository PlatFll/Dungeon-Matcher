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
# Dungeon Matcher — Repository Instructions

## Project

Dungeon Matcher is a mobile pixel-art match-3 dungeon battler built in Unity.

Before making changes, inspect the relevant existing implementation and follow established architecture rather than creating parallel systems.

## Core gameplay philosophy

- The match-3 layer itself must remain satisfying, decision-rich, readable, fast, and responsive.
- Preserve player agency and perceived control despite randomness.
- Mechanics must support clear player goals.
- Favor depth over complexity.
- Keep mechanics easy to understand but capable of expert optimization.
- Board interference and obstacles must remain readable and fair.
- Enemies should create behavior-based pressure and increasingly manipulate the board rather than turning every level into a rigid puzzle.
- Prefer the simplest fun implementation before adding abstraction or complexity.

## Board architecture

- BoardController is the authority for board rules.
- BoardController must remain character-agnostic.
- Use one authoritative deterministic board-resolution pipeline.
- Do not create secondary or competing board-resolution systems.
- Prevent double resolution, duplicate rewards, duplicate damage, duplicate board mutation, and recursive trigger bugs.
- Once a board action is accepted, board ownership/locking must remain authoritative until resolution completes.
- Settle the board completely before enemy board manipulation occurs.
- Preserve existing special-gem chaining and obstacle interaction semantics unless the feature explicitly changes them.

## Combat architecture

- Damage resolution should remain centralized.
- Do not bypass established combat/damage flows just because a feature needs custom damage.
- Respect enemy weaknesses, damage sources, and existing combat contexts.
- Do not conflate HP and shield. The player has a separate shield system and shield presentation.
- Do not alter shield behavior during unrelated work.

## Player abilities

- Player characters and abilities must use the generic PlayerDefinition / CharacterAbilityDefinition / ability-runtime architecture.
- Do not add character-specific rules directly into BoardController.
- Character-specific runtime logic belongs in the appropriate ability runtime/presentation layer while BoardController exposes generic board operations.
- Energy generation and energy storage/spending must remain separate.
- Ability-source board clears must not accidentally generate energy unless explicitly designed to do so.
- Spend ability energy only after activation has actually been accepted.

## Enemies

- Enemy behavior should remain data-driven through EnemyDefinition / ScriptableObjects and the existing enemy systems.
- Avoid hard-coding individual enemies into shared board logic.
- Enemy manipulation should happen only at safe points in board resolution.

## Gameplay vs presentation

- Separate gameplay logic/timing from presentation, animation, and VFX where practical.
- VFX must not become authoritative for gameplay state.
- Gameplay must still resolve correctly if optional art/VFX assets are missing.
- When appropriate, use safe presentation fallbacks for missing art assets.
- Preserve pixel-art rendering conventions already used by the project.

## Scope and implementation

- Make the smallest architecture-safe change that solves the requested problem.
- Do not perform unrelated refactors while implementing a feature or bug fix.
- Do not introduce abstractions unless they solve a real current problem.
- Reuse established systems and patterns where appropriate.
- Inspect serialized Unity assets, scenes, prefabs, ScriptableObjects, GUID references, and .meta files when they are relevant to the bug.
- Do not assume that changing a C# field alone is sufficient when Unity serialization or scene references may be involved.

## Git workflow

- NEVER implement feature work directly on main.
- Before changing files, make sure the local repository is up to date.
- Create one focused branch per feature or bug fix.
- Keep commits focused.
- Review the complete diff before opening a pull request.
- Do not include unrelated files in a PR.
- Open a pull request after completing and reviewing a requested implementation.
- NEVER merge a pull request unless the user explicitly tells you to merge it.
- NEVER force-push or rewrite main history unless the user explicitly requests it.
- Do not create placeholder/dummy files as part of Git operations.

## Validation

- For changes to C# gameplay/runtime/editor code, run:
  `powershell -ExecutionPolicy Bypass -File Tools/Validate-Unity.ps1`
- Run the validator after implementation and before opening the pull request.
- If validation fails, do not open the PR as completed work. Diagnose and fix errors caused by the change first.
- If validation cannot run because the Unity project is already open, report that validation was blocked rather than claiming success.
- Documentation-only changes do not require Unity validation.
- Art-only changes that do not affect Unity serialization or runtime code do not require Unity validation.
- Never claim Unity validation passed unless `Tools/Validate-Unity.ps1` actually completed successfully.
- Run relevant automated tests when they exist.
- Distinguish code review/static validation from actual Unity runtime validation.

## Safety when editing

Before implementing a requested feature:

1. Inspect the current version of all directly relevant files.
2. Identify the authoritative system responsible for the behavior.
3. Check for serialized Unity references that may affect the behavior.
4. Make the smallest coherent implementation.
5. Review the resulting diff for unintended changes.
6. Run available validation.
7. Open a PR.
8. Stop before merging unless explicitly instructed.

## Communication

When reporting completed work, summarize:
- root cause when fixing a bug,
- files changed,
- behavior changed,
- important systems deliberately left untouched,
- validation actually performed,
- PR number/link if one was created.

Do not claim tests or Unity runtime verification that were not actually performed.
Do not guess about repository state when it can be inspected.

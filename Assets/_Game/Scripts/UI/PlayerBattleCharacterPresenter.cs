using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Keeps the battle character Image and Animator in sync with the selected
/// PlayerDefinition. The scene may contain a legacy Animator Controller, but
/// the selected player definition is the runtime authority.
/// </summary>
[DisallowMultipleComponent]
public sealed class PlayerBattleCharacterPresenter : MonoBehaviour
{
    private const string PlayerCharacterName =
        "PlayerCharacter";

    private Image characterImage;
    private Animator characterAnimator;
    private PlayerActor playerActor;
    private PlayerActor subscribedPlayer;
    private PlayerAbilityController subscribedAbility;
    private static readonly int AbilityTrigger = Animator.StringToHash("Ability");

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad
    )]
    private static void InstallForPlayerCharacter()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        InstallInLoadedScene();
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode) => InstallInLoadedScene();

    private static void InstallInLoadedScene()
    {
        RectTransform[] rects =
            Object.FindObjectsByType<RectTransform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (RectTransform rect in rects)
        {
            if (rect == null ||
                rect.name != PlayerCharacterName ||
                rect.GetComponent<Image>() == null)
            {
                continue;
            }

            if (!rect.TryGetComponent(
                    out PlayerBattleCharacterPresenter _
                ))
            {
                rect.gameObject.AddComponent<
                    PlayerBattleCharacterPresenter
                >();
            }
        }
    }

    private void Awake()
    {
        ResolveReferences();
        SubscribeToPlayer();
        RefreshPresentation();
    }

    private void OnEnable()
    {
        ResolveReferences();
        SubscribeToPlayer();
        RefreshPresentation();
    }

    private void Start()
    {
        ResolveReferences();
        SubscribeToPlayer();
        RefreshPresentation();
    }

    private void OnDisable()
    {
        UnsubscribeFromPlayer();
    }

    private void ResolveReferences()
    {
        if (characterImage == null)
        {
            characterImage =
                GetComponent<Image>();
        }

        if (characterAnimator == null)
        {
            characterAnimator =
                GetComponent<Animator>();
        }

        if (playerActor == null)
        {
            playerActor =
                Object.FindObjectOfType<PlayerActor>();
        }
    }

    private void SubscribeToPlayer()
    {
        if (subscribedPlayer == playerActor)
        {
            return;
        }

        UnsubscribeFromPlayer();

        subscribedPlayer = playerActor;

        if (subscribedPlayer != null)
        {
            subscribedPlayer.Initialized +=
                HandlePlayerInitialized;
            subscribedAbility = subscribedPlayer.GetComponent<PlayerAbilityController>();
            if (subscribedAbility != null)
                subscribedAbility.AbilityActivated += HandleAbilityActivated;
        }
    }

    private void UnsubscribeFromPlayer()
    {
        if (subscribedAbility != null)
            subscribedAbility.AbilityActivated -= HandleAbilityActivated;
        subscribedAbility = null;
        if (subscribedPlayer != null)
        {
            subscribedPlayer.Initialized -=
                HandlePlayerInitialized;
        }

        subscribedPlayer = null;
    }

    private void HandleAbilityActivated()
    {
        if (characterAnimator == null || !characterAnimator.isActiveAndEnabled ||
            characterAnimator.runtimeAnimatorController == null) return;
        foreach (var parameter in characterAnimator.parameters)
        {
            if (parameter.nameHash != AbilityTrigger ||
                parameter.type != AnimatorControllerParameterType.Trigger) continue;
            characterAnimator.SetTrigger(AbilityTrigger);
            break;
        }
    }

    private void HandlePlayerInitialized(
        PlayerActor initializedPlayer)
    {
        if (initializedPlayer != playerActor)
        {
            return;
        }

        RefreshPresentation();
    }

    private void RefreshPresentation()
    {
        PlayerDefinition definition =
            playerActor != null &&
            playerActor.IsInitialized
                ? playerActor.Definition
                : null;

        Sprite battleSprite =
            definition != null
                ? definition.BattleCharacterSprite
                : null;

        RuntimeAnimatorController animatorController =
            definition != null
                ? definition.BattleAnimatorController
                : null;

        if (characterAnimator != null)
        {
            characterAnimator.enabled = false;
            characterAnimator.runtimeAnimatorController =
                animatorController;
        }

        if (characterImage != null)
        {
            characterImage.sprite = battleSprite;
            characterImage.preserveAspect = true;
            characterImage.enabled =
                battleSprite != null ||
                animatorController != null;
        }

        if (characterAnimator == null ||
            animatorController == null)
        {
            return;
        }

        characterAnimator.enabled = true;
        characterAnimator.Rebind();
        characterAnimator.Update(0f);
    }
}

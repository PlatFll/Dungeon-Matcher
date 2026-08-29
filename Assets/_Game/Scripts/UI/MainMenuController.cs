using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class MainMenuController : MonoBehaviour
{
    private const string GameSceneName = "Game";
    private const string CharacterSelectPrefabPath =
        "UI/CharacterSelectScreen";

    [Header("Screens")]
    [SerializeField]
    private GameObject homeScreen;

    [SerializeField]
    private GameObject gemMasteryScreen;

    [Header("Navigation")]
    [SerializeField]
    private Button playButton;

    [SerializeField]
    private Button gemMasteryButton;

    [SerializeField]
    private Button gemMasteryBackButton;

    private CharacterSelectMenuController
        characterSelectScreen;

    private bool isLoadingGame;

    private void Awake()
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        playButton.onClick.AddListener(
            ShowCharacterSelect
        );

        gemMasteryButton.onClick.AddListener(
            ShowGemMastery
        );

        gemMasteryBackButton.onClick.AddListener(
            ShowHome
        );

        ShowHome();
    }

    private void OnDestroy()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(
                ShowCharacterSelect
            );
        }

        if (gemMasteryButton != null)
        {
            gemMasteryButton.onClick.RemoveListener(
                ShowGemMastery
            );
        }

        if (gemMasteryBackButton != null)
        {
            gemMasteryBackButton.onClick.RemoveListener(
                ShowHome
            );
        }
    }

    public void ShowHome()
    {
        if (homeScreen == null ||
            gemMasteryScreen == null)
        {
            return;
        }

        homeScreen.SetActive(true);
        gemMasteryScreen.SetActive(false);

        if (characterSelectScreen != null)
        {
            characterSelectScreen.gameObject.SetActive(
                false
            );
        }
    }

    public void ShowGemMastery()
    {
        if (homeScreen == null ||
            gemMasteryScreen == null)
        {
            return;
        }

        homeScreen.SetActive(false);
        gemMasteryScreen.SetActive(true);

        if (characterSelectScreen != null)
        {
            characterSelectScreen.gameObject.SetActive(
                false
            );
        }
    }

    public void ShowCharacterSelect()
    {
        if (!EnsureCharacterSelectScreen())
        {
            return;
        }

        homeScreen.SetActive(false);
        gemMasteryScreen.SetActive(false);
        characterSelectScreen.gameObject.SetActive(true);
    }

    public void PlayGame()
    {
        if (isLoadingGame)
        {
            return;
        }

        if (!PlayerDefinitionRegistry.IsAvailable(
                CharacterSelectionSettings.SelectedPlayerId
            ))
        {
            Debug.LogWarning(
                "The selected character does not have a PlayerDefinition yet.",
                this
            );

            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(
                GameSceneName
            ))
        {
            Debug.LogError(
                $"Main menu could not load scene '{GameSceneName}'. " +
                "Make sure it is enabled in Build Settings.",
                this
            );

            return;
        }

        isLoadingGame = true;

        SceneManager.LoadScene(
            GameSceneName,
            LoadSceneMode.Single
        );
    }

    private bool EnsureCharacterSelectScreen()
    {
        if (characterSelectScreen != null)
        {
            characterSelectScreen.Initialize(
                PlayGame,
                ShowHome
            );

            return true;
        }

        GameObject prefab =
            Resources.Load<GameObject>(
                CharacterSelectPrefabPath
            );

        if (prefab == null)
        {
            Debug.LogError(
                $"Could not load character select prefab at Resources/{CharacterSelectPrefabPath}.",
                this
            );

            return false;
        }

        Transform screenParent =
            homeScreen.transform.parent;

        GameObject instance =
            Instantiate(
                prefab,
                screenParent,
                false
            );

        instance.name =
            "CharacterSelectScreen";

        characterSelectScreen =
            instance.GetComponent<CharacterSelectMenuController>();

        if (characterSelectScreen == null)
        {
            Debug.LogError(
                "Character select prefab is missing CharacterSelectMenuController.",
                this
            );

            Destroy(instance);
            return false;
        }

        characterSelectScreen.Initialize(
            PlayGame,
            ShowHome
        );

        return true;
    }

    private bool HasRequiredReferences()
    {
        bool hasAllReferences =
            homeScreen != null &&
            gemMasteryScreen != null &&
            playButton != null &&
            gemMasteryButton != null &&
            gemMasteryBackButton != null;

        if (hasAllReferences)
        {
            return true;
        }

        Debug.LogError(
            "MainMenuController requires Home/Gem Mastery screen " +
            "references plus Play, Gem Mastery, and Back buttons.",
            this
        );

        return false;
    }
}

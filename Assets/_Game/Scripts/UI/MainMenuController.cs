using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class MainMenuController : MonoBehaviour
{
    private const string GameSceneName = "Game";

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

    private bool isLoadingGame;

    private void Awake()
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        playButton.onClick.AddListener(
            PlayGame
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
                PlayGame
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
    }

    public void PlayGame()
    {
        if (isLoadingGame)
        {
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

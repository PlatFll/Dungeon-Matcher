using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class MainMenuController : MonoBehaviour
{
    private const string GameSceneName = "Game";

    [SerializeField]
    private Button playButton;

    private bool isLoadingGame;

    private void Awake()
    {
        if (playButton == null)
        {
            Debug.LogError(
                "MainMenuController requires a Play Button reference.",
                this
            );

            return;
        }

        playButton.onClick.AddListener(
            PlayGame
        );
    }

    private void OnDestroy()
    {
        if (playButton == null)
        {
            return;
        }

        playButton.onClick.RemoveListener(
            PlayGame
        );
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
}

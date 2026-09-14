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
    private ShopMenuController shopScreen;
    private Text accountLabel;

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

        ConfigureHome();
        ShowHome();
    }

    private void OnDestroy()
    {
        AccountProgression.Current.Changed -= RefreshAccount;
        CharacterSelectionSettings.Changed -= RefreshAccount;
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
        if (shopScreen != null) shopScreen.gameObject.SetActive(false);
        RefreshAccount();

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

    private void ConfigureHome()
    {
        // The serialized HomeContent uses a VerticalLayoutGroup. Reparent the
        // existing controls into the same explicit layout as the new navigation.
        var title = homeScreen.transform.Find("HomeContent/Title") as RectTransform;
        if (title != null)
        {
            title.SetParent(homeScreen.transform, false);
            title.anchorMin=title.anchorMax=title.pivot=new Vector2(.5f,.5f);
            title.anchoredPosition=new Vector2(0,240);title.sizeDelta=new Vector2(420,100);
            title.GetComponent<Text>().fontSize=36;
        }
        var playRect=(RectTransform)playButton.transform;
        playRect.SetParent(homeScreen.transform,false);
        playRect.anchorMin=playRect.anchorMax=new Vector2(0.5f,0.5f);playRect.anchoredPosition=new Vector2(0,40);playRect.sizeDelta=new Vector2(300,56);
        var masteryRect=(RectTransform)gemMasteryButton.transform;
        masteryRect.SetParent(homeScreen.transform,false);
        masteryRect.anchorMin=masteryRect.anchorMax=new Vector2(0.5f,0.5f);masteryRect.anchoredPosition=new Vector2(0,-170);masteryRect.sizeDelta=new Vector2(300,56);
        GameUi.Button("CharactersButton",homeScreen.transform,"Characters",new Vector2(300,56),new Vector2(0,-30),ShowCharacterSelect);
        GameUi.Button("ShopButton",homeScreen.transform,"Shop",new Vector2(300,56),new Vector2(0,-100),ShowShop);
        accountLabel=GameUi.Label("AccountSummary",homeScreen.transform,"",new Vector2(420,76),new Vector2(0,130),21);
        AccountProgression.Current.Changed += RefreshAccount;
        CharacterSelectionSettings.Changed += RefreshAccount;
    }
    private void RefreshAccount()
    {
        if(accountLabel==null)return;
        string id=CharacterSelectionSettings.SelectedPlayerId;
        PlayerDefinitionRegistry.TryGetDefinition(id,out var definition);
        accountLabel.text=$"{(definition != null ? definition.DisplayName : id)} - Level {AccountProgression.Current.Level(id)}\nGold Coins: {AccountProgression.Current.Gold}";
    }
    public void ShowShop()
    {
        if(shopScreen==null)
        {
            var rect=GameUi.Rect("ShopScreen",homeScreen.transform.parent,Vector2.zero,Vector2.zero);
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            shopScreen=rect.gameObject.AddComponent<ShopMenuController>();shopScreen.Initialize(ShowHome);
        }
        homeScreen.SetActive(false);gemMasteryScreen.SetActive(false);
        if(characterSelectScreen!=null)characterSelectScreen.gameObject.SetActive(false);
        shopScreen.gameObject.SetActive(true);
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

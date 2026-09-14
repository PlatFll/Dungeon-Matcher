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
    private RectTransform challengePanel;
    private Button endSavedRun;

    private void Start()
    {
        if (RunLaunchOptions.ChangeBuild) { RunLaunchOptions.ChangeBuild = false; ShowCharacterSelect(); }
    }

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
        GameUi.Button("PracticeButton",homeScreen.transform,"Free practice",new Vector2(145,48),new Vector2(-78,-238),()=>{RunLaunchOptions.Practice=true;PlayGame();});
        GameUi.Button("ChallengesButton",homeScreen.transform,"Challenges",new Vector2(145,48),new Vector2(78,-238),ShowChallenges);
        endSavedRun=GameUi.Button("EndSavedRun",homeScreen.transform,"End saved run",new Vector2(300,44),new Vector2(0,-302),ConfirmEndSavedRun);
        accountLabel=GameUi.Label("AccountSummary",homeScreen.transform,"",new Vector2(420,76),new Vector2(0,130),21);
        AccountProgression.Current.Changed += RefreshAccount;
        CharacterSelectionSettings.Changed += RefreshAccount;
    }
    private void RefreshAccount()
    {
        if(accountLabel==null)return;
        var run=AccountProgression.Current.ActiveRun;
        string id=run?.playerId ?? CharacterSelectionSettings.SelectedPlayerId;
        PlayerDefinitionRegistry.TryGetDefinition(id,out var definition);
        accountLabel.text=$"{(run!=null?"Saved: ":"")}{(definition != null ? definition.DisplayName : id)} - Level {run?.level ?? AccountProgression.Current.Level(id)}\nGold Coins: {AccountProgression.Current.Gold}";
        playButton.GetComponentInChildren<Text>().text=run==null?"Play":"Continue wave "+(run.checkpoint?.wave>0?run.checkpoint.wave:1);
        if(endSavedRun!=null) endSavedRun.gameObject.SetActive(run!=null);
    }
    private void ConfirmEndSavedRun()
    {
        var account=AccountProgression.Current; var run=account.ActiveRun;
        if(run==null || challengePanel!=null) return;
        homeScreen.SetActive(false);
        challengePanel=GameUi.Panel("EndRunConfirmation",homeScreen.transform.parent,new Vector2(440,460));
        GameUi.Label("Title",challengePanel,"End this attempt?",new Vector2(400,55),new Vector2(0,175),28);
        GameUi.Label("Reward",challengePanel,account.PreviewReward("End Run")+"\n\nYour saved board and build will end.\nUnused items stay owned.",new Vector2(390,225),new Vector2(0,25),19);
        GameUi.Button("End",challengePanel,"End run and collect gold",new Vector2(350,46),new Vector2(0,-125),()=>
        {
            if(!account.FinalizeRun(run.id,"End Run")) return;
            Destroy(challengePanel.gameObject); challengePanel=null; ShowHome();
        });
        GameUi.Button("Back",challengePanel,"Keep saved run",new Vector2(350,46),new Vector2(0,-185),()=>
        {Destroy(challengePanel.gameObject);challengePanel=null;ShowHome();});
    }
    public void ShowChallenges()
    {
        if (challengePanel != null) return;
        homeScreen.SetActive(false);
        challengePanel=GameUi.Panel("Challenges",homeScreen.transform.parent,new Vector2(480,580));
        GameUi.Label("Title",challengePanel,"Personal challenges",new Vector2(440,55),new Vector2(0,230),28);
        bool unlocked=AccountProgression.Current.HasClearedKing;
        GameUi.Label("Rules",challengePanel,unlocked ? "Always available. No expiry or attendance rewards.\nNormal rewards; personal records stay separate." : "Defeat the King to unlock optional challenge runs.\nFree practice is already available from Home.",new Vector2(430,90),new Vector2(0,150),20);
        for(int i=1;i<=2;i++)
        {
            var challenge=(RunChallenge)i;var record=AccountProgression.Current.Record(challenge);
            string label=i==1?"No Supplies — abilities allowed":"Board Only — no ability or supplies";
            var button=GameUi.Button("Challenge"+i,challengePanel,label+ $"\nBest wave {record.bestWave} · Wins {record.victories}",new Vector2(420,80),new Vector2(0,40-(i-1)*105),()=>{RunLaunchOptions.Challenge=challenge;PlayGame();});
            button.interactable=unlocked && AccountProgression.Current.ActiveRun==null;
        }
        GameUi.Button("Back",challengePanel,"Back",new Vector2(280,48),new Vector2(0,-215),()=>{Destroy(challengePanel.gameObject);challengePanel=null;ShowHome();});
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

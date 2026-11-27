using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class BalanceRuntimeBootstrap : MonoBehaviour
{
    private float nextFontScan;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        SceneManager.sceneLoaded += SceneLoaded;
    }
    private static void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Game" && scene.name != "MainMenu") return;
        var root = new GameObject("BalanceRuntime");
        root.AddComponent<BalanceRuntimeBootstrap>();
        if (scene.name == "Game")
        {
            root.AddComponent<RunSession>();
            root.AddComponent<RunControlsUI>();
        }
    }
    private void LateUpdate()
    {
        if (Time.unscaledTime < nextFontScan) return;
        nextFontScan = Time.unscaledTime + 0.25f;
        foreach (var label in FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (label.font != GameUi.Font) label.font = GameUi.Font;
        foreach (var label in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            // These menu screens own their reading fonts and responsive type sizes.
            if (gameObject.scene.name == "MainMenu" && (label.GetComponentInParent<ShopMenuController>(true) ||
                label.GetComponentInParent<CharacterSelectMenuController>(true))) continue;
            PixelTextFitter.Apply(label);
        }
    }
}

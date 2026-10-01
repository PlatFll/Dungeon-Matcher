using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Presentation checks through production actors, pooled text and actual scene layout.</summary>
public static class HudTypographyReview
{
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
    private static TMP_Text[] Numbers => Object.FindObjectsByType<FloatingCombatText>(FindObjectsSortMode.None)
        .Select(x => x.GetComponent<TMP_Text>()).Where(t => t.isActiveAndEnabled).ToArray();
    private static CombatTextStyleLibrary Styles => AssetDatabase.LoadAssetAtPath<CombatTextStyleLibrary>("Assets/_Game/Data/Combat Text/CombatTextStyles_Default.asset");
    private static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); }

    public static void ValidateHud()
    {
        Check(GameObject.Find("EnergyAmount") == null, "Bottom HUD energy text removed");
        var player = GameObject.Find("PlayerDungeonBackdrop").GetComponent<Image>();
        var bottom = GameObject.Find("BottomDungeonBackdrop").GetComponent<Image>();
        Check(player.sprite == bottom.sprite && player.color == bottom.color && player.type == bottom.type, "HUD uses same dark tiled background as player");
        var ability = Object.FindFirstObjectByType<AbilityButtonUI>().GetComponent<RectTransform>();
        var potion = GameObject.Find("HealthPotion").GetComponent<RectTransform>();
        var bomb = GameObject.Find("Bomb").GetComponent<RectTransform>();
        Check(Mathf.Abs(potion.position.y-bomb.position.y)<.01f && Mathf.Abs(potion.position.y-ability.position.y)<.01f, "ability and supplies aligned");
        Check(Mathf.Abs((potion.position.x+bomb.position.x)*.5f-ability.position.x)<.01f, "supplies centered around ability");
        foreach (var rect in new[] {potion,bomb})
        {
            var button=rect.GetComponent<Button>();
            Check(button.transition==Selectable.Transition.SpriteSwap && button.spriteState.disabledSprite!=null && button.spriteState.pressedSprite!=null, "all powerup tile states assigned");
        }
        for(int i=0;i<8;i++) Check((int)Styles.GetStyle((CombatTextKind)i).Kind==i && Styles.GetStyle((CombatTextKind)i).Color.a==1, "explicit opaque combat type style");
        var wave=GameObject.Find("WaveText").GetComponent<TMP_Text>();
        string prior=wave.text;
        wave.text="WAVE 999"; PixelTextFitter.Apply(wave);
        Check(wave.preferredWidth<=wave.rectTransform.rect.width+.5f && wave.preferredHeight<=wave.rectTransform.rect.height+.5f,"three-digit wave fits plaque");
        wave.text=prior; PixelTextFitter.Apply(wave);
    }

    public static void ValidateText(string shot, string output)
    {
        var report=new StringBuilder();
        foreach(var text in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Where(t=>t.enabled && !string.IsNullOrEmpty(t.text)))
        {
            PixelTextFitter.Apply(text); text.ForceMeshUpdate();
            Check(text.font==GameUi.TmpFont && text.font.name=="Thaleah Bitmap", "Thaleah assigned: "+text.name);
            Check(text.font.atlasTextures.All(t=>t.filterMode==FilterMode.Point), "Point font atlas");
            Check(text.fontSharedMaterial.shader.name.Contains("Bitmap"), "bitmap text material: "+text.name);
            float physical=text.fontSize*text.canvas.rootCanvas.scaleFactor/16;
            Check(Mathf.Abs(physical-Mathf.Round(physical))<.001f, "integer glyph scaling: "+text.name);
            foreach (var c in text.text.Where(c=>!char.IsControl(c))) Check(text.font.HasCharacter(c), "missing Thaleah glyph "+(int)c+" in "+text.name);
            var preferred=text.GetPreferredValues(text.text,text.rectTransform.rect.width,float.PositiveInfinity);
            bool scroll=text.name=="GuideText"||text.name=="BuildRecap";
            Check(preferred.x<=text.rectTransform.rect.width+.5f && (scroll||preferred.y<=text.rectTransform.rect.height+.5f), "text container overflow: "+text.name+" "+preferred+" / "+text.rectTransform.rect.size);
            report.AppendLine(text.name+" | size="+text.fontSize+" | rect="+text.rectTransform.rect.size+" | "+text.text.Replace('\n',' '));
        }
        File.WriteAllText(Path.Combine(output,shot+"-typography.txt"),report.ToString());
    }

    public static IEnumerator Feedback(int height, Func<string,IEnumerator> shot)
    {
        var player=RunSession.Current.Player;
        var enemy=RunSession.Current.Waves.ActiveEnemies.First();
        var controller=Object.FindFirstObjectByType<CombatTextController>();
        yield return Drain();
        // Re-enable twice to catch duplicate listeners and lost enemy bindings.
        controller.enabled=false; controller.enabled=true; controller.enabled=false; controller.enabled=true;
        int beforePlayer=player.CurrentHealth, beforeEnemy=enemy.CurrentHealth;
        player.TryTakeDamage(15); enemy.TryTakeDamage(15);
        CheckNumber("-"+(beforePlayer-player.CurrentHealth),CombatTextKind.Damage,2);
        Check(beforePlayer-player.CurrentHealth==beforeEnemy-enemy.CurrentHealth,"fixture damage equal");
        yield return shot(height+"-damage-white"); yield return Drain();
        Check(player.Heal(5)==5 && enemy.RestoreHealth(5)==5,"actual five HP restored");
        CheckNumber("+5",CombatTextKind.Healing,2);
        yield return shot(height+"-healing-green"); yield return Drain();
        Check(player.Heal(500)==10 && enemy.RestoreHealth(500)==10,"healing capped to missing HP");
        CheckNumber("+10",CombatTextKind.Healing,2);
        Check(player.Heal(5)==0 && enemy.RestoreHealth(5)==0 && Numbers.Length==2,"no zero heal numbers");
        yield return Drain();
        Check(player.GrantShield(10)==10 && enemy.GrantShield(10)==10,"actual ten shield gained");
        CheckNumber("+10",CombatTextKind.Shield,2);
        yield return shot(height+"-shield-blue");
        var fading=Numbers[0]; Time.timeScale=1;
        float end=Time.realtimeSinceStartup+5;
        while(fading.gameObject.activeSelf && fading.color.a>=.95f) {Check(Time.realtimeSinceStartup<end,"fade begins");yield return null;}
        Time.timeScale=0; Check(fading.gameObject.activeSelf && fading.color.a>0 && fading.color.a<.95f,"floating number fades before release");
        yield return shot(height+"-feedback-fading"); yield return Drain();
        int old=enemy.CurrentShield; enemy.TryTakeDamage(4);
        CheckNumber("-"+(old-enemy.CurrentShield),CombatTextKind.Shield,1); yield return Drain();
        while(enemy.HasShield) enemy.TryTakeDamageWithoutFeedback(1);
        Check(Object.FindFirstObjectByType<CombatController>().ApplyPoisonToAllEnemies()==1,"production poison application");
        var poison=enemy.GetComponent<EnemyPoisonStatus>();
        Check(poison!=null,"production poison component");
        poison.Apply(2,1,3);
        typeof(EnemyPoisonStatus).GetMethod("ApplyTick",Private).Invoke(poison,null);
        CheckNumber("-5",CombatTextKind.PoisonDamage,1);
        yield return shot(height+"-poison-green"); poison.ClearPoison(); yield return Drain();
        enemy.RestoreHealth(999); yield return Drain();
    }

    private static void CheckNumber(string value, CombatTextKind kind,int count)
    {
        var numbers=Numbers;
        Check(numbers.Length==count,"one pooled number per actual event: "+kind+" got "+numbers.Length);
        foreach(var number in numbers)
        {
            Check(number.text==value,"actual amount: "+number.text+" expected "+value);
            Check(number.color==Styles.GetStyle(kind).Color,"effect color "+kind);
            Check(number.font==GameUi.TmpFont,"feedback immediately uses Thaleah");
        }
    }
    private static IEnumerator Drain()
    {
        Time.timeScale=1;float end=Time.realtimeSinceStartup+5;
        while(Numbers.Length>0){Check(Time.realtimeSinceStartup<end,"pooled feedback returns after lifetime");yield return null;}
        Time.timeScale=0;
    }
}

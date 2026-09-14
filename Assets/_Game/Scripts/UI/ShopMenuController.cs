using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class ShopMenuController : MonoBehaviour
{
    private Text wallet, feedback;
    private readonly Text[] quantities = new Text[2];
    private readonly Button[] buy = new Button[2], equip = new Button[2];
    private Action back;

    public void Initialize(Action onBack)
    {
        back = onBack;
        var panel = GameUi.Panel("ShopPanel", transform, new Vector2(460, 620));
        GameUi.Label("Title", panel, "Shop", new Vector2(420, 54), new Vector2(0, 260), 32);
        wallet = GameUi.Label("Gold", panel, "", new Vector2(420, 42), new Vector2(0, 210), 23);
        feedback=GameUi.Label("Feedback",panel,"",new Vector2(420,26),new Vector2(0,175),14);
        for (int i = 0; i < 2; i++)
        {
            var kind = (ConsumableKind)i;
            float y = 100 - i * 210;
            GameUi.Label("ItemTitle", panel, i == 0 ? "Health Potion" : "Bomb", new Vector2(380, 35), new Vector2(0,y+40), 24);
            var icon = GameUi.Rect("ItemIcon", panel, new Vector2(64,64), new Vector2(-150,y));
            var image = icon.gameObject.AddComponent<Image>();
            image.sprite = Resources.Load<Sprite>(i == 0 ? "UI/Consumables/Potion" : "UI/Consumables/Bomb"); image.preserveAspect = true;
            GameplayPixelGrid.FitImage(image,new Vector2(64,64));
            quantities[i] = GameUi.Label("Quantity", panel, "", new Vector2(290, 48), new Vector2(40,y), 19);
            buy[i] = GameUi.Button("Buy", panel, "", new Vector2(185,44), new Vector2(-104,y-62), () => { AccountProgression.Current.TryPurchase(kind); Refresh(); });
            equip[i] = GameUi.Button("Equip", panel, "", new Vector2(185,44), new Vector2(104,y-62), () => { var a=AccountProgression.Current; a.SetEquipped(kind,!a.Equipped(kind)); Refresh(); });
        }
        GameUi.Label("Limits", panel, $"Each equipped item loads up to {BalanceV1.Current.maximumRunCharges} uses per run.\nUnused items stay owned. Independent {BalanceV1.Current.consumableCooldown:0.#}s cooldowns.", new Vector2(420,48),new Vector2(0,-240),16);
        GameUi.Button("Back",panel,"Back",new Vector2(160,42),new Vector2(0,-285),()=>back?.Invoke());
        Refresh();
    }
    private void OnEnable() { AccountProgression.Current.Changed += Refresh; Refresh(); }
    private void OnDisable() { AccountProgression.Current.Changed -= Refresh; }
    private void Refresh()
    {
        if (wallet == null) return;
        var account = AccountProgression.Current;
        wallet.text = $"Gold Coins: {account.Gold}";
        feedback.text=account.LastError!=null?"Save failed. Check storage, then try again.":"";
        for (int i=0;i<2;i++)
        {
            var kind=(ConsumableKind)i;
            int price=i==0?BalanceV1.Current.potionPrice:BalanceV1.Current.bombPrice;
            int width=BalanceV1.Current.consumableBombRadius*2+1;
            quantities[i].text=$"Owned: {account.Owned(kind)}\n"+(i==0?$"Restore {BalanceV1.Current.potionHealthFraction*100:0}% maximum HP":$"Choose a gem: clear a {width} x {width} area");
            buy[i].GetComponentInChildren<Text>().text=$"Buy 1 - {price} gold";
            buy[i].interactable=account.Gold>=price&&account.Owned(kind)<9999;
            equip[i].GetComponentInChildren<Text>().text=account.Equipped(kind)?"Unequip":"Equip";
            equip[i].interactable=account.Owned(kind)>0||account.Equipped(kind);
        }
    }
}

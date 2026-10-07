using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Optional presentation of board-owned state. Never advances AIR, moves or casts.</summary>
[DefaultExecutionOrder(11200)]
public sealed class AquaticEnvironmentView : MonoBehaviour
{
    private BoardController board;
    private RunSession run;
    private GameplayThemeDefinition theme;
    private readonly Dictionary<string, SpriteRenderer> overlays = new Dictionary<string, SpriteRenderer>();
    private readonly Dictionary<string, TMP_Text> captions = new Dictionary<string, TMP_Text>();
    private RectTransform strip, water, sceneWater, waterline;
    private TMP_Text airLabel, tideLabel;
    private Image[] blocks;
    private Image waterImage;
    private Image sceneWaterImage;
    private TMP_Text airReturn;
    private readonly Queue<(int air,string label)> airSteps=new Queue<(int,string)>();
    private float airStepUntil;
    private int displayedAir=-1;
    private float waterAmount;
    private void Start()
    {
        run=RunSession.Current;board=GetComponent<BoardController>();theme=run?.Zone?.Definition?.theme;
        if(board==null || theme==null || run.Zone.Definition.periodicallyFloods!=true) {enabled=false;return;}
        var layout=FindFirstObjectByType<GameplayPixelLayoutController>();
        if(layout?.TopHud==null) return;
        strip=GameUi.Rect("CourtAir",layout.TopHud,new Vector2(300,22),Vector2.zero);
        strip.anchorMin=strip.anchorMax=new Vector2(.5f,0);strip.anchoredPosition=new Vector2(0,16);
        var backdrop=strip.gameObject.AddComponent<Image>();backdrop.color=new Color(.04f,.09f,.12f,.95f);backdrop.raycastTarget=false;
        airLabel=GameUi.Label("Air",strip,"AIR",new Vector2(48,20),new Vector2(-124,0),16);
        blocks=new Image[5];
        for(int i=0;i<5;i++)
        {
            var cell=GameUi.Rect("AirBlock"+i,strip,new Vector2(18,14),new Vector2(-85+i*23,0));
            var frame=cell.gameObject.AddComponent<Image>();frame.color=new Color(.7f,.85f,.82f);frame.raycastTarget=false;
            var inset=GameUi.Rect("Fill",cell,new Vector2(12,8),Vector2.zero);blocks[i]=inset.gameObject.AddComponent<Image>();blocks[i].raycastTarget=false;
        }
        tideLabel=GameUi.Label("Tide",strip,"TIDE",new Vector2(118,20),new Vector2(89,0),16);
        // Water sits behind gameplay actors/text; its alpha exception is limited
        // to this documented compositing layer, never sprite edges or gems.
        water=GameUi.Rect("CourtWater",layout.transform,Vector2.zero,Vector2.zero);
        water.pivot=new Vector2(.5f,0);water.anchorMin=Vector2.zero;water.anchorMax=new Vector2(1,0);water.sizeDelta=Vector2.zero;
        water.SetAsFirstSibling();waterImage=water.gameObject.AddComponent<Image>();waterImage.raycastTarget=false;
        // Scenery tint is behind actor portraits, status text and the frame.
        sceneWater=GameUi.Rect("CourtSceneryWater",layout.TopHud,Vector2.zero,Vector2.zero);
        sceneWater.anchorMin=Vector2.zero;sceneWater.anchorMax=Vector2.one;sceneWater.sizeDelta=Vector2.zero;
        sceneWater.SetAsFirstSibling();sceneWaterImage=sceneWater.gameObject.AddComponent<Image>();sceneWaterImage.raycastTarget=false;
        waterline=GameUi.Rect("TideSurface",water,new Vector2(0,2),Vector2.zero);
        waterline.anchorMin=new Vector2(0,1);waterline.anchorMax=Vector2.one;waterline.sizeDelta=new Vector2(0,2);
        var line=waterline.gameObject.AddComponent<Image>();line.color=new Color(.65f,.91f,.85f,.5f);line.raycastTarget=false;
        airReturn=GameUi.Label("AirReturned",strip,"+2",new Vector2(40,20),new Vector2(-20,20),16);
        airReturn.color=new Color(.5f,1,.85f);airReturn.gameObject.SetActive(false);
        board.AirReceipt+=AnimateAirReceipt;
        if(GetComponent<CourtBoardEffects>()==null)gameObject.AddComponent<CourtBoardEffects>();
    }
    private void LateUpdate()
    {
        if(board==null || theme==null || run==null) return;
        var state=board.Aquatic;bool wet=board.IsFlooded;
        waterAmount=PresentationPreferences.ReducedMotion?(wet?1:0):Mathf.MoveTowards(waterAmount,wet?1:0,Time.deltaTime/.35f);
        if(waterImage!=null)
        {
            water.anchorMax=new Vector2(1,waterAmount);water.anchoredPosition=Vector2.zero;
            waterImage.color=new Color(.15f,.58f,.66f,.055f);
            water.gameObject.SetActive(waterAmount>0);
            waterline.gameObject.SetActive(!PresentationPreferences.ReducedMotion && waterAmount>0 && waterAmount<1);
        }
        if(sceneWaterImage!=null)sceneWaterImage.color=new Color(.15f,.58f,.66f,.15f*waterAmount);
        if(!wet) {airSteps.Clear();displayedAir=-1;airStepUntil=0;}
        else if(Time.time>=airStepUntil)
        {
            if(airSteps.Count>0)
            {
                var step=airSteps.Dequeue();displayedAir=step.air;airReturn.text=step.label;
                airReturn.color=step.label.StartsWith("-")?new Color(1,.8f,.6f):new Color(.5f,1,.85f);
                airStepUntil=Time.time+.24f;
            }
            else displayedAir=state.air;
        }
        if(airReturn!=null)airReturn.gameObject.SetActive(wet && Time.time<airStepUntil);
        BackgroundMusicPlayer.Instance?.SetUnderwaterMix(wet);
        if(strip!=null)
        {
            strip.gameObject.SetActive(wet);
            if(wet)
            {
                airLabel.color=state.air<=1?new Color(1,.6f,.35f):Color.white;
                int shown=displayedAir<0?state.air:displayedAir;
                for(int i=0;i<5;i++) {blocks[i].color=i<shown?new Color(.45f,.9f,1):new Color(.06f,.12f,.16f);
                    blocks[i].rectTransform.sizeDelta=i<shown?new Vector2(12,8):new Vector2(12,2);}
                tideLabel.text=state.air==0?"NO AIR! "+state.wetMoves:"TIDE "+state.wetMoves;
            }
        }
        var used=new HashSet<string>();
        if(state!=null)
        {
            foreach(int id in state.bubbles)
            {
                var gem=board.FindAquaticGem(id);if(gem==null)continue;
                Show("bubble"+id,theme.airBubble,gem.transform.position,1,used);
            }
            foreach(var snare in state.snares)
            {
                var gem=board.FindAquaticGem(snare.gemId);if(gem==null)continue;
                float size=Mathf.Clamp((snare.expiresMove-board.CompletedValidPlayerMoves)/3f,.34f,1);
                Show("snare"+snare.gemId,theme.thornySnare,gem.transform.position,size,used);
            }
            if(state.coffer!=null)
            {
                var c=state.coffer;var p=board.AirCofferVisualPosition ?? board.transform.TransformPoint(board.GetCellLocalPosition(c.x,c.y));
                Caption("coffer",c.charges.ToString(),p,used);
            }
        }
        // Active caster targets are presented by BoardCasterSigilView. Keep this
        // layer for physical bubbles, snares and coffers, without effect icons.
        foreach(var key in overlays.Keys.ToArray())if(!used.Contains(key)){Destroy(overlays[key].gameObject);overlays.Remove(key);}
        foreach(var key in captions.Keys.ToArray())if(!used.Contains(key)){Destroy(captions[key].gameObject);captions.Remove(key);}
    }
    private void Show(string key,Sprite sprite,Vector3 position,float size,HashSet<string> used,float alpha=1)
    {
        if(sprite==null)return;used.Add(key);
        if(!overlays.TryGetValue(key,out var renderer))
        {var go=new GameObject(key);go.transform.SetParent(board.transform,false);renderer=go.AddComponent<SpriteRenderer>();overlays.Add(key,renderer);
         renderer.sortingLayerName="Gems";renderer.sortingOrder=20;renderer.sprite=sprite;renderer.maskInteraction=SpriteMaskInteraction.VisibleInsideMask;}
        renderer.transform.position=position;renderer.transform.localScale=Vector3.one*board.CellSize/sprite.bounds.size.x*.95f*size;
        renderer.color=new Color(1,1,1,alpha);
    }
    private void Caption(string key,string value,Vector3 position,HashSet<string> used)
    {
        used.Add(key);
        if(!captions.TryGetValue(key,out var label))
        {var go=new GameObject(key);go.transform.SetParent(board.transform,false);var tmp=go.AddComponent<TextMeshPro>();
         tmp.font=GameUi.TmpFont;tmp.fontSize=4;tmp.alignment=TextAlignmentOptions.Center;tmp.color=Color.white;
         tmp.GetComponent<MeshRenderer>().sortingLayerName="Gems";tmp.GetComponent<MeshRenderer>().sortingOrder=24;
         label=tmp;captions.Add(key,label);}
        label.text=value;label.ForceMeshUpdate();
        // A world TMP em is not a board pixel. Fit seven-pixel Thaleah capitals
        // to a small integer screen scale, below the pearl rather than over it.
        var camera=Camera.main;
        float pixelsPerUnit=camera!=null?Mathf.Abs(camera.WorldToScreenPoint(position+Vector3.up).y-camera.WorldToScreenPoint(position).y):64;
        float height=7*Mathf.Max(1,Mathf.Floor(board.CellSize*pixelsPerUnit*.2f/7))/Mathf.Max(1,pixelsPerUnit);
        label.transform.localScale=Vector3.one*height/Mathf.Max(.001f,label.textBounds.size.y);
        label.transform.position=position+Vector3.down*board.CellSize*.36f;
    }
    private void AnimateAirReceipt(int opening,int spent,int[] gains)
    {
        int current=Mathf.Clamp(opening-spent,0,5);
        if(spent>0)airSteps.Enqueue((current,"-"+spent));
        foreach(int gain in gains) {current=Mathf.Clamp(current+gain,0,5);airSteps.Enqueue((current,"+"+gain));}
    }
    private void OnDestroy()
    {if(board!=null)board.AirReceipt-=AnimateAirReceipt;
     if(strip!=null)Destroy(strip.gameObject);if(water!=null)Destroy(water.gameObject);if(sceneWater!=null)Destroy(sceneWater.gameObject);
     BackgroundMusicPlayer.Instance?.SetUnderwaterMix(false);}
}

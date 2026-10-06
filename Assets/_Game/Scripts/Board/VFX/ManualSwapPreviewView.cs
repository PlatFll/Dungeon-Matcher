using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>Forecast only. The board constructs and validates the committed move.</summary>
public sealed class ManualSwapPreviewView : MonoBehaviour
{
    private BoardController board;
    private GameObject root;
    private Texture2D pixel;
    private Sprite solid;
    private readonly List<GameObject> pieces = new List<GameObject>();
    private void Awake()
    {
        board=GetComponent<BoardController>();
        pixel=new Texture2D(1,1,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};pixel.SetPixel(0,0,Color.white);pixel.Apply();
        solid=Sprite.Create(pixel,new Rect(0,0,1,1),Vector2.one*.5f,1);
        board.SwapPreviewChanged+=Show;
    }
    private void Show(BoardController.ManualSwapPlan plan)
    {
        if(root!=null) Destroy(root);pieces.Clear();
        if(plan==null) return;
        root=new GameObject("ManualSwapPreview");root.transform.SetParent(transform,false);
        float size=board.CellSize;
        for(int i=0;i<plan.Gems.Length;i++)
        {
            var gem=plan.Gems[i];var destination=plan.Destination(i);
            var position=board.GetCellLocalPosition(destination.x,destination.y);
            Make("DestinationBacking",solid,position,Vector3.one*size*(60f/64),new Color(0.04f,.07f,.09f,.94f),900);
            foreach(var source in gem.GetComponentsInChildren<SpriteRenderer>())
            {
                if(!source.enabled || source.sprite==null || !source.gameObject.activeInHierarchy) continue;
                var offset=transform.InverseTransformVector(source.transform.position-gem.transform.position);
                var scale=source.transform.lossyScale;var parent=transform.lossyScale;
                scale=new Vector3(scale.x/parent.x,scale.y/parent.y,1);
                var copy=Make("FinalGem_"+gem.BoardIdentity,source.sprite,position+offset,scale,Color.white,950+source.sortingOrder);
                copy.transform.rotation=source.transform.rotation;
            }
            // A white outline means the forecast is legal; an X also marks an
            // invalid result so legality never relies on red alone.
            Color color=plan.IsLegal?new Color32(232,237,227,255):new Color32(231,118,109,255);
            float edge=size*(28f/64), stroke=size/64;
            foreach(int sign in new[]{-1,1})
            {
                Make("ForecastEdge",solid,position+new Vector3(edge*sign,0),new Vector3(stroke,size*.9f,1),color,1200);
                Make("ForecastEdge",solid,position+new Vector3(0,edge*sign),new Vector3(size*.9f,stroke,1),color,1200);
            }
            if(!plan.IsLegal && i==0)
                foreach(int sign in new[]{-1,1})
                {
                    var cross=Make("InvalidForecast",solid,position,new Vector3(size*.4f,stroke*2,1),color,1201);
                    cross.transform.localRotation=Quaternion.Euler(0,0,sign*45);
                }
        }
        var from=plan.Cells[0];var to=plan.Cells[plan.Cells.Length-1];
        var arrow=Resources.Load<Sprite>("UI/PlayerStatuses/Slippery");
        if(arrow!=null)
        {
            var end=board.GetCellLocalPosition(to.x,to.y);
            var marker=Make("SlideDirection",arrow,end+new Vector3(0,-size*(18f/64)),Vector3.one*(size*.25f/arrow.bounds.size.x),Color.white,1300);
            marker.transform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(to.y-from.y,to.x-from.x)*Mathf.Rad2Deg);
        }
    }
    private SpriteRenderer Make(string name,Sprite sprite,Vector3 position,Vector3 scale,Color color,int order)
    {
        var go=new GameObject(name);go.transform.SetParent(root.transform,false);go.transform.localPosition=position;go.transform.localScale=scale;
        var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.color=color;renderer.sortingLayerName="Gems";renderer.sortingOrder=order;
        pieces.Add(go);return renderer;
    }
    private void LateUpdate()
    {
        var plan=board.SwapPreview;
        if(plan!=null && (!board.UsesExtraManualSwapStep || board.IsBusy || board.IsExternalInputBlocked || Time.timeScale<=0 ||
            plan.Gems.Where((g,i)=>g==null || board.GetGem(plan.Cells[i].x,plan.Cells[i].y)!=g || board.IsGemPinned(g)).Any()))
            board.ClearManualSwapPreview();
    }
    private void OnDestroy()
    {
        if(board!=null) board.SwapPreviewChanged-=Show;
        if(solid!=null) Destroy(solid);if(pixel!=null) Destroy(pixel);
    }
}

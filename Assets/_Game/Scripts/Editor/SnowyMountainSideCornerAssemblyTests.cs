using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    const string SnowPolish="ArtSource/SnowyMountainStaged/SideCornerPolish";

    // Review-only assembly of approved, unmodified native PNGs. No production binding.
    [UnityTest,Explicit] public IEnumerator SnowyMountainSideCornerStage04AssemblyReview()
    { yield return SnowyMountainAssembledReview(SnowPolish+"/Stage04",true,true); }

    [UnityTest,Explicit] public IEnumerator SnowyMountainSidePeekReview()
    { yield return SnowyMountainAssembledReview(SnowPolish+"/Stage04/SidePeek",true,true,true); }

    // Translate the existing structures only. The rejected second backing layer is removed.
    void SnowPolishMoveSides(Camera cam,Rect frame,float pixel,GameObject sides,GameObject lower)
    {
        var all=sides.GetComponentsInChildren<SpriteRenderer>().Concat(lower.GetComponentsInChildren<SpriteRenderer>()).ToArray();
        var originalSprites=all.Select(r=>r.sprite).ToArray();
        foreach(var sr in all)
        {
            bool move=sr.transform.IsChildOf(sides.transform)||sr.sprite.name.StartsWith("Connector_");
            if(!move)continue;
            int side=cam.WorldToScreenPoint(sr.bounds.center).x<Screen.width*.5f?0:1;
            float target=side==0?-5*pixel:Screen.width-39*pixel;
            float delta=target-SnowPolishSideX(frame,pixel,side);
            var old=cam.WorldToScreenPoint(sr.transform.position);
            sr.transform.position=SnowWorld(cam,new Vector2(old.x+delta,old.y));
            var now=cam.WorldToScreenPoint(sr.transform.position);
            Assert.That(now.y,Is.EqualTo(old.y).Within(.01),"Horizontal placement only");
        }
        var after=sides.GetComponentsInChildren<SpriteRenderer>().Concat(lower.GetComponentsInChildren<SpriteRenderer>()).ToArray();
        Assert.That(after,Is.EqualTo(all),"No added, removed or duplicated foreground pieces");
        Assert.That(after.Select(r=>r.sprite).ToArray(),Is.EqualTo(originalSprites),"Exact approved sprite references");
        Assert.That(after.Any(r=>r.sprite.name.StartsWith("SnowEdgeContinuation_")),Is.False,"No rejected backing layer");
    }

    GameObject SnowPolishSides(Camera cam,Rect frame,float pixel)
    {
        var root=new GameObject("SnowSideCornerStage04Sides_PreviewOnly");
        var sprites=Directory.GetFiles(SnowPolish+"/Stage02/Exports","*.png")
            .ToDictionary(Path.GetFileNameWithoutExtension,p=>SnowPreviewSprite(p,64));
        var lantern=SnowPreviewSprite(SnowStage06+"/Exports/Lantern.png",64);
        var banner=SnowPreviewSprite(SnowStage06+"/Exports/Banner.png",64);
        float top=Mathf.Floor(frame.yMax/pixel)*pixel,bottom=Mathf.Round(frame.yMin/pixel)*pixel;
        for(int side=0;side<2;side++)
        {
            string name=side==0?"Left":"Right";
            float x=SnowPolishSideX(frame,pixel,side);
            int n=0;
            for(float y=top-40*pixel;y>bottom+48*pixel;y-=40*pixel)
                SnowRevisionTile(root.transform,sprites[name+(n++%2==0?"_MiddleA":"_MiddleB")],cam,x,y,pixel,-82);
            SnowRevisionTile(root.transform,sprites[name+"_Upper"],cam,x,top,pixel,-80);
            SnowRevisionTile(root.transform,sprites[name+"_Foot"],cam,x,bottom+52*pixel,pixel,-79);
            // Retain the approved warm light and heraldry against the embedded upright.
            SnowRevisionTile(root.transform,lantern,cam,x+(side==0?2:19)*pixel,top-43*pixel,pixel,-76);
            SnowRevisionTile(root.transform,banner,cam,x,top-103*pixel,pixel,-75);
        }
        return root;
    }

    static float SnowPolishSideX(Rect frame,float pixel,int side)
    {
        float native=side==0?Mathf.Floor(frame.xMin/pixel)+13-44:Mathf.Ceil(frame.xMax/pixel)-13;
        return Mathf.Clamp(native,0,Mathf.Floor(Screen.width/pixel)-44)*pixel;
    }

    GameObject SnowPolishCorners(Camera cam,Rect frame,float pixel)
    {
        var root=new GameObject("SnowSideCornerStage04Corners_PreviewOnly");
        var sprites=Directory.GetFiles(SnowPolish+"/Stage03/Exports","*.png")
            .ToDictionary(Path.GetFileNameWithoutExtension,p=>SnowPreviewSprite(p,64));
        for(int side=0;side<2;side++)
        {
            string name=side==0?"Left":"Right";
            float x=SnowPolishSideX(frame,pixel,side);
            float coreTop=(side==0?76:88)*pixel;
            // Whole connectors bridge extra height; the core covers their lower join.
            for(float y=Mathf.Round(frame.yMin/pixel)*pixel+16*pixel;y>coreTop-10*pixel;y-=40*pixel)
                SnowRevisionTile(root.transform,sprites["Connector_"+name],cam,x,y,pixel,-83);
            float coreX=side==0?-6*pixel:Screen.width-104*pixel;
            SnowRevisionTile(root.transform,sprites["Corner_"+name],cam,coreX,coreTop,pixel,-77);
            // Aprons extend only the grounded foot, behind the core and all shared UI.
            float apronX=side==0?0:Screen.width-64*pixel;
            SnowRevisionTile(root.transform,sprites["SnowApron_"+name],cam,apronX,22*pixel,pixel,-78);
        }
        return root;
    }

    void SnowPolishGlints(SnowAtmosphereReview atmosphere,Camera cam,float pixel,GameObject sides,GameObject lower)
    {
        var glints=atmosphere.details.Where(d=>d.glint).ToArray();
        var anchors=sides.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.sprite.name.EndsWith("_Foot"))
            .Concat(lower.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.sprite.name=="Corner_Left")).ToArray();
        Assert.That(glints.Length,Is.EqualTo(anchors.Length));
        for(int i=0;i<glints.Length;i++)
        {
            var b=anchors[i].bounds;var lo=cam.WorldToScreenPoint(b.min);var hi=cam.WorldToScreenPoint(b.max);
            float x=lo.x+(i==2?48:18)*pixel,y=hi.y-(i==2?62:30)*pixel;
            glints[i].renderer.transform.position=SnowWorld(cam,new Vector2(Mathf.Round(x/pixel)*pixel,Mathf.Round(y/pixel)*pixel));
        }
    }

    void SnowPolishValidate(Camera cam,Rect frame,float pixel,bool sidePeek,params GameObject[] roots)
    {
        var opening=Rect.MinMaxRect(frame.xMin+13*pixel,frame.yMin+13*pixel,frame.xMax-13*pixel,frame.yMax-13*pixel);
        foreach(var root in roots)
        {
            Assert.That(root.GetComponentsInChildren<UnityEngine.EventSystems.UIBehaviour>(),Is.Empty,"Scenery cannot capture input");
            foreach(var sr in root.GetComponentsInChildren<SpriteRenderer>())
            {
                var lo=cam.WorldToScreenPoint(sr.bounds.min);var hi=cam.WorldToScreenPoint(sr.bounds.max);
                Assert.That((hi.x-lo.x)/sr.sprite.rect.width,Is.EqualTo(pixel).Within(.01),sr.name+" native X");
                Assert.That((hi.y-lo.y)/sr.sprite.rect.height,Is.EqualTo(pixel).Within(.01),sr.name+" native Y");
                Assert.That(sr.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
                Assert.That(sr.color,Is.EqualTo(UnityEngine.Color.white),"No recoloring");
                Assert.That(hi.y,Is.LessThanOrEqualTo(frame.yMax+.01),"Upper scene is locked");
                bool shiftedSide=root.name=="SnowSideCornerStage04Sides_PreviewOnly"||sr.sprite.name.StartsWith("Connector_");
                float clipping=sidePeek&&shiftedSide?5*pixel:0;
                var texture=sr.sprite.texture;var colors=texture.GetPixels32();
                for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)
                {
                    var alpha=colors[y*texture.width+x].a;
                    Assert.That(alpha==0||alpha==255,Is.True,"Binary native artwork");if(alpha==0)continue;
                    float px=lo.x+(x+.5f)*pixel,py=lo.y+(y+.5f)*pixel;
                    Assert.That(px,Is.InRange(-clipping,Screen.width+clipping),sr.name+" requested slight outer-edge clipping only");
                    Assert.That(py,Is.InRange(0f,(float)Screen.height),sr.name+" full opaque height");
                    Assert.That(opening.Contains(new Vector2(px,py)),Is.False,sr.name+" stays outside gems");
                }
            }
        }
    }
}

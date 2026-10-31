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

    [UnityTest,Explicit] public IEnumerator SnowyMountainSideCornerEdgeReview()
    { yield return SnowyMountainAssembledReview(SnowPolish+"/Stage04/EdgeRevision",true,true,true); }

    // Source-rectangle views reuse the approved pixels, beneath the existing silhouettes.
    // Only these architectural continuation courses intentionally extend past the viewport.
    void SnowPolishEdgeContinuation(Camera cam,Rect frame,float pixel,GameObject sides)
    {
        for(int side=0;side<2;side++)
        {
            var source=SnowPreviewSprite(SnowPolish+"/Stage02/Exports/"+(side==0?"Left":"Right")+"_MiddleA.png",64);
            int start=side==0?-3:Mathf.CeilToInt(frame.xMax/pixel)-10;
            int end=side==0?Mathf.FloorToInt(frame.xMin/pixel)+10:Mathf.CeilToInt(Screen.width/pixel)+3;
            int sourceX=side==0?9:14,courseWidth=side==0?22:20;
            int ceiling=Mathf.FloorToInt(frame.yMax/pixel);
            for(int top=ceiling;top>0;top-=40)
            for(int x=start;x<end;x+=courseWidth)
            {
                int height=Mathf.Min(40,top),width=Mathf.Min(courseWidth,end-x);
                var rect=new Rect(sourceX,source.texture.height-10-height,width,height);
                var course=Sprite.Create(source.texture,rect,Vector2.one*.5f,64,0,SpriteMeshType.FullRect);
                course.name="SnowEdgeContinuation_"+(side==0?"Left":"Right");snowPortraitTemporary.Add(course);
                SnowRevisionTile(sides.transform,course,cam,x*pixel,top*pixel,pixel,-84);
            }
        }
        // Inspect real source alpha at both physical screen edges, every scanline.
        var edge=sides.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.sprite.name.StartsWith("SnowEdgeContinuation_")).ToArray();
        int yMax=Mathf.FloorToInt(frame.yMax/pixel)*(int)pixel;
        foreach(int x in new[]{0,Screen.width-1})for(int y=0;y<yMax;y++)
        {
            bool covered=edge.Any(sr=>
            {
                var lo=cam.WorldToScreenPoint(sr.bounds.min);var hi=cam.WorldToScreenPoint(sr.bounds.max);
                if(x+.5f<lo.x||x+.5f>=hi.x||y+.5f<lo.y||y+.5f>=hi.y)return false;
                int sx=Mathf.FloorToInt((x+.5f-lo.x)/pixel+sr.sprite.rect.x);
                int sy=Mathf.FloorToInt((y+.5f-lo.y)/pixel+sr.sprite.rect.y);
                return sr.sprite.texture.GetPixel(sx,sy).a==1;
            });
            Assert.That(covered,Is.True,"Continuous snowy structure at physical edge "+x+","+y);
        }
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

    void SnowPolishValidate(Camera cam,Rect frame,float pixel,params GameObject[] roots)
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
                var texture=sr.sprite.texture;var colors=texture.GetPixels32();var source=sr.sprite.rect;
                bool edge=sr.sprite.name.StartsWith("SnowEdgeContinuation_");
                for(int y=0;y<(int)source.height;y++)for(int x=0;x<(int)source.width;x++)
                {
                    var alpha=colors[(y+(int)source.y)*texture.width+x+(int)source.x].a;
                    Assert.That(alpha==0||alpha==255,Is.True,"Binary native artwork");if(alpha==0)continue;
                    float px=lo.x+(x+.5f)*pixel,py=lo.y+(y+.5f)*pixel;
                    Assert.That(px,Is.InRange(edge?-3*pixel:0f,Screen.width+(edge?3*pixel:0)),sr.name+" authorized edge overscan only");
                    Assert.That(py,Is.InRange(0f,(float)Screen.height),sr.name+" full opaque height");
                    Assert.That(opening.Contains(new Vector2(px,py)),Is.False,sr.name+" stays outside gems");
                }
            }
        }
    }
}

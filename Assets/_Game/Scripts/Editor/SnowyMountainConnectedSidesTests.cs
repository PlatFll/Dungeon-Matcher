using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    const string SnowConnected="ArtSource/SnowyMountainStaged/SideCornerPolish/Stage04/ConnectedSides";

    [UnityTest,Explicit] public IEnumerator SnowyMountainConnectedSidesReview()
    { yield return SnowyMountainAssembledReview(SnowConnected,true,true,true,true); }

    const string SnowBannerMounts="ArtSource/SnowyMountainStaged/SideCornerPolish/Stage04/BannerMounts";

    [UnityTest,Explicit] public IEnumerator SnowyMountainMountedBannersReview()
    { yield return SnowyMountainAssembledReview(SnowBannerMounts,true,true,true,true,true); }

    void SnowMountBanners(Camera cam,Rect frame,float pixel,GameObject root)
    {
        var all=root.GetComponentsInChildren<SpriteRenderer>();
        var banners=all.Where(r=>r.sprite.name=="Banner").OrderBy(r=>r.transform.position.x).ToArray();
        Assert.That(banners.Length,Is.EqualTo(2));
        float top=Mathf.Floor(frame.yMax/pixel)*pixel;
        for(int side=0;side<2;side++)
        {
            bool left=side==0;string name=left?"Left":"Right";
            var structure=all.Single(r=>r.sprite.name.StartsWith(name+"_"));
            float gap=left?frame.xMin/pixel:(Screen.width-frame.xMax)/pixel;
            float shaft=Mathf.Ceil(gap)+8;
            float anchorX=left?(shaft-11)*pixel:Screen.width+(13-shaft)*pixel;
            float clothX=anchorX+(left?-27:-15)*pixel;
            float x=clothX-4*pixel,flagTop=top-87*pixel; // Clear the brace seam; bolt into the upright.
            var mounted=SnowPreviewSprite(SnowBannerMounts+"/Exports/Banner_"+name+"_Mounted.png",64);
            var banner=banners[side];banner.sprite=mounted;
            banner.transform.position=SnowWorld(cam,new Vector2(x+mounted.rect.width*pixel*.5f,flagTop-mounted.rect.height*pixel*.5f));
            float plateX=x+(left?31:19)*pixel;
            Assert.That(plateX,Is.EqualTo(anchorX).Within(.01),"Bolted plate aligns to the actual side upright");
            var lo=cam.WorldToScreenPoint(structure.bounds.min);
            int tx=Mathf.FloorToInt((anchorX+.5f*pixel-lo.x)/pixel);
            int ty=Mathf.FloorToInt((flagTop-10.5f*pixel-lo.y)/pixel);
            var under=(Color32)structure.sprite.texture.GetPixel(tx,ty);
            Assert.That(under.a,Is.EqualTo(255),"Mount cannot float over transparent scenery");
            // Existing brown timber or its gray iron band must be under the bolt.
            bool timber=under.r>under.g&&under.g>=under.b;
            bool iron=under.r==85&&under.g==80&&under.b==81;
            Assert.That(timber||iron,Is.True,name+" mount sits on the timber/iron upright, not the cliff face: "+under);
            var old=SnowPreviewSprite(SnowStage06+"/Exports/Banner.png",64).texture;
            for(int sy=0;sy<old.height;sy++)for(int sx=0;sx<old.width;sx++)
            {
                var c=old.GetPixel(sx,sy);if(c.a==0)continue;
                Assert.That(mounted.texture.GetPixel(sx+4,sy),Is.EqualTo(c),"Original cloth, emblem and pole pixels unchanged");
            }
        }
    }

    GameObject SnowConnectedSides(Camera cam,Rect frame,float pixel,string key)
    {
        var root=new GameObject("SnowConnectedSides_PreviewOnly");
        float top=Mathf.Floor(frame.yMax/pixel)*pixel;
        var lantern=SnowPreviewSprite(SnowStage06+"/Exports/Lantern.png",64);
        var banner=SnowPreviewSprite(SnowStage06+"/Exports/Banner.png",64);
        for(int side=0;side<2;side++)
        {
            string name=side==0?"Left":"Right";
            var sprite=SnowPreviewSprite(SnowConnected+"/Exports/"+name+"_"+key+".png",64);
            float x=side==0?-4*pixel:Screen.width-104*pixel;
            SnowRevisionTile(root.transform,sprite,cam,x,top,pixel,-80);
            // The approved lamps and banners remain separate decoration, not
            // additional rock layers. Keep their previous screen-edge positions.
            float oldX=side==0?-5*pixel:Screen.width-39*pixel;
            SnowRevisionTile(root.transform,lantern,cam,oldX+(side==0?2:19)*pixel,top-43*pixel,pixel,-76);
            SnowRevisionTile(root.transform,banner,cam,oldX,top-103*pixel,pixel,-75);
        }
        return root;
    }

    void SnowConnectedValidate(Camera cam,Rect frame,float pixel,GameObject root)
    {
        var all=root.GetComponentsInChildren<SpriteRenderer>();
        var structures=all.Where(r=>r.sprite.name.StartsWith("Left_")||r.sprite.name.StartsWith("Right_")).ToArray();
        Assert.That(structures.Length,Is.EqualTo(2),"Exactly one rock/timber/corner asset per side");
        Assert.That(all.Length,Is.EqualTo(6),"Two structures plus retained two lanterns and two banners only");
        Assert.That(root.GetComponentsInChildren<UnityEngine.EventSystems.UIBehaviour>(),Is.Empty,"Scenery cannot intercept input");
        var opening=Rect.MinMaxRect(frame.xMin+13*pixel,frame.yMin+13*pixel,frame.xMax-13*pixel,frame.yMax-13*pixel);
        foreach(var sr in all)
        {
            var lo=cam.WorldToScreenPoint(sr.bounds.min);var hi=cam.WorldToScreenPoint(sr.bounds.max);
            Assert.That((hi.x-lo.x)/sr.sprite.rect.width,Is.EqualTo(pixel).Within(.02));
            Assert.That((hi.y-lo.y)/sr.sprite.rect.height,Is.EqualTo(pixel).Within(.02));
            Assert.That(sr.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
            Assert.That(sr.color,Is.EqualTo(UnityEngine.Color.white));
            Assert.That(hi.y,Is.LessThanOrEqualTo(frame.yMax+.02));
            var pixels=sr.sprite.texture.GetPixels32();int width=sr.sprite.texture.width;
            for(int y=0;y<sr.sprite.texture.height;y++)for(int x=0;x<width;x++)
            {
                byte alpha=pixels[y*width+x].a;Assert.That(alpha==0||alpha==255,Is.True);if(alpha==0)continue;
                var point=new Vector2(lo.x+(x+.5f)*pixel,lo.y+(y+.5f)*pixel);
                Assert.That(opening.Contains(point),Is.False,"No decoration inside the playable gem opening");
                Assert.That(point.y,Is.InRange(0f,(float)Screen.height));
                Assert.That(point.x,Is.InRange(-5*pixel,Screen.width+5*pixel),"Only intentional outer-edge overscan");
            }
        }
        // Check actual native alpha at both physical edges and at the frame
        // joins for every board-height scanline. Backing cannot conceal a gap.
        foreach(var sr in structures)
        {
            bool left=sr.sprite.name.StartsWith("Left_");
            var lo=cam.WorldToScreenPoint(sr.bounds.min);
            float edge=left?.5f:Screen.width-.5f;
            float join=left?frame.xMin+.5f:frame.xMax-.5f;
            int edgeX=Mathf.FloorToInt((edge-lo.x)/pixel),joinX=Mathf.FloorToInt((join-lo.x)/pixel);
            for(int y=0;y<sr.sprite.texture.height;y++)
            {
                Assert.That(sr.sprite.texture.GetPixel(edgeX,y).a,Is.EqualTo(1),"Structure reaches physical screen edge on every scanline");
                float screenY=lo.y+(y+.5f)*pixel;
                if(screenY>=frame.yMin)Assert.That(sr.sprite.texture.GetPixel(joinX,y).a,Is.EqualTo(1),"Continuous structure joins board frame without a gutter");
            }
        }
    }
}

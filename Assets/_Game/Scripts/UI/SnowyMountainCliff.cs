using UnityEngine;

/// <summary>Replays the approved LibreSprite native assembly at the current gutter/height.
/// No resampling: copied rock planes join along matching material boundaries, with one crown and foot.</summary>
public static class SnowyMountainCliff
{
    sealed class Pixels
    {
        public int w,h; public Color32[] p;
        public Pixels(int width,int height){w=width;h=height;p=new Color32[w*h];}
        public Color32 Get(int x,int y)=>x>=0&&y>=0&&x<w&&y<h?p[y*w+x]:default;
        public void Put(int x,int y,Color32 c){if(x>=0&&y>=0&&x<w&&y<h)p[y*w+x]=c;}
    }
    static Color32 C(int r,int g,int b)=>new Color32((byte)r,(byte)g,(byte)b,255);
    static readonly Color32 shade=C(53,82,108),stone=C(87,121,147),snow=C(236,250,253),blue=C(184,217,232);
    static Pixels Read(Sprite s){var t=s.texture;var a=new Pixels(t.width,t.height);var p=t.GetPixels32();for(int y=0;y<a.h;y++)System.Array.Copy(p,(a.h-1-y)*a.w,a.p,y*a.w,a.w);return a;}
    static void Rect(Pixels a,int x,int y,int w,int h,Color32 c){for(int j=y;j<y+h;j++)for(int i=x;i<x+w;i++)a.Put(i,j,c);}
    static void Polygon(Pixels a,Vector2Int[] points,Color32 c)
    {
        for(int y=0;y<a.h;y++)for(int x=0;x<a.w;x++)
        {bool inside=false;for(int i=0,j=points.Length-1;i<points.Length;j=i++)
        {var u=points[i];var v=points[j];if((u.y>y)!=(v.y>y)&&x<(float)(v.x-u.x)*(y-u.y)/(v.y-u.y)+u.x)inside=!inside;}
        if(inside)a.Put(x,y,c);}
    }
    static void Paste(Pixels a,Pixels b,int x0,int y0){for(int y=0;y<b.h;y++)for(int x=0;x<b.w;x++){var c=b.Get(x,y);if(c.a>0)a.Put(x+x0,y+y0,c);}}
    public static Sprite Build(Sprite crown,Sprite corner,bool left,int height,int shaft)
    {
        // Grow the canvas on unusually wide safe gutters; do not stretch the artwork.
        int width=Mathf.Max(108,shaft+8),H=Mathf.Max(height,1),g=shaft,x0=left?0:width-g;
        var a=new Pixels(width,H);var src=Read(crown);
        Rect(a,x0,0,g,H,shade);Rect(a,x0,0,g,12,snow);Rect(a,x0,12,g,5,blue);
        int ox=left?g-44:x0;
        Color32 Sample(int x,int y)
        {
            var c=src.Get(x-ox,28+y);if(c.a>0)return c;
            int nearest=left?3:39,step=left?1:-1;
            for(int k=0;k<36;k++,nearest+=step){c=src.Get(nearest,28+y);if(c.a>0)return c;}return shade;
        }
        const int overlap=18,len=76;
        for(int top=22,segment=0;top<H;top+=len-overlap,segment++)
        {
            var seam=new int[g];var cost=new int[g,overlap];var prev=new int[g,overlap];
            if(segment>0)
            {
                for(int x=0;x<g;x++)for(int y=0;y<overlap;y++)
                {
                    var c=a.Get(x0+x,top+y);var d=Sample(x0+x,y);
                    int v=Mathf.Abs(c.r-d.r)+Mathf.Abs(c.g-d.g)+Mathf.Abs(c.b-d.b),best=int.MaxValue,by=y;
                    if(x>0)for(int q=Mathf.Max(0,y-1);q<=Mathf.Min(overlap-1,y+1);q++)if(cost[x-1,q]<best){best=cost[x-1,q];by=q;}
                    cost[x,y]=v+(x>0?best:0);prev[x,y]=by;
                }
                int last=0;for(int y=1;y<overlap;y++)if(cost[g-1,y]<cost[g-1,last])last=y;
                for(int x=g-1;x>=0;x--){seam[x]=last;last=prev[x,last];}
            }
            for(int x=0;x<g;x++)for(int y=segment>0?seam[x]:0;y<len&&top+y<H;y++)a.Put(x0+x,top+y,Sample(x0+x,y));
        }
        for(int y=0;y<27;y++)for(int x=0;x<src.w;x++){var c=src.Get(x,y);if(c.a>0)a.Put(ox+x,y,c);}
        if(left)
        {
            Polygon(a,new[]{new Vector2Int(0,4),new Vector2Int(Mathf.Max(4,g-26),3),new Vector2Int(Mathf.Max(6,g-20),8),new Vector2Int(0,17)},snow);
            Polygon(a,new[]{new Vector2Int(0,17),new Vector2Int(Mathf.Max(6,g-20),8),new Vector2Int(Mathf.Max(4,g-22),12),new Vector2Int(0,22)},blue);
        }
        else{Rect(a,width-5,10,5,3,snow);Rect(a,width-5,13,5,2,blue);}
        Paste(a,Read(corner),left?-2:width-108,H-(left?76:88));
        for(int y=0;y<H;y++)for(int x=left?0:width-4;x<(left?5:width);x++)if(a.Get(x,y).a==0)a.Put(x,y,y>H-6?blue:left?stone:shade);
        var tex=new Texture2D(width,H,TextureFormat.RGBA32,false){name=(left?"Left":"Right")+"_ConnectedCliff",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
        var output=new Color32[width*H];for(int y=0;y<H;y++)System.Array.Copy(a.p,y*width,output,(H-1-y)*width,width);
        tex.SetPixels32(output);tex.Apply();
        var result=Sprite.Create(tex,new Rect(0,0,width,H),Vector2.one*.5f,64,0,SpriteMeshType.FullRect);result.name=tex.name;return result;
    }
}

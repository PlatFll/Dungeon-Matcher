using UnityEngine;
using UnityEngine.UI;

/// <summary>Fits one registered scenery module inside an existing layout owner.
/// Source pixels stay Point-filtered; both physical edges land on whole pixels.
/// Used by scenery and the existing top-control owners; it never positions
/// gameplay sections, board cells, or characters.</summary>
[DefaultExecutionOrder(220)]
[RequireComponent(typeof(Image))]
public sealed class ReferenceKitSurface : MonoBehaviour
{
    public Rect Source { get; private set; }
    public Vector2 Extent { get; private set; }
    public Rect ExpectedScreenRect
    {
        get
        {
            var bounds=GameplayPixelLayoutController.ScreenRect((RectTransform)transform.parent);
            return Rect.MinMaxRect(Mathf.Round(bounds.xMin+bounds.width*Source.xMin/Extent.x),
                Mathf.Round(bounds.yMax-bounds.height*Source.yMax/Extent.y),
                Mathf.Round(bounds.xMin+bounds.width*Source.xMax/Extent.x),
                Mathf.Round(bounds.yMax-bounds.height*Source.yMin/Extent.y));
        }
    }
    public void Initialize(Rect source,Vector2 extent){Source=source;Extent=extent;Refresh();}
    private void LateUpdate()=>Refresh();
    public void Refresh()
    {
        if(Extent.x<=0 || Extent.y<=0 || transform.parent is not RectTransform parent)return;
        var bounds=GameplayPixelLayoutController.ScreenRect(parent);
        var canvas=GetComponentInParent<Canvas>().rootCanvas;
        float scale=canvas.scaleFactor;
        Rect expected=ExpectedScreenRect;
        GameplayPixelLayoutController.SetRect((RectTransform)transform,
            new Rect((expected.xMin-bounds.xMin)/scale,(expected.yMin-bounds.yMin)/scale,expected.width/scale,expected.height/scale));
    }
}

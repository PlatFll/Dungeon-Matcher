using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Compresses the painted face while keeping the touch rectangle and its children in place.</summary>
[DisallowMultipleComponent]
public sealed class MenuButtonFeedback : BaseMeshEffect, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, ISubmitHandler
{
    private Button button;
    private MenuCue click;
    private bool held;
    private float face, submitUntil;
    private int pointer;

    public static void Attach(Button target, MenuCue sound = MenuCue.Press)
    {
        if (target.gameObject.scene.name != "MainMenu") return;
        if (!target.TryGetComponent<MenuButtonFeedback>(out var feedback))
        {
            feedback = target.gameObject.AddComponent<MenuButtonFeedback>();
            feedback.button = target;
            target.onClick.AddListener(feedback.OnClick);
        }
        feedback.click = sound;
    }

    private void OnClick() => MenuSoundPlayer.Play(click);

    public void OnPointerDown(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left || !button.IsInteractable() || held) return;
        held = true;
        pointer = e.pointerId;
        SetFace(1);
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (e.pointerId == pointer) held = false;
    }

    public void OnPointerExit(PointerEventData e) => OnPointerUp(e);

    public void OnSubmit(BaseEventData e)
    {
        if (button.IsInteractable())
        {
            submitUntil = Time.unscaledTime + .05f;
            SetFace(1);
        }
    }

    private void LateUpdate()
    {
        if (face == 0) return;
        if (!button.IsInteractable()) held = false;
        if (!held && Time.unscaledTime >= submitUntil)
            SetFace(Mathf.MoveTowards(face, 0, Time.unscaledDeltaTime / .075f));
    }

    private void SetFace(float amount)
    {
        face = amount;
        graphic.SetVerticesDirty();
    }

    public override void ModifyMesh(VertexHelper mesh)
    {
        if (!IsActive() || face <= 0) return;
        Rect bounds = graphic.rectTransform.rect;
        var vertex = new UIVertex();
        bool move = !PresentationPreferences.ReducedMotion;
        for (int i = 0; i < mesh.currentVertCount; i++)
        {
            mesh.PopulateUIVertex(ref vertex, i);
            float height = Mathf.InverseLerp(bounds.yMin, bounds.yMax, vertex.position.y);
            if (move) vertex.position.y -= height * face;
            Color32 c = vertex.color;
            float shade = 1 - face * (.06f + .08f * height);
            vertex.color = new Color32((byte)(c.r * shade), (byte)(c.g * shade), (byte)(c.b * shade), c.a);
            mesh.SetUIVertex(vertex, i);
        }
    }

    protected override void OnDisable()
    {
        held = false;
        face = submitUntil = 0;
        base.OnDisable();
    }
}

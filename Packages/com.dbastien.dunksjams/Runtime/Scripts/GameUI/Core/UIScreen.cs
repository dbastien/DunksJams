using UnityEngine;

public abstract class UIScreen
{
    protected GameObject Panel { get; private set; }
    protected Transform Canvas { get; private set; }

    public bool IsActive => Panel != null && Panel.activeSelf;

    protected UIScreen(Transform canvas)
    {
        Canvas = canvas;
        Panel = UIBuilder.CreatePanel(canvas, GetType().Name);
        Panel.SetActive(false);
    }

    public abstract void Setup();

    public virtual void Show()
    {
        Panel.SetActive(true);
        OnShow();
    }

    public virtual void Hide()
    {
        OnHide();
        Panel.SetActive(false);
    }

    public virtual void Destroy()
    {
        if (Panel != null)
        {
            Object.Destroy(Panel);
            Panel = null;
        }
    }

    protected virtual void OnShow()
    {
    }

    protected virtual void OnHide()
    {
    }

    // Use TransformExtensions.DestroyChildren(this Transform) instead of duplicating logic here.
}
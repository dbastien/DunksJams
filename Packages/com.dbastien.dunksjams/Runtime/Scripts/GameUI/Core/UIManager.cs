using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class UIManager : SingletonEagerBehaviour<UIManager>
{
    [SerializeField] RenderMode canvasRenderMode = RenderMode.ScreenSpaceOverlay;

    GameObject canvas;
    readonly Dictionary<Type, UIScreen> screens = new();
    readonly Stack<UIScreen> screenStack = new();

    protected override void InitInternal()
    {
        canvas = UIBuilder.CreateCanvas(canvasRenderMode);
        if (PersistAcrossScenes) DontDestroyOnLoad(canvas);
    }

    public T RegisterScreen<T>() where T : UIScreen
    {
        var screenType = typeof(T);
        if (screens.ContainsKey(screenType))
        {
            DLog.LogW($"Screen {screenType.Name} already registered");
            return screens[screenType] as T;
        }

        var screen = (T)Activator.CreateInstance(typeof(T), canvas.transform);
        screen.Setup();
        screens[screenType] = screen;
        return screen;
    }

    public void ShowScreen<T>(bool addToStack = true) where T : UIScreen
    {
        var screenType = typeof(T);
        if (!screens.TryGetValue(screenType, out var screen))
        {
            DLog.LogW($"Screen {screenType.Name} not registered");
            return;
        }

        if (screenStack.Count > 0 && addToStack)
            screenStack.Peek().Hide();

        screen.Show();

        if (addToStack)
            screenStack.Push(screen);
    }

    public void HideCurrentScreen()
    {
        if (screenStack.Count == 0) return;
        var screen = screenStack.Pop();
        screen.Hide();

        if (screenStack.Count > 0)
            screenStack.Peek().Show();
    }

    public void HideAllScreens()
    {
        while (screenStack.Count > 0)
            screenStack.Pop().Hide();
    }

    public T GetScreen<T>() where T : UIScreen
    {
        screens.TryGetValue(typeof(T), out var screen);
        return screen as T;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        foreach (var screen in screens.Values)
            screen.Destroy();
        screens.Clear();
    }
}
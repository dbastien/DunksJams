#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using static Tabify;
using static EditorGUIUtil;
using static ColorExtensions;
using static ReflectionUtils;
using Object = UnityEngine.Object;

public class TabifyGUI
{
    public void TabStripGUI(Rect stripRect)
    {
        interactiveRects.Clear();

        if (curEvent.isLayout())
            UpdateState();

        if (TabifyMenu.addTabButtonEnabled)
        {
            Rect buttonRect1 = stripRect.SetX(tabEndPositions.Last()).
                SetWidth(0).
                SetSizeFromMid(24).
                MoveX(TabifyMenu.neatTabStyleEnabled ? 12 : 13);

            float distToRight = stripRect.xMax - buttonRect1.xMax;

            if (distToRight >= 10)
            {
                interactiveRects.Add(item: buttonRect1);

                var fadeStart = 10;
                var fadeEnd = 25;
                float fadeK = ((distToRight - fadeStart) / (fadeEnd - fadeStart)).Clamp01().Pow(2);

                string iconName1 = stripRect.IsHovered() && curEvent.holdingAlt() && tabInfosForReopening.Any()
                    ? "d_Refresh"
                    : "d_Toolbar Plus";
                var iconSize1 = 16;
                Color colorNormal1 = Greyscale(isDarkTheme ? .5f : .47f, alpha: fadeK);
                Color colorHovered1 = Greyscale(isDarkTheme ? 1f : .1f);
                Color colorPressed1 = Greyscale(isDarkTheme ? .75f : .5f);

                if (TabifyAddTabWindow.instance && TabifyAddTabWindow.instance.dockArea == dockArea)
                    colorNormal1 = colorHovered1;

                if (DragAndDrop.objectReferences.Any())
                    colorHovered1 = colorNormal1;

                if (IconButton(rect: buttonRect1, iconName: iconName1, iconSize: iconSize1, colorNormal: colorNormal1,
                        colorHovered: colorHovered1, colorPressed: colorPressed1))
                {
                    if (curEvent.holdingAlt())
                    {
                        if (tabInfosForReopening.Any())
                            ReopenClosedTab();
                    }
                    else
                    {
                        if (TabifyAddTabWindow.instance)
                            TabifyAddTabWindow.instance.Close();
                        else
                            TabifyAddTabWindow.Open(dockArea: dockArea);
                    }
                }
            }
        }

        if (TabifyMenu.closeTabButtonEnabled && (tabs.Count != 1 || curEvent.holdingAlt()))
        {
            isCloseButtonHovered = false;

            if (hoveredTab != null && hoveredTab != hideCloseButtonOnTab)
            {
                Rect buttonRect = stripRect.SetX(tabEndPositions[index: hoveredTabIndex]).
                    SetWidth(0).
                    SetSizeFromMid(12).
                    MoveX(TabifyMenu.largeTabStyleEnabled ? -16 : -14);

                if (buttonRect.xMax <= stripRect.xMax - 10)
                {
                    interactiveRects.Add(item: buttonRect);

                    isCloseButtonHovered = buttonRect.IsHovered();

                    // Draw simple X button
                    bool isPressed = isCloseButtonHovered && curEvent.isMouseDown();
                    Color buttonColor = isPressed
                        ? Greyscale(isDarkTheme ? .75f : .5f)
                        : isCloseButtonHovered
                            ? Greyscale(isDarkTheme ? 1f : .0f)
                            : Greyscale(isDarkTheme ? .55f : .35f);

                    SetGUIColor(c: buttonColor);
                    var style = new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 16,
                        fontStyle = FontStyle.Bold
                    };
                    GUI.Label(buttonRect, "×", style);
                    ResetGUIColor();

                    if (isCloseButtonHovered && curEvent.isMouseUp())
                    {
                        void closeNextUpdate()
                        {
                            CloseTab(tab: hoveredTab);
                            EditorApplication.update -= closeNextUpdate;
                        }

                        if (tabs.Count == 1)
                            EditorApplication.update +=
                                closeNextUpdate; // prevents error on dockarea destruction
                        else
                            CloseTab(tab: hoveredTab);
                    }
                }
            }
        }

        tabStripElement.pickingMode = interactiveRects.Any(r => r.IsHovered())
            ? PickingMode.Position
            : PickingMode.Ignore;
    }

    private readonly List<Rect> interactiveRects = new();

    private bool isCloseButtonHovered;

    public void UpdateState()
    {
        scrollPos = dockArea.GetFieldValue<float>("m_ScrollOffset");
        if (scrollPos != 0)
            scrollPos -= nonZeroTabScrollOffset;

        tabEndPositions.Clear();

        float curPos = -scrollPos +
                       dockArea.GetMemberValue<Rect>("m_TabAreaRect").x *
                       2 // internally this offset is erroneously applied twice
                       -
                       2;

        foreach (EditorWindow tab in tabs)
        {
            curPos += GetTabWidth(tab: tab);

            tabEndPositions.Add(curPos.
                Round()); // internally tabs are drawn using plain round(), not roundToPixelGrid()
        }

        hoveredTab = null;
        hoveredTabIndex = -1;

        if (!tabStripElement.contentRect.IsHovered()) return;

        for (int i = tabs.Count - 1; i >= 0; i--)
            if (curEvent.mousePosition.x < tabEndPositions[index: i])
                hoveredTabIndex = i;

        if (hoveredTabIndex.IsInRangeOf(list: tabs))
            hoveredTab = tabs[index: hoveredTabIndex];
    }

    private float scrollPos;

    public List<float> tabEndPositions = new();

    private int hoveredTabIndex;

    private EditorWindow hoveredTab;

    private void DelayCallRepaintLoop()
    {
        if (!activeTab) return; // happens when maximized

        isTabStripHovered = tabStripElement.contentRect.Move(v: activeTab.position.position).
            Contains(curEvent.mousePosition_screenSpace());

        if (isTabStripHovered)
            activeTab.Repaint();

        EditorApplication.delayCall += DelayCallRepaintLoop;

        // needed because dockarea can fail to repaint when mouse enters/leaves interactive regions (buttons, tabs)
        // seems to only happen in unity 6 when active tab is uitk based
    }

    private bool isTabStripHovered;

    private void HandleTabScrolling(EventBase e)
    {
        if (e is MouseMoveEvent)
        {
            sidescrollPosition = 0;
            return;
        }

        if (e is not WheelEvent scrollEvent) return;

        if (TabifyMenu.switchTabShortcutEnabled)
        {
            if (scrollEvent.modifiers == EventModifiers.Shift ||
                scrollEvent.modifiers == (EventModifiers.Shift | EventModifiers.Control) ||
                scrollEvent.modifiers == (EventModifiers.Shift | EventModifiers.Command))
            {
                float scrollDelta1 = Application.platform == RuntimePlatform.OSXEditor
                    ? scrollEvent.delta.x // osx sends delta.y as delta.x when shift is pressed
                    : scrollEvent.delta.x -
                      scrollEvent.delta.y; // some software on windows (eg logitech options) may do that too
                if (TabifyMenu.reverseScrollDirectionEnabled)
                    scrollDelta1 *= -1;

                if (scrollDelta1 != 0)
                {
                    e.StopPropagation();

                    if (scrollEvent.ctrlKey || scrollEvent.commandKey)
                    {
                        int dir = scrollDelta1 > 0 ? 1 : -1;
                        int i0 = tabs.IndexOf(item: activeTab);
                        int i1 = Mathf.Clamp(i0 + dir, 0, tabs.Count - 1);

                        (tabs[index: i0], tabs[index: i1]) = (tabs[index: i1], tabs[index: i0]);

                        tabs[index: i1].Focus();
                    }
                    else
                    {
                        int dir = scrollDelta1 > 0 ? 1 : -1;
                        int i0 = tabs.IndexOf(item: activeTab);
                        int i1 = Mathf.Clamp(i0 + dir, 0, tabs.Count - 1);

                        tabs[index: i1].Focus();

                        UpdateTitle(tabs[index: i1]);
                    }
                }
            }
        }

        if (!TabifyMenu.sidescrollEnabled) return;

        if (scrollEvent.modifiers != EventModifiers.None &&
            scrollEvent.modifiers != EventModifiers.Command &&
            scrollEvent.modifiers != EventModifiers.Control) return;

        if (scrollEvent.delta.x.Abs() < scrollEvent.delta.y.Abs())
        {
            sidescrollPosition = 0;
            return;
        }

        e.StopPropagation();

        if (scrollEvent.delta.x.Abs() <= 0.06f) return;

        var
            dampenK = 5; // the larger this k is - the smaller big deltas are, and the less is sidescroll's dependency on scroll speed
        float a = scrollEvent.delta.x.Abs() * dampenK;
        float deltaDampened = (a < 1 ? a : Mathf.Log(f: a) + 1) / dampenK * -scrollEvent.delta.x.Sign();

        var sensitivityK = .22f;
        float scrollDelta = deltaDampened * TabifyMenu.sidescrollSensitivity * sensitivityK;

        if (TabifyMenu.reverseScrollDirectionEnabled)
            scrollDelta *= -1;

        if (sidescrollPosition.RoundToInt() == (sidescrollPosition += scrollDelta).RoundToInt()) return;

        if (scrollEvent.ctrlKey || scrollEvent.commandKey)
        {
            int dir = scrollDelta > 0 ? 1 : -1;
            int i0 = tabs.IndexOf(item: activeTab);
            int i1 = Mathf.Clamp(i0 + dir, 0, tabs.Count - 1);

            (tabs[index: i0], tabs[index: i1]) = (tabs[index: i1], tabs[index: i0]);

            tabs[index: i1].Focus();
        }
        else
        {
            int dir = scrollDelta > 0 ? 1 : -1;
            int i0 = tabs.IndexOf(item: activeTab);
            int i1 = Mathf.Clamp(i0 + dir, 0, tabs.Count - 1);

            tabs[index: i1].Focus();

            UpdateTitle(tabs[index: i1]);
        }
    }

    private float sidescrollPosition;

    private void HandleDragndrop(EventBase e)
    {
        if (!TabifyMenu.dragndropEnabled) return;

        Rect dragndropArea =
            panel.visualTree.contentRect.SetHeight(activeTab.GetType() == SceneHierarchyWindowType ? 20 : 40);

        if (!dragndropArea.Contains(point: e.originalMousePosition)) return;

        if (e is DragUpdatedEvent)
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

        if (e is not DragPerformEvent) return;

        DragAndDrop.AcceptDrag();

        AddTab(new TabInfo(DragAndDrop.objectReferences.First()));

        lastDragndropTime = DateTime.UtcNow;
    }

    private static DateTime lastDragndropTime;

    private void HandleHidingCloseButton(EventBase e)
    {
        if (e is MouseDownEvent && !isCloseButtonHovered)
            if (hoveredTab != null && hoveredTab != activeTab)
                hideCloseButtonOnTab = hoveredTab;

        if (e is MouseMoveEvent)
            if (hoveredTab != hideCloseButtonOnTab)
                hideCloseButtonOnTab = null;
    }

    private EditorWindow hideCloseButtonOnTab;

    private void HandleRightClickEmptySpace(MouseDownEvent mouseDownEvent)
    {
        if (mouseDownEvent.button != 1) return; // Right mouse button
        if (!tabStripElement.contentRect.Contains(point: mouseDownEvent.mousePosition)) return;

        // Convert mouse position to tab strip local space
        Vector2 localMousePos = mouseDownEvent.localMousePosition;

        // Check if click is in empty space (not on any tab or interactive element)
        float lastTabEndX = tabEndPositions.Any() ? tabEndPositions.Last() : 0;
        bool isInEmptySpace = localMousePos.x > lastTabEndX;

        // Also check not clicking on any interactive rect
        foreach (Rect rect in interactiveRects)
            if (rect.Contains(point: localMousePos))
                return;

        if (!isInEmptySpace) return;
        if (mouseDownEvent.modifiers == EventModifiers.Alt) return; // Let hidden menu handle alt+right-click

        mouseDownEvent.StopPropagation();

        // Open the add tab window
        if (TabifyAddTabWindow.instance)
            TabifyAddTabWindow.instance.Close();
        else
            TabifyAddTabWindow.Open(dockArea: dockArea);
    }

    private void HandleHiddenMenu(MouseDownEvent mouseDownEvent)
    {
        if (mouseDownEvent.modifiers != EventModifiers.Alt) return;
        if (mouseDownEvent.button != 1) return;
        if (!tabStripElement.contentRect.Contains(point: mouseDownEvent.mousePosition)) return;

        mouseDownEvent.StopPropagation();

        GenericMenu menu = new();

        menu.AddDisabledItem(new GUIContent("Tabify hidden menu"));

        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select cache"), false, () => Selection.activeObject = TabifyCache.instance);
        menu.AddItem(new GUIContent("Clear cache"), false, func: TabifyCache.Clear);

        menu.ShowAsContext();
    }

    public EditorWindow AddTab(TabInfo tabInfo, bool atOriginalTabIndex = false)
    {
        object lastInteractedBrowser =
            ProjectBrowserType.GetFieldValue("s_LastInteractedProjectBrowser"); // changes on new browser creation

        var window = (EditorWindow)ScriptableObject.CreateInstance(className: tabInfo.typeName);

        if (atOriginalTabIndex)
            dockArea.InvokeMethod("AddTab", tabInfo.originalTabIndex, window, true);
        else
            dockArea.InvokeMethod("AddTab", window, true);

        if (tabInfo.isBrowser)
        {
            window.InvokeMethod("Init");

            if (tabInfo.isGridSizeSaved)
                window.GetFieldValue("m_ListArea")?.SetMemberValue("gridSize", val: tabInfo.savedGridSize);

            if (!tabInfo.isGridSizeSaved && lastInteractedBrowser != null)
            {
                object listAreaSource = lastInteractedBrowser.GetFieldValue("m_ListArea");
                object listAreaDest = window.GetFieldValue("m_ListArea");

                if (listAreaSource != null && listAreaDest != null)
                    listAreaDest.SetPropertyValue("gridSize", listAreaSource.GetPropertyValue("gridSize"));
            }

            if (tabInfo.isLayoutSaved)
            {
                var layoutEnum = Enum.ToObject(
                    enumType: ProjectBrowserType.GetField("m_ViewMode", bindingAttr: maxBindingFlags).FieldType,
                    value: tabInfo.savedLayout);

                window.InvokeMethod("SetViewMode", layoutEnum);
            }

            if (!tabInfo.isLayoutSaved && lastInteractedBrowser != null)
                window.InvokeMethod("SetViewMode", lastInteractedBrowser.GetMemberValue("m_ViewMode"));

            if (lastInteractedBrowser != null)
                window.SetFieldValue("m_DirectoriesAreaWidth",
                    lastInteractedBrowser.GetFieldValue("m_DirectoriesAreaWidth"));

            if (tabInfo.isLocked &&
                window.GetMemberValue<int>("m_ViewMode") == 1 &&
                !tabInfo.folderGuid.IsNullOrEmpty())
            {
                int iid = AssetDatabase.
                    LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid: tabInfo.folderGuid)).
                    GetInstanceID();

                window.GetFieldValue("m_ListAreaState").
                    SetFieldValue("m_SelectedInstanceIDs", new List<EntityId>
                    {
                        iid
                    });

                ProjectBrowserType.InvokeMethod("OpenSelectedFolders");

                window.SetPropertyValue("isLocked", true);
            }

            if (tabInfo.isLocked &&
                window.GetMemberValue<int>("m_ViewMode") == 0 &&
                !tabInfo.folderGuid.IsNullOrEmpty() &&
                window.GetMemberValue("m_AssetTree") is { } m_AssetTree &&
                m_AssetTree.GetMemberValue("data") is { } data)
            {
                string folderPath = tabInfo.folderGuid.ToPath();
                int folderIid = AssetDatabase.LoadAssetAtPath<Object>(assetPath: folderPath).
                    GetInstanceID();

                data.SetMemberValue("m_rootInstanceID", (EntityId)folderIid);

                m_AssetTree.InvokeMethod("ReloadData");

                window.SetLockedFolderPath_oneColumn(folderPath: folderPath);

                window.SetPropertyValue("isLocked", true);
            }

            UpdateTitle(window: window);
        }

        if (tabInfo.isPropertyEditor)
        {
            Object lockTo = tabInfo.globalId.GetObject();

            if (tabInfo.lockedPrefabAssetObject)
                lockTo = tabInfo.
                    lockedPrefabAssetObject; // globalId api doesn't work for prefab asset objects, so we use direct object reference in such cases

            if (lockTo)
            {
                window.GetMemberValue("tracker").
                    InvokeMethod("SetObjectsLockedByThisTracker", new List<Object>
                    {
                        lockTo
                    });

                if (StageUtility.GetCurrentStage() is PrefabStage && tabInfo.globalId.isNull)
                    window.SetMemberValue("m_GlobalObjectId", GlobalID.GetForPrefabStageObject(o: lockTo).ToString());
                else
                    window.SetMemberValue("m_GlobalObjectId", tabInfo.globalId.ToString());

                window.SetMemberValue("m_InspectedObject", val: lockTo);

                UpdateTitle(window: window);
            }
        }

        if (window.titleContent.text == window.GetType().FullName && !tabInfo.originalTitle.IsNullOrEmpty())
            window.titleContent.text = tabInfo.originalTitle;

        // custom EditorWindows often have their titles set in EditorWindow.GetWindow
        // and when such windows are created via ScriptableObject.CreateInstance, their titles default to window type name
        // so we have to set original window title in such cases

        window.Focus();
        return window;
    }

    public void CloseTab(EditorWindow tab)
    {
        tabInfosForReopening.Push(new TabInfo(window: tab));
        TabifyAddTabWindow.RememberWindow(window: tab);
        tab.Close();
    }

    public void ReopenClosedTab()
    {
        if (!tabInfosForReopening.Any()) return;

        TabInfo tabInfo = tabInfosForReopening.Pop();

        EditorWindow prevActiveTab = activeTab;

        EditorWindow reopenedTab = AddTab(tabInfo: tabInfo, true);

        if (!tabInfo.wasFocused)
            prevActiveTab.Focus();

        UpdateTitle(window: reopenedTab);
    }

    private readonly Stack<TabInfo> tabInfosForReopening = new();

    public void UpdateScrollAnimation()
    {
        if (activeTab != EditorWindow.focusedWindow) return;
        if (!guiStylesInitialized) return;
        if ((DateTime.UtcNow - lastDragndropTime).TotalSeconds < .05f) return; // to avoid stutter after dragndrop

        var curScrollPos = dockArea.GetFieldValue<float>("m_ScrollOffset");

        if (!curScrollPos.Approx(0))
            curScrollPos -= nonZeroTabScrollOffset;

        if (curScrollPos == 0)
            curScrollPos = prevScrollPos; // prevents immediate jump to 0 on tab close

        float targScrollPos = GetTargetScrollPosition();

        var animationSpeed = 7f;

        float newScrollPos = MathUtil.SmoothDamp(current: curScrollPos, target: targScrollPos, speed: animationSpeed,
            derivative: ref scrollPosDeriv,
            deltaTime: editorDeltaTime);

        if (newScrollPos < .5f)
            newScrollPos = 0;

        prevScrollPos = newScrollPos;

        if (newScrollPos.Approx(f2: curScrollPos)) return;

        if (!newScrollPos.Approx(0))
            newScrollPos += nonZeroTabScrollOffset;

        dockArea.SetFieldValue("m_ScrollOffset", val: newScrollPos);

        activeTab.Repaint();
    }

    public float nonZeroTabScrollOffset = 3f;

    private float scrollPosDeriv;
    private float prevScrollPos;

    public float GetTargetScrollPosition()
    {
        if (!guiStylesInitialized) return 0;

        float tabAreaWidth = dockArea.GetFieldValue<Rect>("m_TabAreaRect").width;

        if (tabAreaWidth == 0)
            tabAreaWidth = activeTab.position.width - 38;

        var activeTabXMin = 0f;
        var activeTabXMax = 0f;

        var tabWidthSum = 0f;

        var activeTabReached = false;

        foreach (EditorWindow tab in tabs)
        {
            float tabWidth = GetTabWidth(tab: tab);

            tabWidthSum += tabWidth;

            if (activeTabReached) continue;

            activeTabXMin = activeTabXMax;
            activeTabXMax += tabWidth;

            if (tab == activeTab)
                activeTabReached = true;
        }

        var optimalScrollPos = 0f;

        var visibleAreaPadding = 65f;

        float visibleAreaXMin = activeTabXMin - visibleAreaPadding;
        float visibleAreaXMax = activeTabXMax + visibleAreaPadding;

        optimalScrollPos = Mathf.Max(a: optimalScrollPos, visibleAreaXMax - tabAreaWidth);
        optimalScrollPos = Mathf.Min(a: optimalScrollPos, tabWidthSum - tabAreaWidth + 4);

        optimalScrollPos = Mathf.Min(a: optimalScrollPos, b: visibleAreaXMin);
        optimalScrollPos = Mathf.Max(a: optimalScrollPos, 0);

        return optimalScrollPos;
    }

    public float GetTabWidth(EditorWindow tab)
    {
        if (guiStylesInitialized)
            tabStyle ??= typeof(GUI).GetMemberValue<GUISkin>("s_Skin")?.FindStyle("dragtab");

        if (tabStyle == null) return 0;

        return dockArea.InvokeMethod<float>("GetTabWidth", tabStyle, tab);
    }

    private static GUIStyle tabStyle;

    private bool guiStylesInitialized => typeof(GUI).GetFieldValue("s_Skin") != null;

    public void UpdateLockButtonHiding()
    {
        bool isLocked(EditorWindow window)
        {
            var t = window.GetType();

            if (t == SceneHierarchyWindowType)
                return window.GetMemberValue("m_SceneHierarchy").GetMemberValue<bool>("isLocked");

            if (t == InspectorWindowType)
                return window.GetMemberValue<bool>("isLocked");

            return false;
        }

        bool shouldHideLockButton = TabifyMenu.hideLockButtonEnabled && !isLocked(window: activeTab);

        switch (shouldHideLockButton)
        {
            case false when lockButtonDelegate != null:
                dockArea.SetMemberValue("m_ShowButton", val: lockButtonDelegate);
                lockButtonDelegate = null;
                break;
            case true:
                lockButtonDelegate ??= dockArea.GetMemberValue("m_ShowButton");
                dockArea.SetMemberValue("m_ShowButton", null);
                break;
        }
    }

    private object lockButtonDelegate;

    public TabifyGUI(Object dockArea)
    {
        this.dockArea = dockArea;

        panel = dockArea.GetMemberValue<EditorWindow>("actualView").rootVisualElement.panel;

        tabs = dockArea.GetMemberValue<List<EditorWindow>>("m_Panes");

        panel.visualTree.RegisterCallback<WheelEvent>(callback: HandleTabScrolling,
            useTrickleDown: TrickleDown.TrickleDown);
        panel.visualTree.RegisterCallback<MouseMoveEvent>(callback: HandleTabScrolling,
            useTrickleDown: TrickleDown.TrickleDown);

        panel.visualTree.RegisterCallback<DragUpdatedEvent>(callback: HandleDragndrop,
            useTrickleDown: TrickleDown.TrickleDown);
        panel.visualTree.
            RegisterCallback<
                DragPerformEvent>(
                callback: HandleDragndrop); // no trickledown to avoid creating tab when dropping on navbar

        panel.visualTree.RegisterCallback<MouseDownEvent>(callback: HandleHidingCloseButton,
            useTrickleDown: TrickleDown.TrickleDown);
        panel.visualTree.RegisterCallback<MouseMoveEvent>(callback: HandleHidingCloseButton,
            useTrickleDown: TrickleDown.TrickleDown);

        panel.visualTree.RegisterCallback<MouseDownEvent>(callback: HandleRightClickEmptySpace,
            useTrickleDown: TrickleDown.TrickleDown);
        panel.visualTree.RegisterCallback<MouseDownEvent>(callback: HandleHiddenMenu,
            useTrickleDown: TrickleDown.TrickleDown);

        tabStripElement = new IMGUIContainer();

        tabStripElement.name = "tabify-tab-strip";

        tabStripElement.style.width = Length.Percent(100);
        tabStripElement.style.height = 34;
        tabStripElement.style.position = Position.Absolute;

        tabStripElement.pickingMode = PickingMode.Ignore;

        tabStripElement.onGUIHandler = () => TabStripGUI(stripRect: tabStripElement.contentRect);

        panel.visualTree.Add(child: tabStripElement);

        EditorApplication.delayCall += DelayCallRepaintLoop;
    }

    private readonly Object dockArea;
    private readonly IPanel panel;
    public List<EditorWindow> tabs;
    private readonly IMGUIContainer tabStripElement;

    public EditorWindow activeTab => tabs.FirstOrDefault(r => r.hasFocus);
}
#endif

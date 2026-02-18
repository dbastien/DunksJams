#if UNITY_EDITOR

#region

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using Type = System.Type;
using static TabifyCache;
using static Tabify;
using static EditorGUIUtil;
using static ColorExtensions;
using SearchField = UnityEditor.IMGUI.Controls.SearchField;

#endregion

public class TabifyAddTabWindow : EditorWindow
{
    private void OnGUI()
    {
        // Background
        windowRect.Draw(color: windowBackground);

        // Close on Escape
        if (curEvent.isKeyDown() && curEvent.keyCode == KeyCode.Escape)
        {
            Close();
            dockArea.GetMemberValue<EditorWindow>("actualView").Repaint(); // for + button to fade
            GUIUtility.ExitGUI();
        }

        // Add tab on Enter
        if (curEvent.keyCode == KeyCode.Return && keyboardFocusedRowIndex != -1 && keyboardFocusedEntry != null)
        {
            AddTab(entry: keyboardFocusedEntry);
            Close();
        }

        // Arrow navigation
        if (curEvent.isKeyDown() && (curEvent.keyCode == KeyCode.UpArrow || curEvent.keyCode == KeyCode.DownArrow))
        {
            curEvent.Use();

            switch (curEvent.keyCode)
            {
                case KeyCode.UpArrow when keyboardFocusedRowIndex == 0: keyboardFocusedRowIndex = rowCount - 1; break;
                case KeyCode.UpArrow: keyboardFocusedRowIndex--; break;
                case KeyCode.DownArrow when keyboardFocusedRowIndex == rowCount - 1: keyboardFocusedRowIndex = 0; break;
                case KeyCode.DownArrow: keyboardFocusedRowIndex++; break;
            }

            keyboardFocusedRowIndex = keyboardFocusedRowIndex.Clamp(0, rowCount - 1);
        }

        // Update search
        if (searchString != prevSearchString)
        {
            prevSearchString = searchString;

            if (searchString == "") { keyboardFocusedRowIndex = -1; }
            else
            {
                UpdateSearch();
                keyboardFocusedRowIndex = 0;
            }
        }

        // Search field
        Rect searchRect = windowRect.SetHeight(18).MoveY(1).AddWidthFromMid(-2);
        if (searchField == null)
        {
            searchField = new();
            searchField.SetFocus();
        }

        searchString = searchField.OnGUI(rect: searchRect, text: searchString);

        // Rows (bookmarked, not bookmarked, and searched entries)
        scrollPos = GUI.BeginScrollView(windowRect.AddHeightFromBottom(f: -firstRowOffsetTop), Vector2.up * scrollPos,
                windowRect.SetHeight(f: scrollAreaHeight), horizontalScrollbar: GUIStyle.none,
                verticalScrollbar: GUIStyle.none).
            y;

        nextRowY = 0;
        nextRowIndex = 0;

        // Bookmarked entries
        if (searchString == "" && bookmarkedEntries.Any())
        {
            bookmarksRect = windowRect.SetHeight(bookmarkedEntries.Count * rowHeight + gaps.Sum());
            BookmarksGUI();

            // Divider
            Color splitterColor = Greyscale(.36f);
            Rect splitterRect = bookmarksRect.SetHeightFromBottom(0).
                SetHeight(f: dividerHeight).
                SetHeightFromMid(1).
                AddWidthFromMid(-10);
            splitterRect.Draw(color: splitterColor);

            nextRowY = bookmarksRect.yMax + dividerHeight;
        }

        // Not bookmarked entries
        if (searchString == "")
            foreach (TabEntry entry in allEntries)
            {
                if (bookmarkedEntries.Contains(item: entry)) continue;
                if (entry == draggedBookmark) continue;

                RowGUI(windowRect.SetHeight(f: rowHeight).SetY(y: nextRowY), entry: entry);

                nextRowY += rowHeight;
                nextRowIndex++;
            }

        // Searched entries
        if (searchString != "")
            foreach (TabEntry entry in searchedEntries)
            {
                RowGUI(windowRect.SetHeight(f: rowHeight).SetY(y: nextRowY), entry: entry);

                nextRowY += rowHeight;
                nextRowIndex++;
            }

        scrollAreaHeight = nextRowY + 23;
        rowCount = nextRowIndex;

        GUI.EndScrollView();

        // No results message
        if (searchString != "" && !searchedEntries.Any())
        {
            GUI.enabled = false;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            GUI.Label(windowRect.AddHeightFromBottom(-14), "No results");

            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.enabled = true;
        }

        // Outline (non-Mac only)
        if (Application.platform != RuntimePlatform.OSXEditor) position.SetPos(0, 0).DrawOutline(Greyscale(.1f));

        if (draggingBookmark || animatingDroppedBookmark || animatingGaps)
            Repaint();
    }

    private Rect windowRect => position.SetPos(0, 0);
    private Rect bookmarksRect;

    private SearchField searchField;

    private Color windowBackground => Greyscale(isDarkTheme ? .23f : .8f);

    private string searchString = "";
    private string prevSearchString = "";

    private float scrollPos;

    private float rowHeight => 22;
    private float dividerHeight => 11;
    private float firstRowOffsetTop => bookmarkedEntries.Any() && searchString == "" ? 21 : 20;

    private int nextRowIndex;
    private float nextRowY;

    private float scrollAreaHeight = 1232;
    private int rowCount = 123;

    private int keyboardFocusedRowIndex = -1;

    private void RowGUI(Rect rowRect, TabEntry entry)
    {
        bool isHovered = rowRect.IsHovered();
        bool isPressed = entry == pressedEntry;
        bool isDragged = draggingBookmark && draggedBookmark == entry;
        bool isDropped = animatingDroppedBookmark && droppedBookmark == entry;
        bool isFocused = entry == keyboardFocusedEntry;
        bool isBookmarked = bookmarkedEntries.Contains(item: entry) || entry == draggedBookmark;

        bool showBlueBackground = isFocused || isPressed || isDragged;

        if (isDropped)
            isHovered = rowRect.SetY(y: droppedBookmarkYTarget).IsHovered();

        rowRect.MarkInteractive();

        // Dragged shadow
        if (isDragged)
        {
            Rect shadowRect = rowRect.AddHeightFromMid(-4);
            shadowRect.Draw(Greyscale(0, .15f));
        }

        // Blue background
        if (curEvent.isRepaint() && showBlueBackground)
        {
            Rect backgroundRect = rowRect.AddHeightFromMid(-3);
            backgroundRect.Draw(color: GUIColors.selectedBackground);
        }

        // Icon
        if (curEvent.isRepaint())
        {
            Texture iconTexture = EditorIcons.GetIcon(iconName: entry.iconName, true);

            if (iconTexture)
            {
                Rect iconRect = rowRect.SetWidth(16).SetHeightFromMid(16).MoveX(4 + 1);
                iconRect = iconRect.SetWidthFromMid(iconRect.height * iconTexture.width / iconTexture.height);
                GUI.DrawTexture(position: iconRect, image: iconTexture);
            }
        }

        // Name
        if (curEvent.isRepaint())
        {
            Rect nameRect = rowRect.MoveX(21 + 1);
            string nameText = searchString != "" ? namesFormattedForFuzzySearch_byEntry[key: entry] : entry.name;

            Color color = showBlueBackground ? Greyscale(123, 123)
                : isHovered && !isPressed ? Greyscale(1.1f)
                : Greyscale(1);
            SetGUIColor(c: color);

            GUI.skin.label.richText = true;
            GUI.Label(position: nameRect, text: nameText);
            GUI.skin.label.richText = false;

            ResetGUIColor();
        }

        // Star button
        if ((isHovered || isBookmarked) && !(isFocused && !isHovered))
        {
            Rect buttonRect = rowRect.SetWidthFromRight(16).MoveX(-6 + 1).SetSizeFromMid(f: rowHeight);

            string iconName = isBookmarked ^ buttonRect.IsHovered() ? "Favorite" : "Favorite Icon";
            var iconSize = 16;
            Color colorNormal = Greyscale(isDarkTheme ? isBookmarked ? .5f : .7f : .3f);
            Color colorHovered = Greyscale(isDarkTheme ? isBookmarked ? .9f : 1 : 0f);
            Color colorPressed = Greyscale(isDarkTheme ? .75f : .5f);
            Color colorDisabled = Greyscale(isDarkTheme ? .53f : .55f);

            if (IconButton(rect: buttonRect, iconName: iconName, iconSize: iconSize, colorNormal: colorNormal,
                    colorHovered: colorHovered, colorPressed: colorPressed))
            {
                if (isBookmarked)
                    bookmarkedEntries.Remove(item: entry);
                else
                    bookmarkedEntries.Add(item: entry);
            }
        }

        // Enter hint
        if (curEvent.isRepaint() && isFocused && !isHovered && isDarkTheme)
        {
            Rect hintRect = rowRect.SetWidthFromRight(33);

            SetLabelFontSize(10);
            SetGUIColor(Greyscale(.9f));

            GUI.Label(position: hintRect, "Enter");

            ResetGUIColor();
            ResetLabelStyle();
        }

        // Hover highlight
        if (isHovered && !(isPressed || isDragged))
        {
            Rect backgroundRect = rowRect.AddHeightFromMid(-2);
            Color backgroundColor = Greyscale(isDarkTheme ? 1 : 0, isPressed ? .085f : .12f);
            backgroundRect.Draw(color: backgroundColor);
        }

        // Mouse down
        if (curEvent.isMouseDown() && rowRect.IsHovered())
        {
            isMousePressedOnEntry = true;
            pressedEntry = entry;
            mouseDownPosition = curEvent.mousePosition;
            Repaint();
        }

        // Mouse up
        if (curEvent.isMouseUp())
        {
            isMousePressedOnEntry = false;
            pressedEntry = null;
            Repaint();

            if (isHovered && !draggingBookmark && (curEvent.mousePosition - mouseDownPosition).magnitude <= 2)
            {
                curEvent.Use();
                AddTab(entry: entry);
                Close();
            }
        }

        // Set focused entry
        int rowIndex = (rowRect.y / rowHeight).FloorToInt();
        if (rowIndex == keyboardFocusedRowIndex)
            keyboardFocusedEntry = entry;
    }

    private TabEntry pressedEntry;

    private bool isMousePressedOnEntry;

    private Vector2 mouseDownPosition;

    private TabEntry keyboardFocusedEntry;

    private void AddTab(TabEntry entry)
    {
        var windowType = Type.GetType(typeName: entry.typeString);

        var window = CreateInstance(type: windowType) as EditorWindow;

        string windowName = entry.name;
        Texture windowIcon = EditorIcons.GetIcon(iconName: entry.iconName, true);

        window.titleContent = new(text: windowName, image: windowIcon);

        dockArea.InvokeMethod("AddTab", window, true);

        window.Focus();
    }

    public void BookmarksGUI()
    {
        BookmarksDragging();
        BookmarksAnimations();

        // Normal bookmarks
        for (var i = 0; i < bookmarkedEntries.Count; ++i)
        {
            if (bookmarkedEntries[index: i] == droppedBookmark && animatingDroppedBookmark) continue;
            Rect bookmarkRect = bookmarksRect.SetHeight(f: rowHeight).SetY(GetBookmarkCenterY(i: i));
            RowGUI(rowRect: bookmarkRect, bookmarkedEntries[index: i]);
        }

        // Dragged bookmark
        if (draggingBookmark)
        {
            Rect bookmarkRect = bookmarksRect.SetHeight(f: rowHeight).SetY(y: draggedBookmarkY);
            RowGUI(rowRect: bookmarkRect, entry: draggedBookmark);
        }

        // Dropped bookmark
        if (animatingDroppedBookmark)
        {
            Rect bookmarkRect = bookmarksRect.SetHeight(f: rowHeight).SetY(y: droppedBookmarkY);
            RowGUI(rowRect: bookmarkRect, entry: droppedBookmark);
        }
    }

    private int GetBookmarkIndex(float mouseY) => ((mouseY - bookmarksRect.y) / rowHeight).FloorToInt();

    private float GetBookmarkCenterY(int i, bool includeGaps = true) =>
        bookmarksRect.y + i * rowHeight + (includeGaps ? gaps.Take(i + 1).Sum() : 0);

    private void BookmarksDragging()
    {
        // Initialize dragging
        if (!draggingBookmark && (curEvent.mousePosition - mouseDownPosition).magnitude > 2)
            if (isMousePressedOnEntry && bookmarkedEntries.Contains(item: pressedEntry))
            {
                int i = GetBookmarkIndex(mouseY: mouseDownPosition.y);

                if (i >= 0 && i < bookmarkedEntries.Count)
                {
                    animatingDroppedBookmark = false;

                    draggingBookmark = true;

                    draggedBookmark = bookmarkedEntries[index: i];
                    draggedBookmarkHoldOffsetY = GetBookmarkCenterY(i: i) - mouseDownPosition.y;

                    gaps[index: i] = rowHeight;

                    this.RecordUndo();

                    bookmarkedEntries.Remove(item: draggedBookmark);
                }
            }

        // Accept drop
        if (draggingBookmark && (curEvent.isMouseUp() || curEvent.isIgnore()))
        {
            curEvent.Use();
            EditorGUIUtility.hotControl = 0;

            // DragAndDrop.PrepareStartDrag(); // fixes phantom dragged component indicator after reordering bookmarks

            this.RecordUndo();

            draggingBookmark = false;
            isMousePressedOnEntry = false;

            bookmarkedEntries.AddAt(item: draggedBookmark, index: insertDraggedBookmarkAtIndex);

            gaps[index: insertDraggedBookmarkAtIndex] -= rowHeight;
            gaps.AddAt(0, index: insertDraggedBookmarkAtIndex);

            droppedBookmark = draggedBookmark;

            droppedBookmarkY = draggedBookmarkY;
            droppedBookmarkYDerivative = 0;
            animatingDroppedBookmark = true;

            draggedBookmark = null;
            pressedEntry = null;

            EditorGUIUtility.hotControl = 0;
        }

        // Update dragging
        if (draggingBookmark)
        {
            EditorGUIUtility.hotControl = EditorGUIUtility.GetControlID(focus: FocusType.Passive);

            draggedBookmarkY =
                (curEvent.mousePosition.y + draggedBookmarkHoldOffsetY).Clamp(0, bookmarksRect.yMax - rowHeight);

            insertDraggedBookmarkAtIndex =
                GetBookmarkIndex(curEvent.mousePosition.y + draggedBookmarkHoldOffsetY + rowHeight / 2).
                    Clamp(0, f1: bookmarkedEntries.Count);
        }
    }

    private bool draggingBookmark;

    private float draggedBookmarkHoldOffsetY;

    private float draggedBookmarkY;
    private int insertDraggedBookmarkAtIndex;

    private TabEntry draggedBookmark;
    private TabEntry droppedBookmark;

    private void BookmarksAnimations()
    {
        if (!curEvent.isLayout()) return;

        // Gaps animation
        bool makeSpaceForDraggedBookmark = draggingBookmark;

        // var lerpSpeed = 1;
        var lerpSpeed = 11;

        for (var i = 0; i < gaps.Count; i++)
            if (makeSpaceForDraggedBookmark && i == insertDraggedBookmarkAtIndex)
                gaps[index: i] = MathUtil.Lerp(gaps[index: i], target: rowHeight, speed: lerpSpeed,
                    deltaTime: editorDeltaTime);
            else
                gaps[index: i] = MathUtil.Lerp(gaps[index: i], 0, speed: lerpSpeed, deltaTime: editorDeltaTime);

        for (var i = 0; i < gaps.Count; i++)
            if (gaps[index: i].Approx(0))
                gaps[index: i] = 0;

        animatingGaps = gaps.Any(r => r > .1f);

        // Dropped bookmark animation
        if (animatingDroppedBookmark)
        {
            // var lerpSpeed = 1;
            lerpSpeed = 8;

            droppedBookmarkYTarget = GetBookmarkCenterY(bookmarkedEntries.IndexOf(item: droppedBookmark), false);

            MathUtil.SmoothDamp(current: ref droppedBookmarkY, target: droppedBookmarkYTarget, speed: lerpSpeed,
                derivative: ref droppedBookmarkYDerivative,
                deltaTime: editorDeltaTime);

            if ((droppedBookmarkY - droppedBookmarkYTarget).Abs() < .5f)
                animatingDroppedBookmark = false;
        }
    }

    private float droppedBookmarkY;
    private float droppedBookmarkYTarget;
    private float droppedBookmarkYDerivative;

    private bool animatingDroppedBookmark;
    private bool animatingGaps;

    private List<float> gaps
    {
        get
        {
            while (_gaps.Count < bookmarkedEntries.Count + 1) _gaps.Add(0);
            while (_gaps.Count > bookmarkedEntries.Count + 1) _gaps.RemoveLast();

            return _gaps;
        }
    }

    private readonly List<float> _gaps = new();

    public static void UpdateAllEntries()
    {
        // Skip if already populated to improve performance
        if (allEntries is { Count: > 0 }) return;

        // Fill with defaults if needed
        if (allEntries.Count < 15 || allEntries.Any(r => r == null || r.typeString.IsNullOrEmpty()))
        {
            allEntries.Clear();

            foreach (Type type in TypeCache.GetTypesWithAttribute<EditorWindowTitleAttribute>())
            {
                var titleAttribute = type.GetCustomAttribute<EditorWindowTitleAttribute>();

                var entry = new TabEntry();

                entry.typeString = type.AssemblyQualifiedName;
                entry.name = titleAttribute.title ?? "";
                entry.iconName = titleAttribute.useTypeNameAsIconName ? type.FullName : titleAttribute.icon ?? "";

                if (entry.iconName.IsNullOrEmpty()) continue; // filters out internal windows and such

                allEntries.Add(item: entry);
            }

            allEntries.Add(new()
            {
                name = "Preferences", iconName = "d_Settings@2x",
                typeString =
                    "UnityEditor.PreferenceSettingsWindow, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
            });
            allEntries.Add(new()
            {
                name = "Project Settings", iconName = "d_Settings@2x",
                typeString =
                    "UnityEditor.ProjectSettingsWindow, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
            });

            allEntries.Add(new()
            {
                name = "Background Tasks", iconName = "",
                typeString =
                    "UnityEditor.ProgressWindow, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
            });

            allEntries.Add(new()
            {
                name = "Frame Debugger", iconName = "",
                typeString =
                    "UnityEditor.FrameDebuggerWindow, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
            });
            allEntries.Add(new()
            {
                name = "Physics Debug", iconName = "",
                typeString =
                    "UnityEditor.PhysicsDebugWindow, UnityEditor.PhysicsModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
            });
            allEntries.Add(new()
            {
                name = "UI Toolkit Debugger", iconName = "",
                typeString =
                    "UnityEditor.UIElements.Debugger.UIElementsDebugger, UnityEditor.UIElementsModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
            });
            allEntries.Add(new()
            {
                name = "UI Builder", iconName = "d_UIBuilder@2x",
                typeString =
                    "Unity.UI.Builder.Builder, UnityEditor.UIBuilderModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
            });

            allEntries.Add(new()
            {
                name = "Test Runnder", iconName = "",
                typeString =
                    "UnityEditor.TestTools.TestRunner.TestRunnerWindow, UnityEditor.TestRunner, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            });
            allEntries.Add(new()
            {
                name = "Search", iconName = "d_SearchWindow@2x",
                typeString =
                    "UnityEditor.Search.SearchWindow, UnityEditor.QuickSearchModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
            });

            allEntries.Add(new()
            {
                name = "Build Settings", iconName = "",
                typeString =
                    "UnityEditor.BuildPlayerWindow, UnityEditor.CoreModule, Version = 0.0.0.0, Culture = neutral, PublicKeyToken = null"
            });
            allEntries.Add(new()
            {
                name = "Build Profiles", iconName = "",
                typeString =
                    "UnityEditor.Build.Profile.BuildProfileWindow, UnityEditor.BuildProfileModule, Version = 0.0.0.0, Culture = neutral, PublicKeyToken = null"
            });

            allEntries.Add(new()
            {
                name = "Shortcuts", iconName = "",
                typeString =
                    "UnityEditor.ShortcutManagement.ShortcutManagerWindow, UnityEditor.CoreModule, Version = 0.0.0.0, Culture = neutral, PublicKeyToken = null"
            });

            allEntries.Add(new()
            {
                name = "IMGUI Debugger", iconName = "",
                typeString =
                    "UnityEditor.GUIViewDebuggerWindow, UnityEditor.CoreModule, Version = 0.0.0.0, Culture = neutral, PublicKeyToken = null"
            });

            allEntries.RemoveAll(r => allEntries.Count(rr => rr.name == r.name) > 1);

            List<string> order = new[]
            {
                "Scene",
                "Game",
                "Project",
                "Console",
                "Inspector",
                "Hierarchy",
                "Package Manager",
                "Project Settings",

                "Animation",
                "Animator",

                "Profiler",

                "Lighting",
                "Light Explorer",
                // "Viewer",
                "Occlusion",

                "UI Toolkit Debugger",
                "UI Builder",

                "Frame Debugger",
                "Physics Debug",

                "Preferences",
                "Simulator",

                "Build Settings",
                "Build Profiles"
            }.ToList();

            allEntries.SortBy(r => order.IndexOf(item: r.name) is int i && i != -1 ? i : 1232);
        }

        // Remember all open tabs
        foreach (EditorWindow window in allEditorWindows)
            RememberWindow(window: window);

        // Remove blacklisted
        allEntries.RemoveAll(r => r.name == "Asset Store");
        allEntries.RemoveAll(r => r.name == "UI Toolkit Samples");

        // Remove unresolvable types
        allEntries.RemoveAll(r => Type.GetType(typeName: r.typeString) == null);
    }

    public static void RememberWindow(EditorWindow window)
    {
        if (!window.docked) return;
        if (window.GetType() == t_PropertyEditor) return;
        if (window.GetType() == t_InspectorWindow) return;
        if (window.GetType() == t_ProjectBrowser) return;

        string typeString = window.GetType().AssemblyQualifiedName;

        if (allEntries.Any(r => r.typeString == typeString)) return;

        string name = window.titleContent.text;

        string iconName = window.titleContent.image ? window.titleContent.image.name : "";

        allEntries.Add(new() { typeString = typeString, name = name, iconName = iconName });
    }

    private static List<TabEntry> allEntries => TabifyCache.instance.allTabEntries;

    private void GetBookmarkedEntries()
    {
        string[] bookmarkedTabTypeStrings = EditorPrefs.GetString("tabify-bookmarked-tab-types").Split("---");

        // Use a dictionary for O(1) lookup instead of O(n) FirstOrDefault for each bookmark
        Dictionary<string, TabEntry> entriesByType = allEntries.ToDictionary(e => e.typeString, e => e);

        bookmarkedEntries = bookmarkedTabTypeStrings.
            Where(typeStr => !string.IsNullOrEmpty(value: typeStr) && entriesByType.ContainsKey(key: typeStr)).
            Select(typeStr => entriesByType[key: typeStr]).
            ToList();
    }

    private void SaveBookmarkedEntries()
    {
        IEnumerable<string> bookmarkedTabTypeStrings = bookmarkedEntries.Select(r => r.typeString);

        EditorPrefs.SetString("tabify-bookmarked-tab-types", string.Join("---", values: bookmarkedTabTypeStrings));
    }

    private List<TabEntry> bookmarkedEntries = new();

    private void OnEnable()
    {
        UpdateAllEntries();
        GetBookmarkedEntries();
    }

    private void OnDisable()
    {
        SaveBookmarkedEntries();
        Save();
    }

    private void UpdateSearch()
    {
        bool tryMatch(string name, string query, int[] matchIndexes, ref float cost)
        {
            var wordInitialsIndexes = new List<int> { 0 };

            for (var i = 1; i < name.Length; i++)
            {
                var separators = new[] { ' ', '-', '_', '.', '(', ')', '[', ']' };

                char prevChar = name[i - 1];
                char curChar = name[index: i];
                char nextChar = i + 1 < name.Length ? name[i + 1] : default;

                bool isSeparatedWordStart =
                    separators.Contains(value: prevChar) && !separators.Contains(value: curChar);
                bool isCamelcaseHump = (curChar.IsUpper() && prevChar.IsLower()) ||
                                       (curChar.IsUpper() && nextChar.IsLower());
                bool isNumberStart = curChar.IsDigit() && (!prevChar.IsDigit() || prevChar == '0');
                bool isAfterNumber = prevChar.IsDigit() && !curChar.IsDigit();

                if (isSeparatedWordStart || isCamelcaseHump || isNumberStart || isAfterNumber)
                    wordInitialsIndexes.Add(item: i);
            }

            var nextWordInitialsIndexMap = new int[name.Length];

            var nextWordIndex = 0;

            for (var i = 0; i < name.Length; i++)
            {
                if (i == wordInitialsIndexes[index: nextWordIndex])
                    if (nextWordIndex + 1 < wordInitialsIndexes.Count)
                        nextWordIndex++;
                    else break;

                nextWordInitialsIndexMap[i] = wordInitialsIndexes[index: nextWordIndex];
            }

            var iName = 0;
            var iQuery = 0;

            int prevMatchIndex = -1;

            cost = 0;

            while (iName < name.Length && iQuery < query.Length)
            {
                char curQuerySymbol = query[index: iQuery].ToLower();
                char curNameSymbol = name[index: iName].ToLower();

                if (curNameSymbol == curQuerySymbol)
                {
                    int gapLength = iName - prevMatchIndex - 1;

                    cost += gapLength;

                    // Register match
                    matchIndexes[iQuery] = iName;
                    iQuery++;
                    iName = iName + 1;
                    prevMatchIndex = iName - 1;

                    continue;

                    // consecutive matches cost 0
                    // distance between index 0 and first match also counts as a gap
                }

                int nextWordInitialIndex =
                    nextWordInitialsIndexMap[iName]; // wordInitialsIndexes.FirstOrDefault(i => i > iName);
                char nextWordInitialSymbol =
                    nextWordInitialIndex == default ? default : name[index: nextWordInitialIndex].ToLower();

                if (nextWordInitialSymbol == curQuerySymbol)
                {
                    int gapLength = nextWordInitialIndex - prevMatchIndex - 1;

                    cost += (gapLength * .01f).ClampMax(.9f);

                    // Register match
                    matchIndexes[iQuery] = nextWordInitialIndex;
                    iQuery++;
                    iName = nextWordInitialIndex + 1;
                    prevMatchIndex = nextWordInitialIndex;

                    continue;

                    // word-initial match costs less than a gap (1+)
                    // but more than a consecutive match (0)
                }

                iName++;
            }

            bool allCharsMatched = iQuery >= query.Length;

            return allCharsMatched;

            // this search works great in practice
            // but fails in more theoretical scenarios, mostly when user skips first letters of words
            // eg searching "arn" won't find "barn_a" because search will jump to last a (word-initial) and fail afterwards
            // so unity search is used as a fallback
        }

        bool tryMatch_unitySearch(string name, string query, int[] matchIndexes, ref float cost)
        {
            long score = 0;

            List<int> matchIndexesList = new();

            bool matched = FuzzySearch.FuzzyMatch(pattern: searchString, origin: name, outScore: ref score,
                matches: matchIndexesList);

            for (var i = 0; i < matchIndexesList.Count; i++)
                matchIndexes[i] = matchIndexesList[index: i];

            cost = 123212 - score;

            return matched;

            // this search is fast but isn't tuned for real use cases
            // quering "vis" ranks "Invisible" higher than "VInspectorState"
            // quering "lst" ranks "SmallShadowTemp" higher than "List"
            // also sometimes it favors matches that are further away from zeroth index
        }

        string formatName(string name, IEnumerable<int> matchIndexes)
        {
            var formattedName = "";

            for (var i = 0; i < name.Length; i++)
                if (matchIndexes.Contains(value: i))
                    formattedName += "<b>" + name[index: i] + "</b>";
                else
                    formattedName += name[index: i];

            return formattedName;
        }

        var costs_byEntry = new Dictionary<TabEntry, float>();

        var matchIndexes = new int[searchString.Length];
        var matchCost = 0f;

        foreach (TabEntry entry in allEntries)
            if (tryMatch(name: entry.name, query: searchString, matchIndexes: matchIndexes, cost: ref matchCost) ||
                tryMatch_unitySearch(name: entry.name, query: searchString, matchIndexes: matchIndexes,
                    cost: ref matchCost))
            {
                costs_byEntry[key: entry] = matchCost;
                namesFormattedForFuzzySearch_byEntry[key: entry] =
                    formatName(name: entry.name, matchIndexes: matchIndexes);
            }

        searchedEntries = costs_byEntry.Keys.OrderBy(r => costs_byEntry[key: r]).ThenBy(r => r.name).ToList();
    }

    private List<TabEntry> searchedEntries = new();

    private readonly Dictionary<TabEntry, string> namesFormattedForFuzzySearch_byEntry = new();

    private void OnLostFocus()
    {
        EditorApplication.delayCall += () =>
        {
            if (focusedWindow != this)
            {
                dockArea.GetMemberValue<EditorWindow>("actualView").Repaint(); // for + button to fade
                Close();
            }
        };

        // delay is needed to prevent reopening after clicking + button for the second time
    }

    public static void Open(Object dockArea)
    {
        instance = CreateInstance<TabifyAddTabWindow>();

        instance.ShowPopup();
        instance.Focus();

        TabifyGUI gui = guis_byDockArea[key: dockArea];

        var windowRect = dockArea.GetMemberValue("actualView").GetMemberValue<Rect>("position");

        Vector2 lastTabEndPosition = windowRect.position +
                                     Vector2.right * gui.tabEndPositions.Last().ClampMax(windowRect.width - 30);

        var width = 161;
        var height = 276;

        int offsetX = -26;
        var offsetY = 24;

        instance.position = instance.position.SetPos(lastTabEndPosition + new Vector2(x: offsetX, y: offsetY)).
            SetSize(w: width, h: height);

        instance.dockArea = dockArea;

        UpdateAllEntries();
    }

    public Object dockArea;

    public static TabifyAddTabWindow instance;
}
#endif
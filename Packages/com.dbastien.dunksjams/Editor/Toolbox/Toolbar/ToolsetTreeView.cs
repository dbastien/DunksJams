using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

/// <summary>TreeView for displaying and selecting toolsets.</summary>
public class ToolsetTreeView : TreeView
{
    List<ToolsetLibrary.ToolsetInfo> _infos = new();
    List<int> _selections = new();

    public List<int> Selections => _selections;
    public List<ToolsetLibrary.ToolsetInfo> Infos { get => _infos; set => _infos = value ?? new(); }

    public ToolsetTreeView(TreeViewState state) : base(state)
    {
        showAlternatingRowBackgrounds = true;
        Reload();
    }

    protected override TreeViewItem BuildRoot() => new TreeViewItem { id = 0, depth = -1 };

    protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
    {
        var rows = GetRows() ?? new List<TreeViewItem>(200);
        rows.Clear();
        for (var i = 0; i < _infos.Count; ++i)
        {
            var item = new TreeViewItem(i, -1, _infos[i].displayName);
            root.AddChild(item);
            rows.Add(item);
        }
        SetupDepthsFromParentsAndChildren(root);
        return rows;
    }

    protected override void SelectionChanged(IList<int> selectedIds)
    {
        _selections.Clear();
        _selections.AddRange(selectedIds);
    }
}

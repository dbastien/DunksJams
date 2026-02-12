using System;
using System.Collections.Generic;
using UnityEditor;

/// <summary>Library of all available toolsets discovered via reflection.</summary>
public class ToolsetLibrary
{
    public class ToolsetInfo
    {
        public Type type;
        public string displayName = "";
        public string fullName = "";
        public string description = "";
    }

    List<ToolsetInfo> _toolsetInfos;

    public List<ToolsetInfo> ToolsetInfos => _toolsetInfos;

    public ToolsetLibrary() => _toolsetInfos = new List<ToolsetInfo>();

    public void Setup()
    {
        _toolsetInfos.Clear();
        var types = TypeCache.GetTypesDerivedFrom<IToolset>();
        for (var i = 0; i < types.Count; ++i)
        {
            var type = types[i];
            var attrs = type.GetCustomAttributes(typeof(ToolsetProvider), true);
            if (attrs.Length == 0) continue;

            var provider = (ToolsetProvider)attrs[0];
            _toolsetInfos.Add(new ToolsetInfo
            {
                type = type,
                displayName = provider.displayName ?? type.Name,
                fullName = type.FullName ?? type.Name,
                description = provider.description ?? ""
            });
        }
    }

    public void Teardown()
    {
        _toolsetInfos?.Clear();
        _toolsetInfos = null;
    }

    public ToolsetInfo GetToolsetInfo(Type type) => _toolsetInfos?.Find(t => t.type == type);

    public ToolsetInfo GetToolsetInfo(string fullName) => _toolsetInfos?.Find(t => t.type.FullName == fullName);

    public IToolset CreateToolset(string fullName) => CreateToolset(GetToolsetInfo(fullName));

    public IToolset CreateToolset(ToolsetInfo info)
    {
        if (info?.type == null) return null;
        var toolset = (IToolset)Activator.CreateInstance(info.type);
        toolset.Setup();
        return toolset;
    }
}

using System;
using System.Collections.Generic;

public sealed class ScoreboardSpec
{
    public static readonly ScoreboardSpec Empty = new(Array.Empty<ScoreFieldDef>());

    public IReadOnlyList<ScoreFieldDef> Fields { get; }
    public int Count => Fields.Count;

    public ScoreboardSpec(IReadOnlyList<ScoreFieldDef> fields) => Fields = fields ?? Array.Empty<ScoreFieldDef>();

    public bool TryGetField(string id, out ScoreFieldDef field)
    {
        for (var i = 0; i < Fields.Count; ++i)
        {
            if (Fields[i].Id != id) continue;
            field = Fields[i];
            return true;
        }

        field = default;
        return false;
    }
}
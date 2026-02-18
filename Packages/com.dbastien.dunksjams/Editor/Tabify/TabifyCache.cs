#if UNITY_EDITOR

#region

using System;
using System.Collections.Generic;
using UnityEditor;

#endregion

[FilePath("Library/Tabify Cache.asset", location: FilePathAttribute.Location.ProjectFolder)]
public class TabifyCache : ScriptableSingleton<TabifyCache>
{
    public List<TabEntry> allTabEntries = new();

    [Serializable]
    public class TabEntry
    {
        public string name = "";
        public string iconName = "";
        public string typeString = "";
    }

    public static void Save() => instance.Save(true);
    public static void Clear() => instance.allTabEntries.Clear();
}
#endif
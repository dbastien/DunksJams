#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[Serializable]
public struct GlobalID : IEquatable<GlobalID>
{
    public UnityEngine.Object GetObject() => GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);

    public int GetObjectInstanceId() => ReflectionUtils.InvokeMethod<int>(typeof(GlobalObjectId),
        "_GlobalObjectId_GlobalObjectIdentifierToInstanceIDSlow", globalObjectId);

    public int idType => globalObjectId.identifierType;
    public string guid => globalObjectId.assetGUID.ToString();
    public ulong fileId => globalObjectId.targetObjectId;
    public ulong prefabId => globalObjectId.targetPrefabId;

    public bool isNull => globalObjectId.identifierType == 0;
    public bool isAsset => globalObjectId.identifierType == 1;
    public bool isSceneObject => globalObjectId.identifierType == 2;

    public GlobalObjectId globalObjectId =>
        _globalObjectId.Equals(default) &&
        globalObjectIdString != null &&
        GlobalObjectId.TryParse(globalObjectIdString, out GlobalObjectId r)
            ? _globalObjectId = r
            : _globalObjectId;

    public GlobalObjectId _globalObjectId;

    public GlobalID
        (UnityEngine.Object o) =>
        globalObjectIdString = (_globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(o)).ToString();

    public GlobalID(string s) => globalObjectIdString = GlobalObjectId.TryParse(s, out _globalObjectId) ? s : s;

    public string globalObjectIdString;

    public bool Equals(GlobalID other) => globalObjectIdString.Equals(other.globalObjectIdString);

    public static bool operator ==(GlobalID a, GlobalID b) => a.Equals(b);
    public static bool operator !=(GlobalID a, GlobalID b) => !a.Equals(b);

    public override bool Equals(object other) => other is GlobalID otherglobalID && Equals(otherglobalID);
    public override int GetHashCode() => globalObjectIdString == null ? 0 : globalObjectIdString.GetHashCode();

    public override string ToString() => globalObjectIdString;

    public GlobalID UnpackForPrefab()
    {
        ulong unpackedFileId = (fileId ^ prefabId) & 0x7fffffffffffffff;
        var unpackedGId = new GlobalID($"GlobalObjectId_V1-{idType}-{guid}-{unpackedFileId}-0");
        return unpackedGId;
    }

    public static GlobalID GetForPrefabStageObject(UnityEngine.Object o)
    {
        if (UnityEditor.SceneManagement.StageUtility.GetCurrentStage() is not UnityEditor.SceneManagement.PrefabStage
            prefabStage)
        {
            Debug.LogError("GetForPrefabAssetObject() got called outside of prefab stage!");
            return o.GetGlobalID();
        }

        GlobalID rawGlobalId = o.GetGlobalID();

#if UNITY_2023_2_OR_NEWER
        var so = new SerializedObject(o);
        so.SetPropertyValue("inspectorMode", InspectorMode.Debug);
        long rawFileId = so.FindProperty("m_LocalIdentfierInFile").longValue;

        if (rawFileId == 0)
            rawFileId = (long)typeof(Editor).Assembly.GetType("UnityEditor.Unsupported").
                InvokeMethod<ulong>("GetOrGenerateFileIDHint", o);
#else
        var rawFileId = rawGlobalId.fileId;
#endif

        long fileId = ((long)rawFileId ^ (long)rawGlobalId.globalObjectId.targetPrefabId) & 0x7fffffffffffffff;
        string prefabGuid = prefabStage.assetPath.ToGuid();
        var sourceGlobalId = new GlobalID($"GlobalObjectId_V1-1-{prefabGuid}-{fileId}-0");

        return sourceGlobalId;
    }
}

public static class GlobalIDExtensions
{
    public static GlobalID GetGlobalID(this UnityEngine.Object o) => new(o);

    public static GlobalID[] GetGlobalIDs(this IEnumerable<int> instanceIds)
    {
        var unityGlobalIds = new GlobalObjectId[instanceIds.Count()];
        ReflectionUtils.InvokeMethod(typeof(GlobalObjectId), "_GlobalObjectId_GetGlobalObjectIdsSlow",
            instanceIds.ToArray(), unityGlobalIds);
        IEnumerable<GlobalID> globalIds = unityGlobalIds.Select(r => new GlobalID(r.ToString()));
        return globalIds.ToArray();
    }

    public static UnityEngine.Object[] GetObjects(this IEnumerable<GlobalID> globalIDs)
    {
        GlobalObjectId[] goids = globalIDs.Select(r => r.globalObjectId).ToArray();
        var objects = new UnityEngine.Object[goids.Length];
        GlobalObjectId.GlobalObjectIdentifiersToObjectsSlow(goids, objects);
        return objects;
    }

    public static int[] GetObjectInstanceIds(this IEnumerable<GlobalID> globalIDs)
    {
        GlobalObjectId[] goids = globalIDs.Select(r => r.globalObjectId).ToArray();
        var iids = new int[goids.Length];
        ReflectionUtils.InvokeMethod(typeof(GlobalObjectId), "_GlobalObjectId_GlobalObjectIdentifiersToInstanceIDsSlow",
            goids, iids);
        return iids;
    }
}
#endif
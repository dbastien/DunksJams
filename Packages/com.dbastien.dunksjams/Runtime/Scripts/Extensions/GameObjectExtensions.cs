using UnityEngine;

public static class GameObjectExtensions
{
    public static T FindOrAddComponent<T>(this GameObject go) where T : Component =>
        go.TryGetComponent(out T comp) ? comp : go.AddComponent<T>();

    public static void SetLayerRecursively(this GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            child.gameObject.SetLayerRecursively(layer);
    }

    public static void SetTagRecursively(this GameObject go, string tag)
    {
        go.tag = tag;
        foreach (Transform child in go.transform)
            child.gameObject.SetTagRecursively(tag);
    }

    public static GameObject Clone(this GameObject go, Transform parent = null)
    {
        GameObject clone = Object.Instantiate(go, parent);
        clone.name = go.name;
        return clone;
    }

    public static void SetActiveSafe(this GameObject go, bool active)
    {
        if (go && go.activeSelf != active) go.SetActive(active);
    }

    public static bool MatchesLayerMask(this GameObject go, LayerMask mask) => (mask & (1 << go.layer)) != 0;

    public static bool HasComponent<T>(this GameObject go) where T : Component => go.TryGetComponent<T>(out _);

    public static void DisableAllRenderers(this GameObject go)
    {
        foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
            renderer.enabled = false;
    }

    public static void EnableAllRenderers(this GameObject go)
    {
        foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
            renderer.enabled = true;
    }

    public static void ToggleActive(this GameObject go) =>
        go.SetActive(!go.activeSelf);

    public static void DestroyWithDelay(this GameObject go, float delay)
    {
        if (Application.isPlaying)
            Object.Destroy(go, delay);
        else
            Object.DestroyImmediate(go);
    }

    public static string GetFullPath(this GameObject go)
    {
        string path = "/" + go.name;
        while (go.transform.parent != null)
        {
            go = go.transform.parent.gameObject;
            path = "/" + go.name + path;
        }

        return path;
    }
}
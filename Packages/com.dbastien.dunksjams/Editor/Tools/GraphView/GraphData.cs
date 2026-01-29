using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GraphData
{
    public List<NodeData> nodes = new();
    public List<EdgeData> edges = new();
}

[Serializable]
public class NodeData
{
    public string guid;
    public string nodeType;
    public Vector2 pos;
    public List<FieldData> fields = new();
}

[Serializable]
public class EdgeData
{
    public string sourceNodeGUID;
    public string sourcePortName;
    public string targetNodeGUID;
    public string targetPortName;
}

[Serializable]
public class FieldData
{
    public string name;
    public string val;
}
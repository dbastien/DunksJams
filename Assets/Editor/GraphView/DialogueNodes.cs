using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class DialogueNodeBase : SerializedGraphNode, IPropagatingNode
{
    public abstract override void PropagateData();
}

public class DialogueNode : DialogueNodeBase
{
    public override void PropagateData() { }
}

public class DialogueNodeChoice : DialogueNode
{
    int _choiceCount;

    public override void Init(Vector2 pos, Vector2 size = default)
    {
        base.Init(pos);
        CreatePort<string>("In", Direction.Input);
        AddChoicePort("Option 1");
        titleContainer.Add(new Button(() => AddChoicePort($"Option {_choiceCount + 1}")) { text = "Add Choice" });
    }

    public DataPort<string> AddChoicePort(string port)
    {
        var choicePort = CreatePort<string>(port, Direction.Output, RemoveChoicePort);
        choicePort.SetData(port);
        ++_choiceCount;
        Refresh();
        return choicePort;
    }

    void RemoveChoicePort(Port port)
    {
        outputContainer.Remove(port);
        --_choiceCount;
        Refresh();
    }

    public override void PropagateData()
    {
        foreach (var port in outputContainer.Children().OfType<DataPort<string>>())
            foreach (Edge con in port.connections)
                if (con.input is IDataPort<string> input)
                    input.SetData(port.GetData()); 
    }
}

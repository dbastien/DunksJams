using UnityEngine;

public class TestCompilationWarnings : MonoBehaviour
{
    // CS0168: Variable declared but never used
    int unusedVariable;

    // CS0162: Unreachable code detected
    void Start()
    {
        return;
        DLog.Log("This code is unreachable");
    }

    // CS0219: Variable assigned but never used
    void Update()
    {
        var assignedButUnused = 42;
    }

    // CS0649: Field is never assigned to
    public int unassignedField;

    // CS0618: Obsolete API usage
    void TestObsolete()
    {
        // This will generate a warning about using obsolete Unity API
        Application.LoadLevel(0);
    }
}
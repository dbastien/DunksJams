using UnityEngine;
using UnityEditor;

public class DLogConsoleTest
{
    [MenuItem("Tools/DLog/Test All Features")]
    public static void TestAllFeatures()
    {
        Debug.Log("=== DLog Console Test Suite ===");

        // Test basic logging
        Debug.Log("DLog Console Test: Info message");
        Debug.LogWarning("DLog Console Test: Warning message");
        Debug.LogError("DLog Console Test: Error message");

        // Test the manual compilation warning check
        DLogConsole.ManualCheckCompilationWarnings();

        // Test the test warning method
        DLogConsole.TestLogWarning();

        Debug.Log("=== Test Complete - Check DLog Console Window ===");
        Debug.Log("You should see:");
        Debug.Log("- Info, Warning, and Error messages from this test");
        Debug.Log("- Any compilation warnings if they exist");
        Debug.Log("- Test warning from the menu method");

        // Open the DLog Console window
        DLogConsole.ShowWindow();
    }

    [MenuItem("Tools/DLog/Test Compilation Warnings Only")]
    public static void TestCompilationWarnings()
    {
        Debug.Log("Testing compilation warning capture...");
        DLogConsole.ManualCheckCompilationWarnings();
    }

    [MenuItem("Tools/DLog/Clear Console")]
    public static void ClearConsole()
    {
        // This will clear the DLog Console
        var window = EditorWindow.GetWindow<DLogConsole>();
        if (window != null)
        {
            // The clear functionality is in the OnGUI method
            Debug.Log("DLog Console cleared via test menu");
        }
    }
}
using UnityEditor;

public class DLogConsoleTest
{
    [MenuItem("‽/DLog/Test All Features")]
    public static void TestAllFeatures()
    {
        DLog.Log("=== DLog Console Test Suite ===");

        // Test basic logging
        DLog.Log("DLog Console Test: Info message");
        DLog.LogW("DLog Console Test: Warning message");
        DLog.LogE("DLog Console Test: Error message");

        // Test the manual compilation warning check
        DLogConsole.ManualCheckCompilationWarnings();

        // Test the test warning method
        DLogConsole.TestLogWarning();

        DLog.Log("=== Test Complete - Check DLog Console Window ===");
        DLog.Log("You should see:");
        DLog.Log("- Info, Warning, and Error messages from this test");
        DLog.Log("- Any compilation warnings if they exist");
        DLog.Log("- Test warning from the menu method");

        // Open the DLog Console window
        DLogConsole.ShowWindow();
    }

    [MenuItem("‽/DLog/Test Compilation Warnings Only")]
    public static void TestCompilationWarnings()
    {
        DLog.Log("Testing compilation warning capture...");
        DLogConsole.ManualCheckCompilationWarnings();
    }
}
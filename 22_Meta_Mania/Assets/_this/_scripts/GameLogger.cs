using UnityEngine;
using System.IO;
using System.Text;

/// <summary>
/// Game-wide logging utility that captures all Unity Debug.Log calls to a file
/// Automatically logs all Debug.Log, Debug.LogWarning, Debug.LogError messages
/// </summary>
public class GameLogger : MonoBehaviour
{
    [Header("Logging Settings")]
    [Tooltip("Enable file logging for all Unity log messages")]
    public bool enableFileLogging = true;

    [Tooltip("Log file name (saved in persistentDataPath)")]
    public string logFileName = "game_log.txt";

    [Tooltip("Also log to file in builds (not just editor)")]
    public bool logInBuilds = true;

    [Header("Filter Settings")]
    [Tooltip("Log normal Debug.Log messages")]
    public bool logNormalMessages = true;

    [Tooltip("Log Debug.LogWarning messages")]
    public bool logWarnings = true;

    [Tooltip("Log Debug.LogError and exceptions")]
    public bool logErrors = true;

    private string logFilePath;
    private StreamWriter logWriter;

    private void Awake()
    {
        if (!enableFileLogging) return;

        // Only log in editor or if logInBuilds is enabled
        if (!Application.isEditor && !logInBuilds) return;

        logFilePath = Path.Combine(Application.persistentDataPath, logFileName);

        try
        {
            // Clear old log and create new one
            logWriter = new StreamWriter(logFilePath, false);
            logWriter.AutoFlush = true;

            // Write session header
            WriteToFile("================================");
            WriteToFile("=== GAME SESSION STARTED ===");
            WriteToFile($"Time: {System.DateTime.Now}");
            WriteToFile($"Unity Version: {Application.unityVersion}");
            WriteToFile($"Platform: {Application.platform}");
            WriteToFile($"Build: {(Application.isEditor ? "Editor" : "Build")}");
            WriteToFile($"Log file: {logFilePath}");
            WriteToFile("================================\n");

            // Hook into Unity's logging system
            Application.logMessageReceived += HandleLog;

            Debug.Log($"<color=cyan>[GameLogger]</color> Log file created at: {logFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameLogger] Failed to initialize: {e.Message}");
            enableFileLogging = false;
        }
    }

    private void OnDestroy()
    {
        if (enableFileLogging)
        {
            Application.logMessageReceived -= HandleLog;

            if (logWriter != null)
            {
                WriteToFile("\n================================");
                WriteToFile("=== GAME SESSION ENDED ===");
                WriteToFile($"Time: {System.DateTime.Now}");
                WriteToFile("================================");

                logWriter.Close();
                logWriter = null;
            }
        }
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (!enableFileLogging || logWriter == null) return;

        // Filter based on log type
        bool shouldLog = false;
        string prefix = "";

        switch (type)
        {
            case LogType.Log:
                shouldLog = logNormalMessages;
                prefix = "[LOG]";
                break;
            case LogType.Warning:
                shouldLog = logWarnings;
                prefix = "[WARNING]";
                break;
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                shouldLog = logErrors;
                prefix = type == LogType.Exception ? "[EXCEPTION]" : "[ERROR]";
                break;
        }

        if (!shouldLog) return;

        try
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
            StringBuilder logEntry = new StringBuilder();
            logEntry.AppendLine($"[{timestamp}] {prefix} {logString}");

            // Add stack trace for errors and exceptions
            if ((type == LogType.Error || type == LogType.Exception) && !string.IsNullOrEmpty(stackTrace))
            {
                logEntry.AppendLine($"  Stack Trace: {stackTrace}");
            }

            WriteToFile(logEntry.ToString());
        }
        catch (System.Exception e)
        {
            // If logging fails, disable it to prevent spam
            Debug.LogError($"[GameLogger] Failed to write log: {e.Message}");
            enableFileLogging = false;
        }
    }

    private void WriteToFile(string message)
    {
        logWriter?.WriteLine(message);
    }

    /// <summary>
    /// Get the full path to the log file
    /// </summary>
    public string GetLogFilePath()
    {
        return logFilePath;
    }
}

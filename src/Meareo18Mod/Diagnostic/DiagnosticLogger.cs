using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Meareo18AmongUsMod.Diagnostic
{
    public static class DiagnosticLogger
    {
        private const string LogPrefix = "[MEAREO18-MOD]";
        private static string? _logFilePath;

        public static void Initialize(string? customLogPath = null)
        {
            _logFilePath = customLogPath;
            PrintBanner();
            LogSystemInfo();
        }

        public static void Info(string message) => WriteLog("INFO", message);
        public static void Warning(string message) => WriteLog("WARN", message);
        public static void Error(string message, Exception? ex = null)
            => WriteLog("ERROR", ex != null ? $"{message}\n{ex}" : message);

        private static void PrintBanner()
        {
            Console.WriteLine("\n=== MEAREO18 AMONG US MOD v0.1.0-alpha (Android ARM64) ===");
        }

        private static void LogSystemInfo()
        {
            Info($"OS: {RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})");
            Info($".NET: {RuntimeInformation.FrameworkDescription}");
        }

        [DllImport("log", EntryPoint = "__android_log_print", CallingConvention = CallingConvention.Cdecl)]
        private static extern int AndroidLogPrint(int priority, string tag, string fmt, string msg);

        private static void WriteLog(string level, string message)
        {
            string line = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} {LogPrefix} [{level}] {message}";
            Console.WriteLine(line);
            TryAndroidLogcat(level, message);
            if (!string.IsNullOrEmpty(_logFilePath))
                TryAppendToFile(_logFilePath, line);
        }

        private static void TryAndroidLogcat(string level, string message)
        {
            try
            {
                int priority = level == "ERROR" ? 6 : level == "WARN" ? 5 : 4;
                AndroidLogPrint(priority, "MEAREO18", "%s", message);
            }
            catch { /* Silencioso en Windows */ }
        }

        private static bool TryAppendToFile(string path, string content)
        {
            try
            {
                string? dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.AppendAllText(path, content + Environment.NewLine);
                return true;
            }
            catch { return false; }
        }
    }
}

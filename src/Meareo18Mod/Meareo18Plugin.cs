using System;
using Meareo18AmongUsMod.Diagnostic;

namespace Meareo18AmongUsMod
{
    public static class ModConstants
    {
        public const string Id = "com.meareo18.amongusmod";
        public const string Name = "Meareo18 Among Us Mod";
        public const string Version = "0.1.0";
        public const string Author = "Meareo18 Community Dev";
    }

    /// <summary>
    /// Punto de entrada principal del mod Meareo18.
    /// NOTA: Actualmente es una clase C# normal. Para que un cargador (BepInEx/FusionCore)
    /// la reconozca, debe heredar de BasePlugin y llevar el atributo [BepInPlugin].
    /// Esto requiere tener las DLLs del framework en libs/.
    /// </summary>
    public class Meareo18Plugin
    {
        public static Meareo18Plugin? Instance { get; private set; }
        public bool IsInitialized { get; private set; }

        public Meareo18Plugin()
        {
            Instance = this;
        }

        public void Load()
        {
            try
            {
                DiagnosticLogger.Initialize();
                DiagnosticLogger.Info($"[LIFECYCLE] Cargando {ModConstants.Name} v{ModConstants.Version}...");
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                IsInitialized = true;
                DiagnosticLogger.Info("[LIFECYCLE] Plugin inicializado (lógica C# únicamente).");
            }
            catch (Exception ex)
            {
                DiagnosticLogger.Error("[LIFECYCLE] Error fatal:", ex);
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                DiagnosticLogger.Error("[UNHANDLED]", ex);
        }
    }
}

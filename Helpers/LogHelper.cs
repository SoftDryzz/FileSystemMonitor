using System;
using System.IO;
using FileSystemMonitor.Models;

namespace FileSystemMonitor.Helpers
{
    public static class LogHelper
    {
        private static string logFilePath = GetLogFilePath();

        public static string GetLogFilePath()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string logDirectory = Path.Combine(desktopPath, "Registro de monitoreo");

            // Crear la carpeta si no existe
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Generar un nuevo archivo de log cada día
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            return Path.Combine(logDirectory, $"MonitorArchivos_{date}.log");
        }

        public static void SaveLogEntry(LogEntry entry)
        {
            try
            {
                string logPath = GetLogFilePath(); // Asegura que se usa el log del día
                File.AppendAllText(logPath, entry.ToString() + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al escribir en el log: {ex.Message}");
            }
        }

        public static void ClearLog()
        {
            try
            {
                string logPath = GetLogFilePath();
                File.WriteAllText(logPath, string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al borrar el log: {ex.Message}");
            }
        }
    }
}

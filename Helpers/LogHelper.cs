using System;
using System.Collections.Generic;
using System.IO;
using FileSystemMonitor.Models;

namespace FileSystemMonitor.Helpers
{
    public static class LogHelper
    {
        private static string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MonitorArchivos.log");

        // Guardar un evento en el log
        public static void SaveLogEntry(LogEntry entry)
        {
            try
            {
                File.AppendAllText(logFilePath, entry.ToString() + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al escribir en el log: {ex.Message}");
            }
        }

        // Leer todos los registros del log
        public static List<string> ReadLogEntries()
        {
            try
            {
                return File.Exists(logFilePath) ? new List<string>(File.ReadAllLines(logFilePath)) : new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer el log: {ex.Message}");
                return new List<string>();
            }
        }

        // Borrar el log
        public static void ClearLog()
        {
            try
            {
                File.WriteAllText(logFilePath, string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al borrar el log: {ex.Message}");
            }
        }
    }
}

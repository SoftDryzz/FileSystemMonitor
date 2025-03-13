using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileSystemMonitor.Models;
using FileSystemMonitor.Helpers;

namespace FileSystemMonitor.Services
{
    public class FileMonitorService
    {
        private List<System.IO.FileSystemWatcher> watchers = new List<System.IO.FileSystemWatcher>();
        private HashSet<string> copiedFiles = new HashSet<string>();
        private Dictionary<string, DateTime> recentEvents = new Dictionary<string, DateTime>();
        private TimeSpan eventCooldown = TimeSpan.FromSeconds(2);
        private bool filterUserOnly = false;

        // Rutas del sistema a ignorar si el usuario lo activa
        private readonly HashSet<string> systemPaths = new()
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
        };

        public event Action<LogEntry> OnLogEntryCreated;

        public void StartMonitoring(bool userOnly)
        {
            filterUserOnly = userOnly;

            if (watchers.Count > 0) // ⚠️ Evita detener el monitoreo antes de iniciarlo
                StopMonitoring();

            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                var watcher = new System.IO.FileSystemWatcher
                {
                    Path = drive.RootDirectory.FullName,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true
                };

                watcher.Created += OnCreated;
                watcher.Changed += OnChanged;
                watcher.Deleted += OnDeleted;
                watcher.Renamed += OnRenamed;

                watchers.Add(watcher);
            }

            OnLogEntryCreated?.Invoke(new LogEntry { Timestamp = DateTime.Now, EventType = "📡 Monitoreo iniciado...", FilePath = "Sistema" });
        }

        public void StopMonitoring()
        {
            if (watchers.Count == 0) return; // ⚠️ No mostrar "Monitoreo detenido" si no hay monitoreo activo

            foreach (var watcher in watchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            watchers.Clear();

            OnLogEntryCreated?.Invoke(new LogEntry { Timestamp = DateTime.Now, EventType = "❌ Monitoreo detenido.", FilePath = "Sistema" });
        }



        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (ShouldIgnoreEvent(e.FullPath)) return;

            // 📌 Evitar eventos repetitivos en poco tiempo
            if (recentEvents.TryGetValue(e.FullPath, out DateTime lastEventTime) &&
                (DateTime.Now - lastEventTime) < eventCooldown)
                return;

            recentEvents[e.FullPath] = DateTime.Now;

            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                EventType = "✏️ Modificado",
                FilePath = e.FullPath
            };

            LogHelper.SaveLogEntry(entry);
            OnLogEntryCreated?.Invoke(entry);
        }


        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (ShouldIgnoreEvent(e.FullPath))
                return;

            string originalFile = DetectCopiedFile(e.FullPath);
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                EventType = !string.IsNullOrEmpty(originalFile) ? "📑 Copiado" : "📂 Creado",
                FilePath = e.FullPath
            };

            if (!string.IsNullOrEmpty(originalFile))
            {
                entry.EventType = "📑 Copiado";
                entry.OldFilePath = originalFile;

                // 🔹 Agregar ruta del archivo original
                entry.FilePath += $" (Duplicado de: {Path.GetFileName(originalFile)} | 📁 Ruta original: {originalFile})";
            }

            LogHelper.SaveLogEntry(entry);
            OnLogEntryCreated?.Invoke(entry);
        }


        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (ShouldIgnoreEvent(e.FullPath))
                return;

            copiedFiles.Remove(e.FullPath);

            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                EventType = IsRecycleBinPath(e.FullPath) ? "🗑️ Enviado a Papelera" : "🚮 Eliminado",
                FilePath = e.FullPath
            };

            LogHelper.SaveLogEntry(entry);
            OnLogEntryCreated?.Invoke(entry);
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            if (ShouldIgnoreEvent(e.FullPath))
                return;

            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                EventType = "🔄 Renombrado",
                FilePath = e.FullPath,
                OldFilePath = e.OldFullPath
            };

            LogHelper.SaveLogEntry(entry);
            OnLogEntryCreated?.Invoke(entry);
        }


        private bool ShouldIgnoreEvent(string filePath)
        {
            return filePath.Equals(LogHelper.GetLogFilePath(), StringComparison.OrdinalIgnoreCase) ||
                   (filterUserOnly && systemPaths.Any(path => filePath.StartsWith(path, StringComparison.OrdinalIgnoreCase))) ||
                   IsRecycleBinPath(filePath);
        }

        private bool IsRecycleBinPath(string filePath)
        {
            return filePath.StartsWith("C:\\$Recycle.Bin", StringComparison.OrdinalIgnoreCase);
        }

        private string DetectCopiedFile(string newFilePath)
        {
            FileInfo newFile = new FileInfo(newFilePath);
            if (!newFile.Exists)
                return null;

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(newFile.Name);
            string extension = newFile.Extension;

            // 🔹 Posibles sufijos de copia en distintos idiomas
            string[] copySuffixes = { " - copia", " - copy", " - kopi", " - kopya" };

            // 🔹 Eliminar números en paréntesis (ejemplo: " - copia (2)")
            string cleanedFileName = System.Text.RegularExpressions.Regex.Replace(fileNameWithoutExtension, @"\s(\- copia|\- copy|\- kopi|\- kopya)(\s\(\d+\))?$", "");

            // 🏷 Buscar primero si hay una copia reciente que pueda ser el origen
            string latestCopy = FindLatestCopy(cleanedFileName, extension, newFile.DirectoryName);

            if (latestCopy != null && AreFilesIdentical(latestCopy, newFile.FullName))
                return latestCopy;

            // Si no encuentra una copia reciente, busca el original más antiguo
            string originalFile = FindOriginalFile(cleanedFileName + extension, newFile.DirectoryName);

            // 🔍 Verificar si realmente es una copia idéntica antes de asignarlo
            if (originalFile != null && AreFilesIdentical(originalFile, newFile.FullName))
                return originalFile;

            return null;
        }

        private string FindLatestCopy(string baseFileName, string extension, string directoryPath)
        {
            DirectoryInfo dir = new DirectoryInfo(directoryPath);
            var potentialCopies = dir.GetFiles()
                .Where(f => f.Name.StartsWith(baseFileName, StringComparison.OrdinalIgnoreCase) &&
                            IsCopiedFile(f.FullName))
                .OrderByDescending(f => f.LastWriteTime) // 📅 Ordenar por fecha de modificación más reciente
                .ToList();

            return potentialCopies.FirstOrDefault()?.FullName;
        }

        private bool IsCopiedFile(string filePath)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string[] copySuffixes = { " - copia", " - copy", " - kopi", " - kopya" };

            return copySuffixes.Any(suffix => fileNameWithoutExtension.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
        }



        private bool AreFilesIdentical(string path1, string path2)
        {
            FileInfo file1 = new FileInfo(path1);
            FileInfo file2 = new FileInfo(path2);

            if (file1.Length != file2.Length)
                return false;

            try
            {
                using (FileStream fs1 = new FileStream(path1, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (FileStream fs2 = new FileStream(path2, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] file1Bytes = new byte[file1.Length];
                    byte[] file2Bytes = new byte[file2.Length];

                    fs1.Read(file1Bytes, 0, file1Bytes.Length);
                    fs2.Read(file2Bytes, 0, file2Bytes.Length);

                    return file1Bytes.SequenceEqual(file2Bytes);
                }
            }
            catch (IOException)
            {
                return false;
            }
        }

        private string FindOriginalFile(string originalFileName, string directoryPath)
        {
            string originalFilePath = Path.Combine(directoryPath, originalFileName);
            return File.Exists(originalFilePath) ? originalFilePath : null;
        }
    }
}

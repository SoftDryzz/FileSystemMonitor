using System;

namespace FileSystemMonitor.Models
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string EventType { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string OldFilePath { get; set; } = string.Empty;

        public override string ToString()
        {
            return EventType == "Renombrado"
                ? $"{Timestamp:yyyy-MM-dd HH:mm:ss} | {EventType} | De: {OldFilePath} A: {FilePath}"
                : $"{Timestamp:yyyy-MM-dd HH:mm:ss} | {EventType} | {FilePath}";
        }
    }
}

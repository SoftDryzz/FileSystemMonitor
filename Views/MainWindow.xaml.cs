using System.Globalization;
using System.IO;
using System.Windows;

namespace FileSystemMonitor;


public partial class MainWindow : Window
{
    private List<System.IO.FileSystemWatcher> watchers = new List<System.IO.FileSystemWatcher>();
    private string logFilePath;
    private string language;
    private bool isMonitoringStarted = false;

    private Dictionary<string, DateTime> recentFiles = new Dictionary<string, DateTime>();
    private HashSet<string> copiedFiles = new HashSet<string>();
    private HashSet<string> ignoredFiles = new HashSet<string>();
    private TimeSpan copyDetectionWindow = TimeSpan.FromSeconds(5);
    private TimeSpan eventCooldown = TimeSpan.FromSeconds(2);
    private readonly Dictionary<string, DateTime> recentEvents = new Dictionary<string, DateTime>();

    public MainWindow()
    {
        InitializeComponent();

        string systemLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        language = systemLanguage == "es" ? "es" : "en";

        UpdateLogFilePath(); // Ahora primero se define el log en la carpeta correcta
        ignoredFiles.Add(logFilePath); // Se asegura que el archivo de log no se monitoree

        LoadLog();
        StopButton.IsEnabled = false;
    }



    private void UpdateLogFilePath()
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string logDirectory = Path.Combine(desktopPath, "Registro de monitoreo");

        // Crear la carpeta si no existe
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        // Definir el archivo de log con la fecha actual
        string date = DateTime.Now.ToString("yyyy-MM-dd");
        logFilePath = Path.Combine(logDirectory, $"MonitorArchivos_{date}.log");

        // Agregar tanto el archivo como la carpeta a la lista de ignorados
        ignoredFiles.Add(logFilePath);
        ignoredFiles.Add(logDirectory);
    }




    private void LoadLog()
    {
        if (File.Exists(logFilePath))
        {
            var lines = File.ReadAllLines(logFilePath);
            LogListBox.Items.Clear();
            foreach (var line in lines)
            {
                LogListBox.Items.Add(line);
            }
        }
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        StartMonitoring();
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        StopMonitoring();
    }

    private void ClearLogButton_Click(object sender, RoutedEventArgs e)
    {
        File.WriteAllText(logFilePath, string.Empty);
        LogListBox.Items.Clear();
    }

    private void StartMonitoring()
    {

        StartButton.IsEnabled = false; // 🔹 Deshabilitar botón Iniciar
        StopButton.IsEnabled = true;   // 🔹 Habilitar botón Detener

        foreach (var drive in DriveInfo.GetDrives())
        {
            if (drive.IsReady)
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
        }

        LogEvent("📡 Monitoreo iniciado...");
    }

    private void StopMonitoring()
    {
        foreach (var watcher in watchers)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
        watchers.Clear();

        LogEvent("❌ Monitoreo detenido.");

        StartButton.IsEnabled = true;  // 🔹 Habilitar botón Iniciar
        StopButton.IsEnabled = false;  // 🔹 Deshabilitar botón Detener
    }


    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (ShouldIgnoreEvent(e.FullPath))
            return;

        string originalFile = DetectCopiedFile(e.FullPath);
        string message;

        if (!string.IsNullOrEmpty(originalFile))
        {
            message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {(language == "es" ? "Copiado" : "Copied")} | {e.FullPath} " +
                      $"({(language == "es" ? "Duplicado de" : "Duplicated from")}: {Path.GetFileName(originalFile)})";
            copiedFiles.Add(e.FullPath);
        }
        else
        {
            message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {(language == "es" ? "Creado" : "Created")} | {e.FullPath}";
        }

        LogEvent(message);
    }



    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        if (ShouldIgnoreEvent(e.FullPath))
            return;

        copiedFiles.Remove(e.FullPath);
        LogEvent($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {(language == "es" ? "Eliminado" : "Deleted")} | {e.FullPath}");
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (ShouldIgnoreEvent(e.FullPath) || copiedFiles.Contains(e.FullPath))
            return;

        // Evitar que el mismo archivo genere múltiples eventos en poco tiempo
        if (recentEvents.TryGetValue(e.FullPath, out DateTime lastEventTime) &&
            (DateTime.Now - lastEventTime) < eventCooldown)
        {
            return; // Ignorar eventos repetitivos en el mismo archivo
        }

        recentEvents[e.FullPath] = DateTime.Now; // Actualizar tiempo del último evento

        LogEvent($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {(language == "es" ? "Modificado" : "Changed")} | {e.FullPath}");
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        if (ShouldIgnoreEvent(e.FullPath))
            return;

        // Evitar que el mismo archivo genere múltiples eventos en poco tiempo
        if (recentEvents.TryGetValue(e.FullPath, out DateTime lastEventTime) &&
            (DateTime.Now - lastEventTime) < eventCooldown)
        {
            return; // Ignorar eventos repetitivos en el mismo archivo
        }

        recentEvents[e.FullPath] = DateTime.Now; // Actualizar tiempo del último evento

        LogEvent($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {(language == "es" ? "Renombrado" : "Renamed")} | " +
                 $"{(language == "es" ? "De:" : "From:")} {e.OldFullPath} {(language == "es" ? "A:" : "To:")} {e.FullPath}");
    }

    // ✅ Función mejorada para ignorar eventos innecesarios
    // ✅ Ahora solo filtra archivos del sistema si el usuario ha marcado el CheckBox
    private bool ShouldIgnoreEvent(string filePath)
    {
        return filePath.Equals(logFilePath, StringComparison.OrdinalIgnoreCase) ||  // Ignorar el log actual
               ignoredFiles.Contains(filePath) ||  // Ignorar cualquier archivo en la lista
               IsRecycleBinPath(filePath) ||  // Ignorar la Papelera de Reciclaje
               systemPaths.Any(path => filePath.StartsWith(path, StringComparison.OrdinalIgnoreCase)); // Ignorar archivos del sistema
    }




    // Lista de rutas del sistema a ignorar
    private readonly HashSet<string> systemPaths = new()
{
    Environment.GetFolderPath(Environment.SpecialFolder.Windows),         // C:\Windows
    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),    // C:\Program Files
    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), // C:\Program Files (x86)
    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), // C:\ProgramData
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),  // C:\Users\...\AppData\Local
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)  // C:\Users\...\AppData\Roaming
};


    // ✅ Método para detectar si el archivo pertenece a la papelera de reciclaje
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

        string[] copySuffixes = { " - copia", " - copy", " - kopi", " - kopya" };

        foreach (var suffix in copySuffixes)
        {
            if (fileNameWithoutExtension.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                string originalFileName = fileNameWithoutExtension[..^suffix.Length] + extension;
                string originalFile = FindOriginalFile(originalFileName, newFile.DirectoryName);

                // Compara el tamaño de los archivos para verificar si realmente es una copia
                if (originalFile != null && AreFilesIdentical(originalFile, newFile.FullName))
                    return originalFile;
            }
        }

        return null;
    }
    private bool AreFilesIdentical(string path1, string path2)
    {
        FileInfo file1 = new FileInfo(path1);
        FileInfo file2 = new FileInfo(path2);

        if (file1.Length != file2.Length) return false; // Si los tamaños son distintos, no son copias

        // Comparar byte por byte si es necesario
        return File.ReadAllBytes(path1).SequenceEqual(File.ReadAllBytes(path2));
    }


    private string FindOriginalFile(string originalFileName, string directoryPath)
    {
        string originalFilePath = Path.Combine(directoryPath, originalFileName);
        return File.Exists(originalFilePath) ? originalFilePath : null;
    }


    // ✅ Detecta archivos del sistema correctamente sin afectar el monitoreo
    private bool IsSystemGeneratedEvent(string filePath)
    {
        return systemPaths.Any(path => filePath.StartsWith(path, StringComparison.OrdinalIgnoreCase));
    }


    private readonly Queue<string> recentEventsQueue = new Queue<string>();
    private readonly int maxQueueSize = 10; // Máximo de eventos a recordar

    private void LogEvent(string message)
    {
        Dispatcher.Invoke(() =>
        {
            // Evitar añadir mensajes duplicados recientes
            if (recentEventsQueue.Contains(message))
                return;

            LogListBox.Items.Add(message);

            // Agregar a la cola de eventos recientes
            recentEventsQueue.Enqueue(message);
            if (recentEventsQueue.Count > maxQueueSize)
                recentEventsQueue.Dequeue(); // Eliminar el más antiguo para no saturar memoria
        });

        File.AppendAllText(logFilePath, message + Environment.NewLine);
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        this.Hide(); // Ocultar ventana en vez de cerrar
    }

}





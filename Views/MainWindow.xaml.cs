using System;
using System.Windows;
using System.IO;
using System.Linq;
using FileSystemMonitor.Services;
using FileSystemMonitor.Models;
using FileSystemMonitor.Helpers;

namespace FileSystemMonitor
{
    public partial class MainWindow : Window
    {
        private readonly FileMonitorService fileMonitorService = new FileMonitorService();

        public MainWindow()
        {
            InitializeComponent();
            fileMonitorService.OnLogEntryCreated += LogEvent;
            StopButton.IsEnabled = false;
            LoadLog(); // Cargar eventos anteriores al iniciar
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            LogListBox.Items.Clear(); // Limpiar la lista al iniciar un nuevo monitoreo
            bool filterUserOnly = UserOnlyCheckBox.IsChecked == true;
            fileMonitorService.StartMonitoring(filterUserOnly);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            fileMonitorService.StopMonitoring();
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            LogHelper.ClearLog();
            LogListBox.Items.Clear();
        }

        private void LoadLog()
        {
            string logPath = LogHelper.GetLogFilePath(); // Obtener el log del día

            try
            {
                if (File.Exists(logPath))
                {
                    var lines = File.ReadLines(logPath).Reverse().Take(100).Reverse(); // Carga solo las últimas 100 líneas
                    LogListBox.Items.Clear();
                    foreach (var line in lines)
                    {
                        LogListBox.Items.Add(line);
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show("El archivo de log está en uso por otro proceso.", "Error de acceso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LogEvent(LogEntry entry)
        {
            Dispatcher.Invoke(() =>
            {
                if (!LogListBox.Items.Contains(entry.ToString())) // Evitar duplicados en la interfaz
                {
                    LogListBox.Items.Add(entry.ToString());
                }
            });
        }

        /*
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide(); // Ocultar ventana en vez de cerrar
        }
        */
    }
}

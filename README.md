# 🔍 FileSystemMonitor Last update 05/03/2024

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-Desktop-68217A?style=for-the-badge&logo=windows&logoColor=white)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)
![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white)

**Real-time file system monitoring for Windows**

[English](#english) | [Español](#español)

<img src="https://raw.githubusercontent.com/SoftDryzz/FileSystemMonitor/main/screenshot.png" alt="Screenshot" width="700"/>

</div>

---

# English

## 📖 About

**FileSystemMonitor** is a lightweight Windows desktop application that monitors file system changes in real-time. Built with **WPF** and **.NET 8**, it tracks file and folder creation, modification, deletion, and renaming events with a clean, modern interface.

Perfect for developers, sysadmins, and power users who need to track what's happening on their file system.

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🔴 **Real-time Monitoring** | Instant detection of file system changes |
| 📝 **Event Types** | Created, Modified, Deleted, Renamed |
| 📁 **Automatic Logging** | Daily log files for audit trails |
| 🚫 **Smart Filtering** | Ignores system files and Recycle Bin |
| 🌐 **Bilingual** | English & Spanish (auto-detected) |
| 🎨 **Modern UI** | Clean WPF interface with dark theme |
| ⚡ **Optimized** | Debounced events to prevent spam |
| 📦 **Portable** | Single executable, no installation required |

## 🖼️ Screenshots

<details>
<summary>Click to view screenshots</summary>

| Main Window | Event Log |
|-------------|-----------|
| ![Main](docs/main.png) | ![Log](docs/log.png) |

</details>

## 🚀 Quick Start

### Option 1: Download Release (Recommended)

1. Go to [Releases](https://github.com/SoftDryzz/FileSystemMonitor/releases)
2. Download `FileSystemMonitor-vX.X.X-win64.zip`
3. Extract and run `FileSystemMonitor.exe`

### Option 2: Build from Source

```bash
# Clone
git clone https://github.com/SoftDryzz/FileSystemMonitor.git
cd FileSystemMonitor

# Build
dotnet build -c Release

# Run
dotnet run
```

## 🛠️ Requirements

| Requirement | Version |
|-------------|---------|
| **OS** | Windows 10/11 |
| **.NET Runtime** | 8.0+ (included in release) |

## 📁 Project Structure

```
FileSystemMonitor/
├── Helpers/           # Utility classes and extensions
├── Models/            # Data models and event types
├── Services/          # Core monitoring service (FileSystemWatcher)
├── Views/             # WPF XAML views and windows
├── App.xaml           # Application entry point
└── FileSystemMonitor.csproj
```

## ⚙️ How It Works

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  FileSystem     │────▶│  FileWatcher     │────▶│  UI Display     │
│  (Windows API)  │     │  Service         │     │  + Log File     │
└─────────────────┘     └──────────────────┘     └─────────────────┘
```

1. **FileSystemWatcher** monitors specified directories
2. **Events** are filtered (ignoring system/temp files)
3. **Debouncing** prevents duplicate event spam
4. **UI updates** in real-time via WPF data binding
5. **Log files** saved daily in `logs/` folder

## 🤝 Contributing

Contributions welcome! Feel free to:

- 🐛 Report bugs
- 💡 Suggest features
- 🔧 Submit pull requests

## 📜 License

MIT License - See [LICENSE](LICENSE)

---

# Español

## 📖 Acerca de

**FileSystemMonitor** es una aplicación de escritorio ligera para Windows que monitorea cambios en el sistema de archivos en tiempo real. Desarrollada con **WPF** y **.NET 8**, rastrea eventos de creación, modificación, eliminación y renombrado de archivos y carpetas con una interfaz limpia y moderna.

Perfecta para desarrolladores, administradores de sistemas y usuarios avanzados que necesitan saber qué está pasando en su sistema de archivos.

## ✨ Características

| Característica | Descripción |
|----------------|-------------|
| 🔴 **Monitoreo en Tiempo Real** | Detección instantánea de cambios |
| 📝 **Tipos de Eventos** | Creado, Modificado, Eliminado, Renombrado |
| 📁 **Registro Automático** | Archivos log diarios para auditoría |
| 🚫 **Filtrado Inteligente** | Ignora archivos del sistema y Papelera |
| 🌐 **Bilingüe** | Español e Inglés (auto-detectado) |
| 🎨 **UI Moderna** | Interfaz WPF limpia con tema oscuro |
| ⚡ **Optimizado** | Eventos con debounce para evitar spam |
| 📦 **Portable** | Ejecutable único, sin instalación |

## 🚀 Inicio Rápido

### Opción 1: Descargar Release (Recomendado)

1. Ve a [Releases](https://github.com/SoftDryzz/FileSystemMonitor/releases)
2. Descarga `FileSystemMonitor-vX.X.X-win64.zip`
3. Extrae y ejecuta `FileSystemMonitor.exe`

### Opción 2: Compilar desde Código

```bash
# Clonar
git clone https://github.com/SoftDryzz/FileSystemMonitor.git
cd FileSystemMonitor

# Compilar
dotnet build -c Release

# Ejecutar
dotnet run
```

## 🛠️ Requisitos

| Requisito | Versión |
|-----------|---------|
| **SO** | Windows 10/11 |
| **.NET Runtime** | 8.0+ (incluido en release) |

## 📁 Estructura del Proyecto

```
FileSystemMonitor/
├── Helpers/           # Clases de utilidad y extensiones
├── Models/            # Modelos de datos y tipos de eventos
├── Services/          # Servicio principal (FileSystemWatcher)
├── Views/             # Vistas XAML y ventanas WPF
├── App.xaml           # Punto de entrada de la aplicación
└── FileSystemMonitor.csproj
```

## ⚙️ Cómo Funciona

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  Sistema de     │────▶│  Servicio        │────▶│  Interfaz       │
│  Archivos       │     │  FileWatcher     │     │  + Archivo Log  │
└─────────────────┘     └──────────────────┘     └─────────────────┘
```

1. **FileSystemWatcher** monitorea los directorios especificados
2. **Eventos** se filtran (ignorando archivos del sistema/temporales)
3. **Debouncing** previene spam de eventos duplicados
4. **UI se actualiza** en tiempo real via WPF data binding
5. **Archivos log** guardados diariamente en carpeta `logs/`

## 🤝 Contribuir

¡Las contribuciones son bienvenidas! Puedes:

- 🐛 Reportar bugs
- 💡 Sugerir características
- 🔧 Enviar pull requests

## 📜 Licencia

Licencia MIT - Ver [LICENSE](LICENSE)

---

<div align="center">

**Made with ❤️ by [SoftDryzz](https://github.com/SoftDryzz)**

[![GitHub](https://img.shields.io/badge/GitHub-SoftDryzz-181717?style=flat-square&logo=github)](https://github.com/SoftDryzz)
[![Website](https://img.shields.io/badge/Web-softdryzz.com-FF5722?style=flat-square&logo=google-chrome&logoColor=white)](https://softdryzz.com)

</div>

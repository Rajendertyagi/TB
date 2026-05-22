# Minimal Browser - Dark Chrome UI

A minimal browser built with WinUI 3, .NET 10, and WebView2 featuring a dark Chrome-like UI.

## Features

- **Dark Chrome-like UI**: Minimal address bar with back, forward, and refresh buttons
- **Framework-dependent**: Small executable size (requires .NET 10 Runtime)
- **Portable**: No installer needed, just run the EXE
- **WebView2**: Uses Microsoft Edge WebView2 for modern web rendering
- **Smart Address Bar**: Enter URLs or search queries directly

## Requirements

To run this application, the target machine must have:
1. **.NET 10 Runtime** (or later)
2. **Microsoft Edge WebView2 Runtime** (usually pre-installed on Windows 10/11)

## Building

### Prerequisites
- Visual Studio 2022 with .NET desktop development workload
- .NET 10 SDK
- Windows App SDK

### Build Steps

1. Open a command prompt in the project directory
2. Run `build.bat` or execute manually:

```bash
dotnet restore
dotnet publish -c Release -r win-x64 --no-self-contained /p:PublishSingleFile=true /p:EnableCompressionInSingleFile=true
```

3. The output will be in `bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\`

## Output

- **Single EXE**: Framework-dependent, approximately 50-100 KB
- **No DLLs**: All code bundled into single executable
- **Portable**: Copy and run anywhere

## Usage

1. Run `MinimalBrowser.exe`
2. Enter a URL or search term in the address bar
3. Press Enter to navigate
4. Use back/forward/refresh buttons for navigation

## Customization

Edit `MainWindow.xaml.cs` to customize:
- Default homepage (currently Google)
- Color scheme (currently dark Chrome-like)
- Window size and title

## License

MIT License

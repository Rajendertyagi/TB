@echo off
echo Building Minimal Browser...
cd /d "%~dp0"

:: Restore packages
dotnet restore

:: Build release version
dotnet publish -c Release -r win-x64 --no-self-contained /p:PublishSingleFile=true /p:EnableCompressionInSingleFile=true

:: Copy WebView2 loader dll if needed
copy /Y bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\*.exe .\MinimalBrowser.exe 2>nul

echo.
echo Build complete! Output: MinimalBrowser.exe
echo The exe is framework-dependent and requires .NET 10 Runtime and WebView2 installed on the target machine.
pause

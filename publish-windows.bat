@echo off
echo Publishing ServerWatcher for Windows (x64)...
echo.

dotnet publish ServerWatcher/ServerWatcher.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:EnableCompressionInSingleFile=true ^
    -o publish/win-x64

echo.
echo Build complete! Output location:
echo %CD%\publish\win-x64
echo.
echo Files:
dir /B publish\win-x64\ServerWatcher.exe
dir /B publish\win-x64\serverconfig.json
echo.
pause

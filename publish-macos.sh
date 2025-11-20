#!/bin/bash
echo "Publishing ServerWatcher for macOS (x64)..."
echo

dotnet publish ServerWatcher/ServerWatcher.csproj \
    -c Release \
    -r osx-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true \
    -o publish/osx-x64

echo
echo "Build complete! Output location:"
echo "$(pwd)/publish/osx-x64"
echo
echo "Files:"
ls -lh publish/osx-x64/ServerWatcher
ls -lh publish/osx-x64/serverconfig.json
echo
echo "To run: cd publish/osx-x64 && ./ServerWatcher"

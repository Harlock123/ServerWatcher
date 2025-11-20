#!/bin/bash
echo "Publishing ServerWatcher for Linux (x64)..."
echo

dotnet publish ServerWatcher/ServerWatcher.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true \
    -o publish/linux-x64

echo
echo "Build complete! Output location:"
echo "$(pwd)/publish/linux-x64"
echo
echo "Files:"
ls -lh publish/linux-x64/ServerWatcher
ls -lh publish/linux-x64/serverconfig.json
echo
echo "To run: cd publish/linux-x64 && ./ServerWatcher"

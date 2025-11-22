# Server Watcher - SSH Connection Monitor

An Avalonia-based .NET 9 desktop application that monitors SSH connectivity to multiple servers using configured credentials.

## Features

- Configure multiple servers with associated credential groups
- Test SSH connectivity to all configured servers
- **Website monitoring** - Optional HTTP/HTTPS endpoint monitoring for each server
- **Open in Browser** - Quick-launch button to open configured websites in system default browser
- **Automatic IP resolution** - Displays resolved IP addresses next to hostnames
- Visual status indicators (red/green/orange lights) for each server connection state
- Dual status display: SSH connectivity + website availability
- Real-time connection status updates
- **Password encryption tool** - Built-in utility to encrypt passwords for secure storage
- **Encrypted password support** - Automatically decrypts passwords from config file at runtime
- Scrollable server list to accommodate any number of servers
- Cross-platform support (Windows, Linux, macOS)

## Configuration

The application reads from a `serverconfig.json` file that must be placed in the same directory as the executable.

### Configuration File Format

```json
{
  "Servers": [
    {
      "ServerName": "server1.example.com",
      "Group": "GROUP1",
      "Description": "Production Web Server - Main Application",
      "Path": "/var/www/html",
      "WebSite": "https://server1.example.com"
    },
    {
      "ServerName": "192.168.1.100",
      "Group": "GROUP2",
      "Description": "Database Server - Primary",
      "Path": "/var/lib/mysql"
    },
    {
      "ServerName": "api.example.com",
      "Group": "GROUP1",
      "Description": "API Server",
      "Path": "/opt/api",
      "WebSite": "https://api.example.com:8443/health"
    }
  ],
  "CredentialGroups": [
    {
      "GroupName": "GROUP1",
      "LoginName": "admin",
      "Password": "password123"
    },
    {
      "GroupName": "GROUP2",
      "LoginName": "root",
      "Password": "rootpass"
    }
  ]
}
```

### Configuration Elements

- **Servers**: List of servers to monitor
  - `ServerName`: Hostname or IP address of the server (hostnames will show resolved IP in blue)
  - `Group`: Reference to the credential group to use (e.g., "GROUP1", "GROUP2")
  - `Description`: Optional descriptive label for the server (displayed in the UI)
  - `Path`: Optional file system path for file browsing functionality
  - `WebSite`: Optional HTTP/HTTPS URL to monitor (supports custom ports, e.g., "https://server.com:8443/health")

- **CredentialGroups**: List of credential sets
  - `GroupName`: Unique identifier for the credential group
  - `LoginName`: SSH username
  - `Password`: SSH password (can be encrypted using the built-in Password Encryption Tool)

## Building and Running

### Prerequisites

- .NET 9 SDK
- SSH access to test servers (optional, for testing)

### Build

```bash
dotnet build ServerWatcher/ServerWatcher.csproj
```

### Run

```bash
dotnet run --project ServerWatcher/ServerWatcher.csproj
```

Or navigate to the build output directory and run the executable:

```bash
cd ServerWatcher/bin/Debug/net9.0
./ServerWatcher
```

### Publish as Single-File Executable

To create a standalone, self-contained executable that doesn't require .NET runtime to be installed:

#### Windows

```bash
# Run the provided script
publish-windows.bat

# Or manually:
dotnet publish ServerWatcher/ServerWatcher.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish/win-x64
```

#### Linux

```bash
# Make script executable and run
chmod +x publish-linux.sh
./publish-linux.sh

# Or manually:
dotnet publish ServerWatcher/ServerWatcher.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish/linux-x64
```

#### macOS

```bash
# Make script executable and run
chmod +x publish-macos.sh
./publish-macos.sh

# Or manually:
dotnet publish ServerWatcher/ServerWatcher.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish/osx-x64
```

The published executable will be in the `publish/[platform]` directory along with the `serverconfig.json` file. The configuration file remains separate and can be edited without rebuilding the application.

## Usage

### Basic Operation

1. Edit the `serverconfig.json` file with your servers and credentials
2. Launch the application
3. Click the "Test All Connections" button to test SSH connectivity and website availability
4. View the status indicators for each server:

#### SSH Status (First Row)
   - **Gray**: Not tested
   - **Orange**: Connecting
   - **Green**: Successfully connected
   - **Red**: Connection failed

#### Website Status (Second Row - only shown if WebSite is configured)
   - **Gray**: Not tested or no website configured
   - **Orange**: Checking website
   - **Green**: Website responding (HTTP 200-299)
   - **Red**: Website unreachable or error response
   - **Open in Browser button**: Click to launch the configured website in your system's default web browser
     - This button is only visible for servers that have a website URL configured
     - Opens the exact URL configured in the `WebSite` field

#### IP Address Display
   - Hostnames automatically display their resolved IP address in blue next to the server name
   - Example: `server1.example.com (192.168.1.100)`
   - If the ServerName is already an IP address, no additional IP is shown

### Password Encryption

The application supports encrypted passwords in the configuration file for enhanced security.

#### Encrypting Passwords

1. Launch the application
2. Click the "Encrypt Password" button in the main window
3. In the Password Encryption Tool dialog:
   - Enter your plain text password
   - Click "Encrypt Password"
   - Copy the encrypted password that appears
4. Paste the encrypted password into the `Password` field in your `serverconfig.json` file

#### Example Configuration with Encrypted Password

```json
{
  "CredentialGroups": [
    {
      "GroupName": "GROUP1",
      "LoginName": "admin",
      "Password": "k8vF2Xq9mP3nR5tY7wZ1aB4cD6eG8hJ0"
    }
  ]
}
```

The application automatically detects and decrypts encrypted passwords at runtime. Plain text passwords are also supported for backward compatibility.

## Security Considerations

The application provides password encryption to protect credentials in the configuration file:
- **Encrypted passwords**: Use the built-in Password Encryption Tool to encrypt passwords before storing them in the config file
- **Secure storage**: Store the configuration file in a secure location with restricted file permissions
- **Access control**: Restrict access to the application executable and configuration file
- **SSH keys**: For maximum security, consider using SSH key-based authentication instead of passwords
- **Encryption key**: The encryption key is embedded in the application. For enterprise deployments, consider storing the key in a secure vault (e.g., Azure Key Vault, Windows Credential Manager)

## Project Structure

```
ServerWatcher/
├── Models/
│   ├── AppConfiguration.cs      # Configuration data model
│   ├── ServerConfig.cs          # Server configuration
│   ├── CredentialGroup.cs       # Credential group configuration
│   └── ServerStatus.cs          # Runtime server status (SSH + website status)
├── Services/
│   ├── ConfigurationService.cs  # Configuration file loader
│   ├── SshConnectionService.cs  # SSH connectivity tester
│   └── WebsiteCheckService.cs   # HTTP/HTTPS website checker
├── ViewModels/
│   └── MainWindowViewModel.cs   # Main window logic with DNS resolution
├── Views/
│   └── MainWindow.axaml         # Main window UI with dual status indicators
└── serverconfig.json            # Configuration file
```

## Technologies Used

- **.NET 9**: Latest .NET framework
- **Avalonia UI**: Cross-platform UI framework
- **SSH.NET**: SSH connectivity library
- **CommunityToolkit.Mvvm**: MVVM helpers and utilities

## License

This project is provided as-is for monitoring SSH server connectivity.

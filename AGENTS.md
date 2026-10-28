# AGENTS.md

Notes for anyone, human or AI agent, changing this code. `README.md` covers the features, the config file format
and the password tool; this file covers how the code fits together and how to change it safely.

## What this is

A desktop monitor in C# on .NET 9 (`net9.0`) and Avalonia 11.2 that tests SSH logins and optional website checks
against a list of servers, with an SFTP file browser and a password encryption tool. It uses MVVM with
`CommunityToolkit.Mvvm`, and `SSH.NET` for SSH and SFTP. It runs on Windows, Linux and macOS.

## Commands

```bash
dotnet build ServerWatcher.sln                    # no global.json: any .NET 9 SDK
dotnet run --project ServerWatcher/ServerWatcher.csproj
./publish-linux.sh                                # self-contained single file for linux-x64, into publish/linux-x64
./publish-macos.sh                                # the same for osx-x64
publish-windows.bat                               # the same for win-x64
```

There are no tests yet.

## Layout

- `ServerWatcher/Program.cs`, `App.axaml(.cs)`, `ViewLocator.cs`: the standard Avalonia MVVM start-up.
- `ServerWatcher/Models/`: `AppConfiguration` (servers and credential groups), `ServerConfig`, `CredentialGroup`,
  `ServerStatus` (the live SSH and website state of one row) and `RemoteFileInfo`.
- `ServerWatcher/Services/`:
  - `ConfigurationService.cs` loads and saves `serverconfig.json` from `AppContext.BaseDirectory`, and decrypts
    each password on load.
  - `SshConnectionService.cs` tests a login (`SshClient`) and lists a server's `Path` (`SftpClient`), with a
    10-second timeout on port 22.
  - `WebsiteCheckService.cs` checks a server's `WebSite` URL with `HttpClient`, also with a 10-second timeout.
  - `PasswordEncryptionService.cs` encrypts and decrypts passwords with AES.
- `ServerWatcher/ViewModels/`: `MainWindowViewModel` holds the commands (test all, test one, open the file
  browser, open the website, open the password tool) and resolves host names to IP addresses.
  `FileBrowserViewModel` and `PasswordEncryptionViewModel` back the other two windows.
- `ServerWatcher/Views/`: `MainWindow`, `FileBrowserWindow` and `PasswordEncryptionWindow`.
- `ServerWatcher/serverconfig.json`: a sample config with placeholder hosts, copied to the output folder.

## Rules that keep it working

- **The config lives beside the executable.** `serverconfig.json` is read from `AppContext.BaseDirectory`, so the
  project copies it to the output and the csproj sets `ExcludeFromSingleFile`, keeping it a separate file in
  single-file publishes. Keep both settings.
- **Encrypted and plain passwords both work.** On load, a password that fails to decrypt is kept as plain text.
  Don't make decryption failures fatal, or existing configs break.
- **The key is fixed in code.** `PasswordEncryptionService` derives its AES key from a passphrase in the source
  and uses a fixed IV. Changing either makes every encrypted password in existing configs unreadable.
- **Never commit real hosts or passwords.** Keep `serverconfig.json` in the repository as placeholders only.
- **Network work stays off the UI thread.** The services wrap blocking `SSH.NET` calls in `Task.Run`; keep new
  network calls asynchronous.
- **Compiled bindings.** `AvaloniaUseCompiledBindingsByDefault` is on, so each view needs an `x:DataType`, and
  binding mistakes fail the build.

## Making a change

- Check changes by running the app against a server you control, since there are no tests.
- Update `README.md` (features, config format, project structure) to match.
- The `publish/` folder is ignored by git; don't commit builds.

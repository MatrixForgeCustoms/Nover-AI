# Nover

A Windows AI chat assistant with user accounts, tokens, and models from Google Gemini and Groq.
Built with C#, WebView2, and a local HTTP server.

## Project structure

| Area | Files |
|---|---|
| Entry | NoverApp.csproj, Program.cs, MainForm.cs |
| Config | Config/AppSettings.cs, Config/Roles.cs |
| Core | Core/TokenRules.cs, PasswordHasher.cs, SessionStore.cs, Logger.cs, CommandParser.cs |
| Data | Data/JsonStore.cs, Models.cs, UserRepository.cs, ChatRepository.cs |
| Logic | Services/UserService.cs, ChatService.cs |
| AI | AI/IChatProvider.cs, GeminiProvider.cs, OpenAiCompatibleProvider.cs, PromptBuilder.cs |
| HTTP | Web/ApiServer.cs, ApiRouter.cs, Json.cs, AuthHandler.cs, ChatHandler.cs, AdminHandler.cs |
| Frontend | wwwroot/index.html, style.css, app.js |

## Requirements

- Windows 10 or 11
- .NET 8 SDK
- Microsoft Edge WebView2 Runtime (included in most Windows 11 installations)

## Setup

1. Set the environment variables `GEMINI_API_KEY` and `GROQ_API_KEY`, then restart your terminal.
2. Open `NoverApp/NoverApp.csproj` in Visual Studio and press F5.

## Build a single-file executable

```
dotnet publish NoverApp/NoverApp.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

## Build the installer

1. Install Inno Setup 6.
2. Run `installer/build_installer.bat`. The setup file is created in `dist/`.
3. Optional: sign the files with `installer/sign_setup.bat "path\to\certificate.pfx" "password"`.

The setup wizard offers the language (taken from Windows), the install location, shortcuts, autostart,
and an option to always run as administrator.

## Data

Accounts, chats, sessions, and logs are stored in `%APPDATA%\Nover`. They are kept during updates and
uninstalls unless you explicitly choose to delete them.

## Chat commands

`/help`, `/balance`, `/new`, `/logout`, `/tokens <name> <amount>` (admins),
`/account <name> <password> <role>` and `/role <name> <role>` (owner only).

## Limitations

- The code has not been compiled in every environment yet. Build errors may occur on first use.
- The server runs on localhost only.
- For multiple users over the internet, a hosted backend with a database is needed.

## License

MIT, see LICENSE.

# Copilot Instructions for Vision Edit

## Architecture Overview

Vision Edit is a .NET 10 cross-platform desktop application using MAUI (.NET Multi-platform App UI) for the client and ASP.NET Core for the backend API. The solution consists of five projects:

- **Vision Edit** - MAUI desktop client targeting Windows, Android, iOS, and macOS
- **Vision Edit API** - ASP.NET Core 10 REST API backend
- **Models** - Shared data models (UserModel, DocumentModel, LoginModel, etc.)
- **ORM** - Entity Framework Core 9 database layer with MySQL backend
- **Tools** - Shared utilities (ApiHandler, UserManager, Validation)

The application uses an MVVM pattern with the Community Toolkit.Mvvm library. The API communicates with a MySQL database via Entity Framework Core with Pomelo provider.

## Build, Test, and Lint Commands

### Build
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build Vision\ Edit.sln -p:Configuration=Release
```

### Run
```bash
# Run API (from Vision Edit API directory)
dotnet run

# Run MAUI client (requires Windows for WinUI)
dotnet run -f net10.0-windows10.0.19041.0
```

### Database
```bash
# Create/update migrations (from ORM directory)
dotnet ef migrations add MigrationName
dotnet ef database update
```

No automated tests or linters are currently configured in the repository.

## Key Conventions

### ViewModel Pattern
- All ViewModels inherit from `ObservableObject` (MAUI Community Toolkit)
- Use `[ObservableProperty]` attribute for properties that need to notify UI of changes
- Use `[RelayCommand]` attribute for command methods (converts method to IAsyncRelayCommand)
- **Error Handling**: Every async command includes input validation, `IsLoading` state management, `ErrorMessage` property for UI feedback, and try-catch-finally blocks
- Example: See `LoginViewModel.cs` and `ChatViewModel.cs`

### Dependency Injection
- Services are registered in `MauiProgram.cs` (MAUI app) or `Program.cs` (API)
- Use `AddSingleton` for shared services like `UserManager`, `ApiHandler`, `EditorViewModel`
- Use `AddTransient` for page/view instances to create new instances per navigation
- Controllers use constructor injection of `DbManager` to access the database context

### Database Access
- `DbManager` class (in ORM project) extends `DbContext` and defines `DbSet<UserModel>` and `DbSet<DocumentModel>`
- Service classes (`UserService`, `DocumentService`) handle database operations
- Connection string is hardcoded in `DbManager.OnConfiguring()` (Server=localhost;Database=vision_edit;User=root;Password=root)
- **This is for development only** - connection should be externalized in production

### API Controllers
- Controllers are located in the `Vision Edit API\Controllers` directory
- Constructor receives `DbManager` and creates service instances
- Input validation uses the `Validation` utility class before model state checks
- HTTP responses follow standard REST patterns: OK (200), Created (201), NoContent (204), Unauthorized (401), ValidationProblem (400)

### MVVM Toolkit Configuration
- MVVMTK0045 AOT warnings are suppressed in the Vision Edit project via `<NoWarn>MVVMTK0045</NoWarn>` in the csproj file

### Code Structure
- XAML views use code-behind paired with ViewModels
- Views folder contains XAML pages and custom controls (ChatView, EditorView, GlowBorderView)
- Models are defined in the shared Models project and used across all layers
- DTOs are in the Models/DTOs folder for API data transfer

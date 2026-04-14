# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Solution Structure

Five projects in a layered architecture:

| Project | Framework | Role |
|---------|-----------|------|
| `Vision Edit` | .NET 10 MAUI | Desktop/mobile UI client |
| `Vision Edit API` | ASP.NET Core 10 | REST API backend |
| `Models` | .NET 10 Class Library | Shared DTOs and entity models |
| `ORM` | .NET 10 Class Library | EF Core 9 + MySQL database layer |
| `Tools` | .NET 10 Class Library | OpenAI integration, UserManager, Validation |

**Dependency graph:** `Vision Edit` → Models, Tools | `Vision Edit API` → ORM, Models | `ORM` → Models

## Build & Run Commands

```bash
# Build entire solution
dotnet build

# Run the API (from "Vision Edit API" directory)
dotnet run

# Run the MAUI client on Windows
dotnet run -f net10.0-windows10.0.19041.0

# EF Core migrations (from ORM directory)
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

No automated tests or linters are configured.

## Architecture

**Client (MAUI MVVM):**
- ViewModels use `CommunityToolkit.Mvvm` — properties with `[ObservableProperty]`, commands with `[RelayCommand]`
- All ViewModels inherit `ObservableObject`
- Every async command follows: validate input → set `IsLoading = true` → try/catch/finally → set `ErrorMessage` on failure
- `EditorViewModel` drives inline AI text completions (debounced, GPT-4o-mini via `ApiHandler`)
- `ChatViewModel` maintains a rolling conversation window (max 10 messages) using GPT-4o; can inject selected editor text
- `AppShellViewModel` manages flyout menu visibility based on `UserManager.IsLoggedIn`
- `MVVMTK0045` AOT warnings suppressed in `Vision Edit.csproj` via `<NoWarn>`

**DI registration (`MauiProgram.cs`):**
- Singletons: `UserManager`, `ApiHandler`, `EditorViewModel`
- Transients: all pages and views
- Named `HttpClient` "Base" → `https://localhost:44311/api/`

**API:**
- Controllers receive `DbManager` via constructor injection and instantiate service classes from it
- Input validated with `Validation` utility before model-state checks
- HTTP responses: 200/201/204/400/401

**Database (ORM project):**
- `DbManager` extends `DbContext`; defines `DbSet<UserModel>` and `DbSet<DocumentModel>`
- Connection string is hardcoded in `DbManager.OnConfiguring()` (`Server=localhost;Database=vision_edit;User=root;Password=root`) — dev only
- `UserService` and `DocumentService` wrap `DbManager` for business logic
- Document save is an upsert: create if name is new for user, update if it already exists

**Tools project:**
- `ApiHandler` — wraps OpenAI SDK; two models: `gpt-4o` (chat), `gpt-4o-mini` (completions)
- `UserManager` — observable `Username` property; `IsLoggedIn` derived from it
- `Validation` — static helpers for strings, passwords, usernames, dates

**Shared Models:**
- `UserModel` / `DocumentModel` — EF Core entities (unique index on `UserModel.Username`)
- `LoginModel` / `SaveDocumentModel` — API DTOs (in `Models/DTOs`)

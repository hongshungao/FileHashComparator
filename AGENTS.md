# Repository Guidelines

## Project Structure & Module Organization
Source lives under `src/FileHashComparator.App/`, a WPF app organized in an MVVM layout:
- `Models/` for domain models and settings.
- `Services/` for hashing, dialogs, localization, and settings logic (`Services/Hashing/` holds algorithm implementations).
- `ViewModels/` for UI state, `Views/` for XAML views, and `Resources/` for localized strings (e.g., `Strings.en-US.xaml`).
Entry points are `App.xaml` and `MainWindow.xaml`. Project docs live in `specs/` and `plans/`. Build outputs are under `bin/` and `obj/` and should stay out of version control.

## Build, Test, and Development Commands
Use a .NET SDK that supports `net10.0-windows`.
- `dotnet build .\src\FileHashComparator.App\FileHashComparator.App.csproj` — builds the app.
- `dotnet run --project .\src\FileHashComparator.App\FileHashComparator.App.csproj` — runs the WPF app.
- `dotnet publish .\src\FileHashComparator.App\FileHashComparator.App.csproj -c Release -r win-x64` — creates a release build.

## Coding Style & Naming Conventions
C# uses nullable reference types and file-scoped namespaces. Indent with 4 spaces, use PascalCase for public types/members, and `_camelCase` for private fields. View models are `*ViewModel` and use CommunityToolkit.Mvvm attributes such as `[ObservableProperty]`. XAML uses PascalCase names for views and controls.

## Testing Guidelines
There are no test projects in this repository yet. If you add tests, place them under a `tests/` folder, name test files `*Tests.cs`, and run `dotnet test` from the repo root.

## Commit & Pull Request Guidelines
No Git history is present in this workspace, so there is no established commit style to follow. Use short, imperative commit messages (e.g., `Add hash list parser validation`). PRs should include a clear summary, manual test steps or results, and screenshots for UI changes. Link related specs in `specs/` or plans in `plans/` when applicable.

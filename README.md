# MonacoRoslynCompletionProvider

Provides C# intellisense (completion, hover, signature help, code check) for Monaco Editor using Roslyn.

## Features

- **Tab Completion**: Suggestions for methods, properties, classes, keywords, etc.
- **Hover Information**: XML documentation and signatures on hover.
- **Signature Help**: Parameter info when typing methods.
- **Code Check**: Live diagnostics (errors, warnings) as you type.
- **Dynamic Compilation**: Compiles code in-memory using Roslyn.

## Architecture

- **Core Library**: `MonacoRoslynCompletionProvider`
    - `CompletionService`: Main entry point, manages workspaces and handles requests.
    - `CompletionWorkspace`: Wraps Roslyn `AdhocWorkspace`.
    - `Providers`: Specific logic for completion, hover, etc.
- **Sample App**: `Sample` (ASP.NET Core Web API)
    - Exposes endpoints used by the frontend.
    - Serves the Monaco Editor frontend.

## Setup

1.  **Prerequisites**:
    -   .NET 8.0 SDK
    -   Node.js (for frontend dependencies)

2.  **Frontend Setup**:
    ```bash
    cd MonacoRoslynCompletionProvider/Sample/wwwroot
    npm install
    ```

3.  **Run Sample**:
    ```bash
    cd MonacoRoslynCompletionProvider/Sample
    dotnet run
    ```
    Open `http://localhost:5280` in your browser.

## Key Assumptions

- The backend runs on the same machine/network as the frontend access.
- Assemblies for reference are loaded from the machine running the backend.
- Security: The `CompletionWorkspace` allows execution of arbitrary code during compilation (e.g. analyzers) and loads assemblies. **Do not expose this service publicly without sandboxing.**

## Recent Refactoring

- **API Improvements**: Migrated to `MapPost` with typed requests and `[FromBody]`. Added global exception handling and logging.
- **Frontend**: Updated request handling to ensure correct content types.
- **Core Optimization**: Parallelized tab completion description fetching. Improved validation and logging.
- **Project Structure**: Unified target framework to .NET 8.0.

# Architecture

The `MonacoRoslynCompletionProvider` solution consists of two main parts: the backend library/API and the frontend integration.

## Backend (`MonacoRoslynCompletionProvider`)

This project provides the core logic for C# code completion, hover information, signature help, and code analysis using the Roslyn API (Microsoft.CodeAnalysis).

### Key Components

*   **`ICompletionService` / `CompletionService`**: The main entry point for the API. It manages `CompletionWorkspace` instances and executes requests. It handles request validation and error logging.
*   **`CompletionWorkspace`**: Represents a Roslyn workspace. It holds a `Project` and an `AdhocWorkspace`. It's responsible for creating `CompletionDocument` snapshots from the provided code. It caches metadata references to optimize performance.
*   **`CompletionDocument`**: A snapshot of the code at a specific point in time. It provides access to the `Document`, `SemanticModel`, and `Diagnostics`.
*   **Providers**:
    *   `TabCompletionProvider`: Generates completion items (methods, classes, keywords, etc.).
    *   `HoverInformationProvider`: Provides tooltips with type information and documentation.
    *   `SignatureHelpProvider`: Provides information about method parameters.
    *   `CodeCheckProvider`: returns compiler diagnostics (errors and warnings).

### Flow

1.  A request comes in via the API (e.g., `TabCompletionRequest`).
2.  `CompletionService` validates the request.
3.  It retrieves or creates a `CompletionWorkspace` based on the requested assemblies.
4.  It calls `CreateDocument` on the workspace, which creates a new `Document` in the Roslyn workspace with the provided code.
5.  The appropriate provider (e.g., `TabCompletionProvider`) is invoked with the document.
6.  The result is returned to the client.

## Frontend (`Sample`)

The sample project demonstrates how to consume the backend API in a web application using the Monaco Editor.

### Key Components

*   **`index.html`**: The main page hosting the editor.
*   **`csharpLanguageProvider.js`**: A JavaScript file that registers the C# language provider for Monaco. It implements:
    *   `provideCompletionItems`: Fetches completion suggestions.
    *   `provideSignatureHelp`: Fetches parameter info.
    *   `provideHover`: Fetches hover tooltips.
    *   `validate`: periodically checks for code errors (diagnostics).
*   **`Program.cs`**: The ASP.NET Core backend for the sample app. It exposes the `CompletionService` methods as HTTP endpoints.

## Interaction

The frontend sends HTTP POST requests to the backend. The backend processes the code using Roslyn and returns the results as JSON. The frontend then maps these results to Monaco Editor's internal data structures.

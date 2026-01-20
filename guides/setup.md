# Setup Guide

## Prerequisites

*   **.NET 8.0 SDK**: Ensure you have the .NET 8.0 SDK installed.
*   **Node.js & npm**: Required for the frontend dependencies.

## Building the Backend

1.  Navigate to the repository root.
2.  Build the solution:
    ```bash
    dotnet build MonacoRoslynCompletionProvider/MonacoRoslynCompletionProvider.sln
    ```

## Setting up the Frontend

1.  Navigate to the `Sample/wwwroot` directory:
    ```bash
    cd MonacoRoslynCompletionProvider/Sample/wwwroot
    ```
2.  Install dependencies:
    ```bash
    npm install
    ```

## Running the Sample Application

1.  Navigate to the `Sample` directory:
    ```bash
    cd MonacoRoslynCompletionProvider/Sample
    ```
2.  Run the application:
    ```bash
    dotnet run
    ```
3.  Open your browser and navigate to `http://localhost:5280` (or the port indicated in the console output).

## Running Tests

To run the unit tests:

```bash
dotnet test MonacoRoslynCompletionProvider/Tests/Tests.csproj
```

# Getting Started

This guide explains how to build, run, and test the MonacoRoslynCompletionProvider sample application.

## Prerequisites

*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [Node.js](https://nodejs.org/) (for installing frontend dependencies)

## Setup

1.  Clone the repository.
2.  Navigate to the `MonacoRoslynCompletionProvider/Sample/wwwroot` directory and install dependencies:
    ```bash
    cd MonacoRoslynCompletionProvider/Sample/wwwroot
    npm install
    ```
    (Note: The sample uses `axios` and `monaco-editor` loaded via CDN in `index.html`, but `package.json` tracks dependencies).

## Running the Sample

1.  Navigate to the `MonacoRoslynCompletionProvider/Sample` directory.
2.  Run the application:
    ```bash
    dotnet run
    ```
3.  Open your browser and navigate to `http://localhost:5280` (or the port specified in the console).

## Running Tests

To run the unit tests:

1.  Navigate to the `MonacoRoslynCompletionProvider` root directory.
2.  Run the tests:
    ```bash
    dotnet test
    ```

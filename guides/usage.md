# Usage Guide

This guide explains how to use the features provided by the Monaco Roslyn Completion Provider.

## Features

### Code Completion (IntelliSense)

*   **Trigger**: Type a character (e.g., `.`) or press `Ctrl+Space`.
*   **Behavior**: A list of suggestions will appear, including classes, methods, properties, and keywords relevant to the current context.
*   **Details**: Selecting an item might show additional documentation (if available).

### Hover Information

*   **Trigger**: Hover your mouse cursor over a symbol (variable, class, method).
*   **Behavior**: A tooltip will appear showing the symbol's type, signature, and documentation comments.

### Signature Help

*   **Trigger**: Type an opening parenthesis `(` after a method name.
*   **Behavior**: A popup will show the method signature(s), highlighting the current parameter you are typing.

### Code Validation (Diagnostics)

*   **Trigger**: Automatic. The code is checked periodically as you type (with a slight delay).
*   **Behavior**: Errors and warnings are highlighted with red squiggles. Hovering over the squiggle shows the error message.

## Extending the Provider

To support additional assemblies (libraries) for completion:

1.  **Backend**: Ensure the assemblies are available on the server.
2.  **Frontend**: In `csharpLanguageProvider.js`, modify the `assemblies` array in `registerCsharpProvider` function.
    ```javascript
    var assemblies = [
        "/path/to/MyLibrary.dll",
        "/path/to/AnotherLibrary.dll"
    ];
    ```
    *Note: The backend must be able to resolve these paths.*

## Troubleshooting

*   **No Completion**: Check the browser console for network errors. Ensure the backend server is running.
*   **Missing Types**: Ensure the required assemblies are referenced. The default setup includes basic system assemblies.
*   **Slow Performance**: Large files or too many references can slow down Roslyn. The backend caches workspaces to mitigate this, but initial load might be slower.

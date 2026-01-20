# API Reference

The backend exposes several endpoints to support code completion and analysis.

## Endpoints

### Tab Completion
*   **URL**: `/completion/complete`
*   **Method**: `POST`
*   **Body**: `TabCompletionRequest`
    ```json
    {
      "Code": "string",
      "Position": int,
      "Assemblies": ["string"]
    }
    ```
*   **Response**: `TabCompletionResult[]`
    ```json
    [
      {
        "Suggestion": "string",
        "Description": "string",
        "Tag": "string"
      }
    ]
    ```

### Hover Information
*   **URL**: `/completion/hover`
*   **Method**: `POST`
*   **Body**: `HoverInfoRequest`
    ```json
    {
      "Code": "string",
      "Position": int,
      "Assemblies": ["string"]
    }
    ```
*   **Response**: `HoverInfoResult`
    ```json
    {
      "Information": "string",
      "OffsetFrom": int,
      "OffsetTo": int
    }
    ```

### Signature Help
*   **URL**: `/completion/signature`
*   **Method**: `POST`
*   **Body**: `SignatureHelpRequest`
*   **Response**: `SignatureHelpResult`

### Code Check
*   **URL**: `/completion/codeCheck`
*   **Method**: `POST`
*   **Body**: `CodeCheckRequest`
*   **Response**: `CodeCheckResult[]`
    ```json
    [
      {
        "Id": "string",
        "Message": "string",
        "Severity": int, // 0: Hidden, 1: Info, 2: Warning, 3: Error
        "OffsetFrom": int,
        "OffsetTo": int
      }
    ]
    ```

## Request Objects

All requests share common properties:
*   `Code`: The full source code of the file.
*   `Assemblies`: A list of assembly paths to reference.

Requests requiring a cursor position (Completion, Hover, Signature) also include:
*   `Position`: The zero-based character offset of the cursor.

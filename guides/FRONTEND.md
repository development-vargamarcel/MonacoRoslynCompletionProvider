# Frontend Integration

To use the completion provider in your web application, include the `csharpLanguageProvider.js` file and register the provider.

## Usage

1.  **Include the script**:
    ```html
    <script src="csharpLanguageProvider.js"></script>
    ```

2.  **Initialize Monaco Editor**:
    ```javascript
    require.config({ paths: { 'vs': 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.44.0/min/vs' }});
    require(['vs/editor/editor.main'], function() {
        // Register the provider
        registerCsharpProvider();

        // Or with specific assemblies:
        // registerCsharpProvider(["/path/to/assembly.dll"]);

        var editor = monaco.editor.create(document.getElementById('container'), {
            value: [
                'using System;',
                'public class Class1 {',
                '    public void Test() {',
                '        Console.WriteLine("Hello World");',
                '    }',
                '}'
            ].join('\n'),
            language: 'csharp'
        });
    });
    ```

## Customization

The `registerCsharpProvider` function accepts an optional array of assembly paths. These paths are sent to the backend with every request, allowing the Roslyn workspace to include references to external libraries.

```javascript
registerCsharpProvider(["/libs/MyLibrary.dll", "/libs/AnotherLib.dll"]);
```

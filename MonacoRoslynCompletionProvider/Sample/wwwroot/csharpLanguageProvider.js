async function sendRequest(type, request) {
    let endPoint;
    switch (type) {
        case 'complete': endPoint = '/completion/complete'; break;
        case 'signature': endPoint = '/completion/signature'; break;
        case 'hover': endPoint = '/completion/hover'; break;
        case 'codeCheck': endPoint = '/completion/codeCheck'; break;
    }
    try {
        return await axios.post(endPoint, request);
    } catch (error) {
        console.warn(`Failed to perform request: ${type}`, error);
        return null;
    }
}

function registerCsharpProvider() {

    var assemblies = [];

    monaco.languages.registerCompletionItemProvider('csharp', {
        triggerCharacters: [".", " "],
        provideCompletionItems: async (model, position) => {
            let suggestions = [];

            let request = {
                Code: model.getValue(),
                Position: model.getOffsetAt(position),
                Assemblies: assemblies
            }

            let resultQ = await sendRequest("complete", request);

            if (resultQ && resultQ.data) {
                for (let elem of resultQ.data) {
                    let kind = monaco.languages.CompletionItemKind.Function;
                    if (elem.Tag) {
                        switch (elem.Tag) {
                            case 'Class': kind = monaco.languages.CompletionItemKind.Class; break;
                            case 'Delegate': kind = monaco.languages.CompletionItemKind.Function; break;
                            case 'Enum': kind = monaco.languages.CompletionItemKind.Enum; break;
                            case 'EnumMember': kind = monaco.languages.CompletionItemKind.EnumMember; break;
                            case 'Event': kind = monaco.languages.CompletionItemKind.Event; break;
                            case 'ExtensionMethod': kind = monaco.languages.CompletionItemKind.Method; break;
                            case 'Field': kind = monaco.languages.CompletionItemKind.Field; break;
                            case 'Interface': kind = monaco.languages.CompletionItemKind.Interface; break;
                            case 'Keyword': kind = monaco.languages.CompletionItemKind.Keyword; break;
                            case 'Local': kind = monaco.languages.CompletionItemKind.Variable; break;
                            case 'Method': kind = monaco.languages.CompletionItemKind.Method; break;
                            case 'Module': kind = monaco.languages.CompletionItemKind.Module; break;
                            case 'Namespace': kind = monaco.languages.CompletionItemKind.Module; break;
                            case 'Operator': kind = monaco.languages.CompletionItemKind.Operator; break;
                            case 'Parameter': kind = monaco.languages.CompletionItemKind.Variable; break;
                            case 'Property': kind = monaco.languages.CompletionItemKind.Property; break;
                            case 'RangeVariable': kind = monaco.languages.CompletionItemKind.Variable; break;
                            case 'Reference': kind = monaco.languages.CompletionItemKind.Reference; break;
                            case 'Structure': kind = monaco.languages.CompletionItemKind.Struct; break;
                            case 'TypeParameter': kind = monaco.languages.CompletionItemKind.TypeParameter; break;
                            case 'Snippet': kind = monaco.languages.CompletionItemKind.Snippet; break;
                            case 'Constant': kind = monaco.languages.CompletionItemKind.Constant; break;
                        }
                    }
                    suggestions.push({
                        label: {
                            label: elem.Suggestion,
                            description: elem.Description
                        },
                        kind: kind,
                        insertText: elem.Suggestion
                    });
                }
            }

            return { suggestions: suggestions };
        }
    });

    monaco.languages.registerSignatureHelpProvider('csharp', {
        signatureHelpTriggerCharacters: ["("],
        signatureHelpRetriggerCharacters: [","],

        provideSignatureHelp: async (model, position, token, context) => {

            let request = {
                Code: model.getValue(),
                Position: model.getOffsetAt(position),
                Assemblies: assemblies
            }

            let resultQ = await sendRequest("signature", request);
            if (!resultQ || !resultQ.data) return;

            let signatures = [];
            for (let signature of resultQ.data.Signatures) {
                let params = [];
                for (let param of signature.Parameters) {
                    params.push({
                        label: param.Label,
                        documentation: param.Documentation ?? ""
                    });
                }

                signatures.push({
                    label: signature.Label,
                    documentation: signature.Documentation ?? "",
                    parameters: params,
                });
            }

            let signatureHelp = {};
            signatureHelp.signatures = signatures;
            signatureHelp.activeParameter = resultQ.data.ActiveParameter;
            signatureHelp.activeSignature = resultQ.data.ActiveSignature;

            return {
                value: signatureHelp,
                dispose: () => { }
            };
        }
    });


    monaco.languages.registerHoverProvider('csharp', {
        provideHover: async function (model, position) {

            let request = {
                Code: model.getValue(),
                Position: model.getOffsetAt(position),
                Assemblies: assemblies
            }

            let resultQ = await sendRequest("hover", request);

            if (resultQ && resultQ.data) {
                posStart = model.getPositionAt(resultQ.data.OffsetFrom);
                posEnd = model.getPositionAt(resultQ.data.OffsetTo);

                return {
                    range: new monaco.Range(posStart.lineNumber, posStart.column, posEnd.lineNumber, posEnd.column),
                    contents: [
                        { value: resultQ.data.Information }
                    ]
                };
            }

            return null;
        }
    });

    monaco.editor.onDidCreateModel(function (model) {
        async function validate() {

            let request = {
                Code: model.getValue(),
                Assemblies: assemblies
            }

            let resultQ = await sendRequest("codeCheck", request)

            if (resultQ && resultQ.data) {
                let markers = [];

                for (let elem of resultQ.data) {
                    posStart = model.getPositionAt(elem.OffsetFrom);
                    posEnd = model.getPositionAt(elem.OffsetTo);
                    markers.push({
                        severity: elem.Severity,
                        startLineNumber: posStart.lineNumber,
                        startColumn: posStart.column,
                        endLineNumber: posEnd.lineNumber,
                        endColumn: posEnd.column,
                        message: elem.Message,
                        code: elem.Id
                    });
                }

                monaco.editor.setModelMarkers(model, 'csharp', markers);
            }
        }

        var handle = null;
        model.onDidChangeContent(() => {
            monaco.editor.setModelMarkers(model, 'csharp', []);
            clearTimeout(handle);
            handle = setTimeout(() => validate(), 500);
        });
        validate();
    });

}

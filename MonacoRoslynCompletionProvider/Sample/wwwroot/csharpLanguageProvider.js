async function sendRequest(type, request) {
    let endPoint;
    switch (type) {
        case 'complete': endPoint = '/completion/complete'; break;
        case 'resolve': endPoint = '/completion/resolve'; break;
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

function registerCsharpProvider(assemblies = []) {

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
                const mapTagToKind = {
                    'Class': monaco.languages.CompletionItemKind.Class,
                    'Delegate': monaco.languages.CompletionItemKind.Function,
                    'Enum': monaco.languages.CompletionItemKind.Enum,
                    'EnumMember': monaco.languages.CompletionItemKind.EnumMember,
                    'Event': monaco.languages.CompletionItemKind.Event,
                    'ExtensionMethod': monaco.languages.CompletionItemKind.Method,
                    'Field': monaco.languages.CompletionItemKind.Field,
                    'Interface': monaco.languages.CompletionItemKind.Interface,
                    'Keyword': monaco.languages.CompletionItemKind.Keyword,
                    'Local': monaco.languages.CompletionItemKind.Variable,
                    'Method': monaco.languages.CompletionItemKind.Method,
                    'Module': monaco.languages.CompletionItemKind.Module,
                    'Namespace': monaco.languages.CompletionItemKind.Module,
                    'Operator': monaco.languages.CompletionItemKind.Operator,
                    'Parameter': monaco.languages.CompletionItemKind.Variable,
                    'Property': monaco.languages.CompletionItemKind.Property,
                    'RangeVariable': monaco.languages.CompletionItemKind.Variable,
                    'Reference': monaco.languages.CompletionItemKind.Reference,
                    'Structure': monaco.languages.CompletionItemKind.Struct,
                    'TypeParameter': monaco.languages.CompletionItemKind.TypeParameter,
                    'Snippet': monaco.languages.CompletionItemKind.Snippet,
                    'Constant': monaco.languages.CompletionItemKind.Constant
                };

                for (let elem of resultQ.data) {
                    let kind = monaco.languages.CompletionItemKind.Function;
                    if (elem.Tag && mapTagToKind[elem.Tag]) {
                        kind = mapTagToKind[elem.Tag];
                    }
                    suggestions.push({
                        label: {
                            label: elem.Suggestion,
                            description: elem.Description
                        },
                        kind: kind,
                        insertText: elem.Suggestion,
                        // Store context for resolve
                        _roslynContext: {
                            Code: request.Code,
                            Position: request.Position,
                            Assemblies: assemblies,
                            Suggestion: elem.Suggestion
                        }
                    });
                }
            }

            return { suggestions: suggestions };
        },

        resolveCompletionItem: async (item, token) => {
            let context = item._roslynContext;
            if (!context) return item;

            let request = {
                Code: context.Code,
                Position: context.Position,
                Assemblies: context.Assemblies,
                Suggestion: context.Suggestion
            };

            let resultQ = await sendRequest("resolve", request);

            if (resultQ && resultQ.data && resultQ.data.Description) {
                // Update the description
                // Note: Monaco requires a new object or mutation of the label object
                if (typeof item.label === 'string') {
                    item.label = {
                        label: item.label,
                        description: resultQ.data.Description
                    };
                } else {
                    item.label.description = resultQ.data.Description;
                }

                // Also adding to documentation as it provides more space
                item.documentation = resultQ.data.Description;
            }

            return item;
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

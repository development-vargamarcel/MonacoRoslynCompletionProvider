
class CSharpLanguageProvider {
    constructor(assemblies = [], baseUrl = '/completion') {
        this.assemblies = assemblies;
        this.baseUrl = baseUrl;
        this.validationHandle = null;
        this.mapTagToKind = {
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
    }

    /**
     * Sends a request to the backend.
     * @param {string} endpoint - The API endpoint (relative to baseUrl).
     * @param {object} request - The request body.
     */
    async sendRequest(endpoint, request) {
        try {
            // Trigger loading event (can be hooked into UI later)
            this.onLoading(true);
            const response = await axios.post(`${this.baseUrl}/${endpoint}`, request);
            return response;
        } catch (error) {
            console.warn(`Failed to perform request: ${endpoint}`, error);
            this.onError(`Request failed: ${endpoint}`);
            return null;
        } finally {
            this.onLoading(false);
        }
    }

    /**
     * Placeholder for loading state change.
     * @param {boolean} isLoading
     */
    onLoading(isLoading) {}

    /**
     * Placeholder for error reporting.
     * @param {string} message
     */
    onError(message) {}

    /**
     * Registers all providers with Monaco.
     */
    register() {
        monaco.languages.registerCompletionItemProvider('csharp', {
            triggerCharacters: [".", " "],
            provideCompletionItems: (model, position) => this.provideCompletionItems(model, position),
            resolveCompletionItem: (item, token) => this.resolveCompletionItem(item, token)
        });

        monaco.languages.registerSignatureHelpProvider('csharp', {
            signatureHelpTriggerCharacters: ["("],
            signatureHelpRetriggerCharacters: [","],
            provideSignatureHelp: (model, position, token, context) => this.provideSignatureHelp(model, position, token, context)
        });

        monaco.languages.registerHoverProvider('csharp', {
            provideHover: (model, position) => this.provideHover(model, position)
        });

        monaco.languages.registerDocumentFormattingEditProvider('csharp', {
            provideDocumentFormattingEdits: (model, options, token) => this.provideDocumentFormattingEdits(model, options, token)
        });

        monaco.languages.registerDocumentRangeFormattingEditProvider('csharp', {
            provideDocumentRangeFormattingEdits: (model, range, options, token) => this.provideDocumentRangeFormattingEdits(model, range, options, token)
        });

        monaco.languages.registerDefinitionProvider('csharp', {
            provideDefinition: (model, position, token) => this.provideDefinition(model, position, token)
        });

        monaco.languages.registerRenameProvider('csharp', {
            provideRenameEdits: (model, position, newName, token) => this.provideRenameEdits(model, position, newName, token)
        });

        monaco.editor.onDidCreateModel((model) => this.setupValidation(model));
    }

    async provideRenameEdits(model, position, newName, token) {
        let request = {
            Code: model.getValue(),
            Position: model.getOffsetAt(position),
            NewName: newName,
            Assemblies: this.assemblies
        };

        let resultQ = await this.sendRequest("rename", request);

        if (resultQ && resultQ.data && resultQ.data.changesInDocument) {
            let edits = [];
            for (let change of resultQ.data.changesInDocument) {
                let posStart = model.getPositionAt(change.offsetFrom);
                let posEnd = model.getPositionAt(change.offsetTo);
                edits.push({
                    resource: model.uri,
                    versionId: model.getVersionId(),
                    textEdit: {
                        range: {
                            startLineNumber: posStart.lineNumber,
                            startColumn: posStart.column,
                            endLineNumber: posEnd.lineNumber,
                            endColumn: posEnd.column
                        },
                        text: change.newText
                    }
                });
            }

            return {
                edits: edits
            };
        }

        return {
            edits: []
        };
    }

    async provideDefinition(model, position, token) {
        let request = {
            Code: model.getValue(),
            Position: model.getOffsetAt(position),
            Assemblies: this.assemblies
        };

        let resultQ = await this.sendRequest("definition", request);

        if (resultQ && resultQ.data && resultQ.data.definitions) {
            let definitions = [];
            for (let def of resultQ.data.definitions) {
                let posStart = model.getPositionAt(def.offsetFrom);
                let posEnd = model.getPositionAt(def.offsetTo);
                definitions.push({
                    uri: model.uri, // Assuming definitions are in the same file for now
                    range: {
                        startLineNumber: posStart.lineNumber,
                        startColumn: posStart.column,
                        endLineNumber: posEnd.lineNumber,
                        endColumn: posEnd.column
                    }
                });
            }
            return definitions;
        }

        return [];
    }

    async provideDocumentFormattingEdits(model, options, token) {
        let request = {
            Code: model.getValue(),
            Assemblies: this.assemblies
        };

        let resultQ = await this.sendRequest("format", request);
        return this.mapFormatEdits(model, resultQ);
    }

    async provideDocumentRangeFormattingEdits(model, range, options, token) {
        let request = {
            Code: model.getValue(),
            Start: model.getOffsetAt(range.getStartPosition()),
            End: model.getOffsetAt(range.getEndPosition()),
            Assemblies: this.assemblies
        };

        let resultQ = await this.sendRequest("format", request);
        return this.mapFormatEdits(model, resultQ);
    }

    mapFormatEdits(model, resultQ) {
        if (resultQ && resultQ.data && resultQ.data.length > 0) {
            let elem = resultQ.data[0]; // Format document usually returns one 'CodeActionResult' with multiple changes
            if (elem.changesInDocument) {
                let edits = [];
                for (let change of elem.changesInDocument) {
                    let posStart = model.getPositionAt(change.offsetFrom);
                    let posEnd = model.getPositionAt(change.offsetTo);
                    edits.push({
                        range: {
                            startLineNumber: posStart.lineNumber,
                            startColumn: posStart.column,
                            endLineNumber: posEnd.lineNumber,
                            endColumn: posEnd.column
                        },
                        text: change.newText
                    });
                }
                return edits;
            }
        }
        return [];
    }

    async provideCompletionItems(model, position) {
        let suggestions = [];

        let request = {
            Code: model.getValue(),
            Position: model.getOffsetAt(position),
            Assemblies: this.assemblies
        };

        let resultQ = await this.sendRequest("complete", request);

        if (resultQ && resultQ.data) {
            for (let elem of resultQ.data) {
                let kind = monaco.languages.CompletionItemKind.Function;
                if (elem.tag && this.mapTagToKind[elem.tag]) {
                    kind = this.mapTagToKind[elem.tag];
                }
                suggestions.push({
                    label: {
                        label: elem.suggestion,
                        description: elem.description || ""
                    },
                    kind: kind,
                    insertText: elem.suggestion,
                    // Store context for resolve
                    _roslynContext: {
                        Code: request.Code,
                        Position: request.Position,
                        Assemblies: this.assemblies,
                        Suggestion: elem.suggestion
                    }
                });
            }
        }

        return { suggestions: suggestions };
    }

    async resolveCompletionItem(item, token) {
        let context = item._roslynContext;
        if (!context) return item;

        let request = {
            Code: context.Code,
            Position: context.Position,
            Assemblies: context.Assemblies,
            Suggestion: context.Suggestion
        };

        let resultQ = await this.sendRequest("resolve", request);

        if (resultQ && resultQ.data && resultQ.data.description) {
            if (typeof item.label === 'string') {
                item.label = {
                    label: item.label,
                    description: resultQ.data.description
                };
            } else {
                item.label.description = resultQ.data.description;
            }
            item.documentation = resultQ.data.description;
        }

        return item;
    }

    async provideSignatureHelp(model, position, token, context) {
        let request = {
            Code: model.getValue(),
            Position: model.getOffsetAt(position),
            Assemblies: this.assemblies
        };

        let resultQ = await this.sendRequest("signature", request);
        if (!resultQ || !resultQ.data) return;

        let signatures = [];
        if (resultQ.data.signatures) {
            for (let signature of resultQ.data.signatures) {
                let params = [];
                if (signature.parameters) {
                    for (let param of signature.parameters) {
                        params.push({
                            label: param.label,
                            documentation: param.documentation ?? ""
                        });
                    }
                }

                signatures.push({
                    label: signature.label,
                    documentation: signature.documentation ?? "",
                    parameters: params,
                });
            }
        }

        return {
            value: {
                signatures: signatures,
                activeParameter: resultQ.data.activeParameter,
                activeSignature: resultQ.data.activeSignature
            },
            dispose: () => { }
        };
    }

    async provideHover(model, position) {
        let request = {
            Code: model.getValue(),
            Position: model.getOffsetAt(position),
            Assemblies: this.assemblies
        };

        let resultQ = await this.sendRequest("hover", request);

        if (resultQ && resultQ.data) {
            let posStart = model.getPositionAt(resultQ.data.offsetFrom);
            let posEnd = model.getPositionAt(resultQ.data.offsetTo);

            return {
                range: new monaco.Range(posStart.lineNumber, posStart.column, posEnd.lineNumber, posEnd.column),
                contents: [
                    { value: resultQ.data.information }
                ]
            };
        }

        return null;
    }

    setupValidation(model) {
        const validate = async () => {
            let request = {
                Code: model.getValue(),
                Assemblies: this.assemblies
            };

            let resultQ = await this.sendRequest("codeCheck", request);

            let markers = [];
            if (resultQ && resultQ.data) {
                for (let elem of resultQ.data) {
                    let posStart = model.getPositionAt(elem.offsetFrom);
                    let posEnd = model.getPositionAt(elem.offsetTo);
                    markers.push({
                        severity: elem.severity, // Ensure this maps correctly to Monaco severity
                        startLineNumber: posStart.lineNumber,
                        startColumn: posStart.column,
                        endLineNumber: posEnd.lineNumber,
                        endColumn: posEnd.column,
                        message: elem.message,
                        code: elem.id
                    });
                }
            }
            monaco.editor.setModelMarkers(model, 'csharp', markers);

            // Notify validation complete (can be hooked into UI)
            this.onValidationComplete(markers);
        };

        model.onDidChangeContent(() => {
            monaco.editor.setModelMarkers(model, 'csharp', []);
            clearTimeout(this.validationHandle);
            this.validationHandle = setTimeout(() => validate(), 500);
        });

        // Initial validation
        validate();
    }

    /**
     * Placeholder for validation complete event.
     * @param {Array} markers
     */
    onValidationComplete(markers) {}
}

// Global instance for convenience and backward compatibility
var csharpProviderInstance;

function registerCsharpProvider(assemblies = []) {
    csharpProviderInstance = new CSharpLanguageProvider(assemblies);
    csharpProviderInstance.register();
    return csharpProviderInstance;
}

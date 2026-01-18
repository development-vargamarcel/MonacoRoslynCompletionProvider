using MonacoRoslynCompletionProvider;
using MonacoRoslynCompletionProvider.Api;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/completion/complete", async (HttpContext e) =>
{
    using var reader = new StreamReader(e.Request.Body);
    string text = await reader.ReadToEndAsync();
    var tabCompletionRequest = JsonSerializer.Deserialize<TabCompletionRequest>(text);
    var tabCompletionResults = await CompletionRequestHandler.Handle(tabCompletionRequest);
    await JsonSerializer.SerializeAsync(e.Response.Body, tabCompletionResults);
});

app.MapPost("/completion/signature", async (HttpContext e) =>
{
    using var reader = new StreamReader(e.Request.Body);
    string text = await reader.ReadToEndAsync();
    var signatureHelpRequest = JsonSerializer.Deserialize<SignatureHelpRequest>(text);
    var signatureHelpResult = await CompletionRequestHandler.Handle(signatureHelpRequest);
    await JsonSerializer.SerializeAsync(e.Response.Body, signatureHelpResult);
});

app.MapPost("/completion/hover", async (HttpContext e) =>
{
    using var reader = new StreamReader(e.Request.Body);
    string text = await reader.ReadToEndAsync();
    var hoverInfoRequest = JsonSerializer.Deserialize<HoverInfoRequest>(text);
    var hoverInfoResult = await CompletionRequestHandler.Handle(hoverInfoRequest);
    await JsonSerializer.SerializeAsync(e.Response.Body, hoverInfoResult);
});

app.MapPost("/completion/codeCheck", async (HttpContext e) =>
{
    using var reader = new StreamReader(e.Request.Body);
    string text = await reader.ReadToEndAsync();
    var codeCheckRequest = JsonSerializer.Deserialize<CodeCheckRequest>(text);
    var codeCheckResults = await CompletionRequestHandler.Handle(codeCheckRequest);
    await JsonSerializer.SerializeAsync(e.Response.Body, codeCheckResults);
});

app.UseFileServer();

app.Run();

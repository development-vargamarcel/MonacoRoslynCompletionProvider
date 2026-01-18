using MonacoRoslynCompletionProvider;
using MonacoRoslynCompletionProvider.Api;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ICompletionService, CompletionService>();
var app = builder.Build();

app.MapPost("/completion/complete", async (HttpContext e, ICompletionService completionService) =>
{
    using var reader = new StreamReader(e.Request.Body);
    string text = await reader.ReadToEndAsync();
    var tabCompletionRequest = JsonSerializer.Deserialize<TabCompletionRequest>(text);
    var tabCompletionResults = await completionService.GetTabCompletion(tabCompletionRequest);
    await JsonSerializer.SerializeAsync(e.Response.Body, tabCompletionResults);
});

app.MapPost("/completion/signature", async (HttpContext e, ICompletionService completionService) =>
{
    using var reader = new StreamReader(e.Request.Body);
    string text = await reader.ReadToEndAsync();
    var signatureHelpRequest = JsonSerializer.Deserialize<SignatureHelpRequest>(text);
    var signatureHelpResult = await completionService.GetSignatureHelp(signatureHelpRequest);
    await JsonSerializer.SerializeAsync(e.Response.Body, signatureHelpResult);
});

app.MapPost("/completion/hover", async (HttpContext e, ICompletionService completionService) =>
{
    using var reader = new StreamReader(e.Request.Body);
    string text = await reader.ReadToEndAsync();
    var hoverInfoRequest = JsonSerializer.Deserialize<HoverInfoRequest>(text);
    var hoverInfoResult = await completionService.GetHoverInformation(hoverInfoRequest);
    await JsonSerializer.SerializeAsync(e.Response.Body, hoverInfoResult);
});

app.MapPost("/completion/codeCheck", async (HttpContext e, ICompletionService completionService) =>
{
    using var reader = new StreamReader(e.Request.Body);
    string text = await reader.ReadToEndAsync();
    var codeCheckRequest = JsonSerializer.Deserialize<CodeCheckRequest>(text);
    var codeCheckResults = await completionService.GetCodeCheckResults(codeCheckRequest);
    await JsonSerializer.SerializeAsync(e.Response.Body, codeCheckResults);
});

app.UseFileServer();

app.Run();

using Microsoft.AspNetCore.Mvc;
using MonacoRoslynCompletionProvider;
using MonacoRoslynCompletionProvider.Api;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ICompletionService, CompletionService>();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();

        if (exceptionHandlerPathFeature?.Error != null)
        {
            logger.LogError(exceptionHandlerPathFeature.Error, "Unhandled exception occurred.");
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
    });
});

app.MapPost("/completion/complete", async ([FromBody] TabCompletionRequest request, ICompletionService completionService) =>
{
    return await completionService.GetTabCompletion(request);
});

app.MapPost("/completion/resolve", async ([FromBody] CompletionResolveRequest request, ICompletionService completionService) =>
{
    return await completionService.GetCompletionResolve(request);
});

app.MapPost("/completion/signature", async ([FromBody] SignatureHelpRequest request, ICompletionService completionService) =>
{
    return await completionService.GetSignatureHelp(request);
});

app.MapPost("/completion/hover", async ([FromBody] HoverInfoRequest request, ICompletionService completionService) =>
{
    return await completionService.GetHoverInformation(request);
});

app.MapPost("/completion/codeCheck", async ([FromBody] CodeCheckRequest request, ICompletionService completionService) =>
{
    return await completionService.GetCodeCheckResults(request);
});

app.UseFileServer();

app.Run();

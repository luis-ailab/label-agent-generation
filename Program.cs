using Azure.AI.Projects;
using Azure.Identity;
using Label.Agent.Generation.Contracts;
using Label.Agent.Generation.Services;
using Microsoft.Agents.AI;

var builder = WebApplication.CreateBuilder(args);
string endpoint = builder.Configuration["FoundryProjectEndpoint"]
    ?? throw new InvalidOperationException("FoundryProjectEndpoint is required.");
string model = builder.Configuration["ModelDeploymentName"]
    ?? throw new InvalidOperationException("ModelDeploymentName is required.");
var projectClient = new AIProjectClient(
    new Uri(endpoint), new AzureCliCredential());
builder.Services.AddSingleton(projectClient);
builder.Services.AddSingleton<AIAgent>(_ => projectClient.AsAIAgent(
    model: model,
    name: "LabelGenerationAgent",
    instructions: "Generate structured label-content candidates from verified inputs only. Return JSON only."));
builder.Services.AddSingleton<GenerationResponseParser>();
builder.Services.AddSingleton<LabelGenerationService>();
builder.Services.AddSingleton<CandidateExpansionService>();
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "Label.Agent.Generation",
    timestamp = DateTimeOffset.UtcNow
}));
app.MapPost("/api/generation/candidates", async (
    LabelGenerationRequest request,
    LabelGenerationService service,
    CancellationToken cancellationToken) =>
    Results.Ok(await service.GenerateAsync(request, cancellationToken)));
app.MapPost("/api/generation/expand", async (
    CandidateExpansionRequest request,
    CandidateExpansionService service,
    CancellationToken cancellationToken) =>
    Results.Ok(await service.ExpandAsync(request, cancellationToken)));
app.Run();

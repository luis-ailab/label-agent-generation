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
var projectClient = new AIProjectClient(new Uri(endpoint), new AzureCliCredential());
builder.Services.AddSingleton(projectClient);
builder.Services.AddSingleton<AIAgent>(_ => projectClient.AsAIAgent(
    model: model, name: "LabelGenerationAgent",
    instructions: "You generate structured label-content candidates from verified inputs. Never invent facts. Return JSON only."));
builder.Services.AddSingleton<GenerationResponseParser>();
builder.Services.AddSingleton<LabelGenerationService>();
builder.Services.AddProblemDetails();
builder.Services.AddCors(o => o.AddPolicy("Allowed", p => {
    var origins=builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    if(origins.Length>0) p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
}));
var app=builder.Build();
app.UseExceptionHandler(); app.UseCors("Allowed");
app.MapGet("/health",()=>Results.Ok(new {status="Healthy",service="Label.Agent.Generation",timestamp=DateTimeOffset.UtcNow}));
app.MapPost("/api/generation/candidates", async (LabelGenerationRequest request, LabelGenerationService service, CancellationToken ct) =>
    Results.Ok(await service.GenerateAsync(request,ct)));
app.Run();

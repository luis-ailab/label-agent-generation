using Label.Agent.Generation.Contracts;
using Microsoft.Agents.AI;
namespace Label.Agent.Generation.Services;

public sealed class LabelGenerationService(AIAgent generationAgent, GenerationResponseParser parser)
{
    public async Task<LabelGenerationResponse> GenerateAsync(LabelGenerationRequest request, CancellationToken ct)
    {
        Validate(request);
        AgentSession session = await generationAgent.CreateSessionAsync(ct);
        AgentResponse response = await generationAgent.RunAsync(
            GenerationPromptBuilder.Build(request), session, options: null, cancellationToken: ct);
        var candidates = parser.Parse(response.ToString(), request.CandidateCount);
        return new($"gen-{Guid.NewGuid():N}", request.Template.Id, candidates,
            DateTimeOffset.UtcNow, "candidate-generation-v1");
    }
    private static void Validate(LabelGenerationRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.UserRequest) || string.IsNullOrWhiteSpace(r.ProductInformation) ||
            string.IsNullOrWhiteSpace(r.RegulatoryGuidance) || r.Template is null)
            throw new ArgumentException("UserRequest, ProductInformation, RegulatoryGuidance, and Template are required.");
        if (r.CandidateCount != 3) throw new ArgumentException("Phase 2 requires exactly three candidates.");
        if (r.Template.Sections.Count == 0) throw new ArgumentException("The selected template must contain sections.");
    }
}

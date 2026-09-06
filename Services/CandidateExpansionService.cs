using System.Text.Json;
using Label.Agent.Generation.Contracts;
using Microsoft.Agents.AI;

namespace Label.Agent.Generation.Services;

public sealed class CandidateExpansionService(AIAgent generationAgent)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed record Envelope(IReadOnlyList<LabelCandidate> Candidates);

    public async Task<CandidateExpansionResponse> ExpandAsync(
        CandidateExpansionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ChildCount is < 1 or > 3)
            throw new ArgumentException("ChildCount must be between 1 and 3.");

        AgentSession session = await generationAgent.CreateSessionAsync(
            cancellationToken);
        AgentResponse response = await generationAgent.RunAsync(
            ExpansionPromptBuilder.Build(request),
            session,
            options: null,
            cancellationToken: cancellationToken);

        string json = StripMarkdownFences(response.ToString());
        Envelope envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<Envelope>(json, Options)
                ?? throw new InvalidOperationException(
                    "The expansion model returned an empty response.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                "The expansion model returned invalid JSON.", ex);
        }

        if (envelope.Candidates.Count != request.ChildCount)
            throw new InvalidOperationException(
                $"The expansion model must return exactly {request.ChildCount} candidates.");

        var expected = Enumerable.Range(1, request.ChildCount)
            .Select(index => $"{request.ParentCandidate.Id}{index}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var actual = envelope.Candidates.Select(item => item.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!expected.SetEquals(actual))
            throw new InvalidOperationException(
                "Expanded candidate identifiers do not match the required parent-child convention.");

        return new CandidateExpansionResponse(
            $"expand-{Guid.NewGuid():N}",
            request.ParentCandidate.Id,
            envelope.Candidates,
            DateTimeOffset.UtcNow,
            "candidate-expansion-v1");
    }

    private static string StripMarkdownFences(string value)
    {
        string trimmed = value.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal)) return trimmed;
        int firstLineEnd = trimmed.IndexOf('\n');
        int finalFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
        return firstLineEnd >= 0 && finalFence > firstLineEnd
            ? trimmed[(firstLineEnd + 1)..finalFence].Trim()
            : trimmed;
    }
}

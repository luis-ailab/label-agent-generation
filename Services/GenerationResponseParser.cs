using System.Text.Json;
using Label.Agent.Generation.Contracts;

namespace Label.Agent.Generation.Services;

public sealed class GenerationResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions =
    new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private sealed record Envelope(
        IReadOnlyList<LabelCandidate> Candidates);

    public IReadOnlyList<LabelCandidate> Parse(
        string response,
        int expectedCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(response);

        string json = StripMarkdownFences(response);

        Envelope? envelope;

        try
        {
            envelope = JsonSerializer.Deserialize<Envelope>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                "The generation model returned invalid JSON.",
                ex);
        }

        if (envelope?.Candidates is null ||
            envelope.Candidates.Count != expectedCount)
        {
            throw new InvalidOperationException(
                $"Generation model must return exactly {expectedCount} candidates.");
        }

        if (envelope.Candidates.Any(candidate =>
                string.IsNullOrWhiteSpace(candidate.Id) ||
                string.IsNullOrWhiteSpace(candidate.Strategy) ||
                candidate.Sections is null ||
                candidate.Sections.Count == 0))
        {
            throw new InvalidOperationException(
                "One or more generated candidates are incomplete.");
        }

        return envelope.Candidates;
    }

    private static string StripMarkdownFences(string value)
    {
        string trimmed = value.Trim();

        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        int firstLineEnd = trimmed.IndexOf('\n');
        int finalFence = trimmed.LastIndexOf(
            "```",
            StringComparison.Ordinal);

        if (firstLineEnd < 0 || finalFence <= firstLineEnd)
        {
            return trimmed;
        }

        return trimmed[(firstLineEnd + 1)..finalFence].Trim();
    }
}

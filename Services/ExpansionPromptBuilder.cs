using System.Text.Json;
using Label.Agent.Generation.Contracts;

namespace Label.Agent.Generation.Services;

public static class ExpansionPromptBuilder
{
    public static string Build(CandidateExpansionRequest request)
    {
        string input = JsonSerializer.Serialize(request,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

        return $$"""
Create exactly {{request.ChildCount}} improved child candidates from the supplied
parent candidate. Preserve verified facts and template structure. Improve the
parent by addressing the supplied evaluation risks while preserving strengths.
Do not invent product facts, claims, quantities, warnings, directions, approvals,
or missing legal information. Keep required review placeholders when verified
content is unavailable.

Child identifiers must be exactly the parent identifier followed by 1 through
{{request.ChildCount}}. Example: parent A produces A1 and A2.

Return JSON only, without Markdown fences:
{
  "candidates": [
    {
      "id": "A1",
      "strategy": "brief child strategy",
      "summary": "brief improvement summary",
      "sections": [
        { "key": "string", "displayName": "string", "content": "string" }
      ],
      "assumptions": ["string"],
      "reviewFlags": ["string"]
    }
  ]
}

Input:
{{input}}
""";
    }
}

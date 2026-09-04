using System.Text.Json;
using Label.Agent.Generation.Contracts;
namespace Label.Agent.Generation.Services;

public static class GenerationPromptBuilder
{
    public static string Build(LabelGenerationRequest request)
    {
        var input = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
        return $$"""
Generate exactly {{request.CandidateCount}} materially different candidate label-content packages.
The candidates must use these strategies in order:
A: Scientific and regulatory clarity
B: Consumer-friendly clarity
C: Brand-oriented clarity while remaining conservative and compliant

Hard requirements:
- Use only facts present in productInformation.
- Respect regulatoryGuidance, template sections, section rules, and content rules.
- Do not invent ingredients, quantities, claims, warnings, directions, certifications, or approvals.
- Include every required template section. If verified content is missing, use "[REVIEW REQUIRED: missing verified content]" and add a review flag.
- These are content candidates, not final artwork or legal approval.
- Return valid JSON only, with no Markdown fences.

Required response shape:
{
  "candidates": [
    {
      "id": "A",
      "strategy": "Scientific and regulatory clarity",
      "summary": "brief description",
      "sections": [{ "key": "string", "displayName": "string", "content": "string" }],
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

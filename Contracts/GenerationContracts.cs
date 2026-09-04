namespace Label.Agent.Generation.Contracts;

public sealed record LabelGenerationRequest(
    string UserRequest,
    string ProductInformation,
    string RegulatoryGuidance,
    GenerationTemplate Template,
    int CandidateCount = 3);

public sealed record GenerationTemplate(
    string Id,
    string Name,
    IReadOnlyList<GenerationTemplateSection> Sections,
    IReadOnlyList<string> ContentRules);

public sealed record GenerationTemplateSection(
    string Key, string DisplayName, bool Required, int Order,
    string Region, IReadOnlyList<string> Rules);

public sealed record LabelSectionContent(
    string Key, string DisplayName, string Content);

public sealed record LabelCandidate(
    string Id, string Strategy, string Summary,
    IReadOnlyList<LabelSectionContent> Sections,
    IReadOnlyList<string> Assumptions,
    IReadOnlyList<string> ReviewFlags);

public sealed record LabelGenerationResponse(
    string GenerationId, string TemplateId,
    IReadOnlyList<LabelCandidate> Candidates,
    DateTimeOffset GeneratedAtUtc, string GeneratorVersion);

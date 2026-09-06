namespace Label.Agent.Generation.Contracts;

public sealed record CandidateExpansionRequest(
    string UserRequest,
    string ProductInformation,
    string RegulatoryGuidance,
    GenerationTemplate Template,
    LabelCandidate ParentCandidate,
    ParentEvaluation ParentEvaluation,
    int ChildCount = 2);

public sealed record ParentEvaluation(
    string CandidateId, int Compliance, int Readability,
    int BrandAlignment, int ConsumerClarity, int OverallScore,
    IReadOnlyList<string> Strengths, IReadOnlyList<string> Risks,
    string RationaleSummary);

public sealed record CandidateExpansionResponse(
    string ExpansionId,
    string ParentCandidateId,
    IReadOnlyList<LabelCandidate> Candidates,
    DateTimeOffset GeneratedAtUtc,
    string GeneratorVersion);

using FormDesignerAPI.Core.FormContext.Aggregates;
using FormDesignerAPI.Core.FormContext.ValueObjects;

namespace FormDesignerAPI.UseCases.Interfaces;

/// <summary>
/// Infrastructure service for generating code
/// </summary>
public interface ICodeGenerator
{
    Task<GeneratedCodeOutput> GenerateCodeAsync(
        Form form,
        string formName,
        CodeNamespace targetNamespace,
        string? formPurpose,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Generated code output (infrastructure DTO)
/// </summary>
public class GeneratedCodeOutput
{
    public string ModelCode { get; init; } = string.Empty;
    public string ControllerCode { get; init; } = string.Empty;
    public string ViewCode { get; init; } = string.Empty;
    public string MigrationCode { get; init; } = string.Empty;
    public GeneratedCodeAnalysis Analysis { get; init; } = new();
}

public class GeneratedCodeAnalysis
{
    public string DetectedPurpose { get; init; } = string.Empty;
    public string Complexity { get; init; } = string.Empty;
    public bool HasRepeatingData { get; init; }
    public string RecommendedApproach { get; init; } = string.Empty;
}
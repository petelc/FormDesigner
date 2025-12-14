using MediatR;

namespace FormDesignerAPI.UseCases.Commands.AnalyzeForm;

/// <summary>
/// Command to analyze a PDF form
/// </summary>
public record AnalyzeFormCommand(
    Stream PdfStream,
    string FileName
) : IRequest<AnalyzeFormResult>;
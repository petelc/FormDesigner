using FluentValidation;

namespace FormDesignerAPI.UseCases.Commands.AnalyzeForm;

/// <summary>
/// Validator for AnalyzeFormCommand
/// </summary>
public class AnalyzeFormCommandValidator : AbstractValidator<AnalyzeFormCommand>
{
    public AnalyzeFormCommandValidator()
    {
        RuleFor(x => x.PdfStream)
            .NotNull()
            .WithMessage("PDF stream is required");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .Must(BeAPdfFile)
            .WithMessage("File must be a PDF (.pdf extension)");

        RuleFor(x => x.PdfStream)
            .Must(HaveValidSize)
            .When(x => x.PdfStream != null)
            .WithMessage("PDF file size must be between 1 KB and 50 MB");
    }

    private bool BeAPdfFile(string fileName)
    {
        return fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    private bool HaveValidSize(Stream stream)
    {
        const long minSize = 1024; // 1 KB
        const long maxSize = 50 * 1024 * 1024; // 50 MB

        return stream.Length >= minSize && stream.Length <= maxSize;
    }
}
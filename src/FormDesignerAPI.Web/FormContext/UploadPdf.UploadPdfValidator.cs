using FluentValidation;

namespace FormDesignerAPI.Web.FormContext;

public class UploadPdfValidator : Validator<UploadPdfRequest>
{
    public UploadPdfValidator()
    {
        RuleFor(x => x.PdfFile)
            .NotNull()
            .WithMessage("PDF file is required");

        RuleFor(x => x.PdfFile!.Length)
            .GreaterThan(0)
            .WithMessage("PDF file cannot be empty")
            .When(x => x.PdfFile != null);

        RuleFor(x => x.PdfFile!.ContentType)
            .Equal("application/pdf")
            .WithMessage("File must be a PDF")
            .When(x => x.PdfFile != null);

        RuleFor(x => x.PdfFile!.Length)
            .LessThanOrEqualTo(10 * 1024 * 1024) // 10MB limit
            .WithMessage("PDF file must be less than 10MB")
            .When(x => x.PdfFile != null);
    }
}

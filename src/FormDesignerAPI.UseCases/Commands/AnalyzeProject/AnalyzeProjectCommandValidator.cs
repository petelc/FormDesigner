using FluentValidation;

namespace FormDesignerAPI.UseCases.Commands.AnalyzeProject;

/// <summary>
/// Validator for AnalyzeProjectCommand
/// </summary>
public class AnalyzeProjectCommandValidator : AbstractValidator<AnalyzeProjectCommand>
{
    public AnalyzeProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEqual(Guid.Empty)
            .WithMessage("Project ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}

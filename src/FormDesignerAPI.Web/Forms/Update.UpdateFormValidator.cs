using FluentValidation;
using FormDesignerAPI.Infrastructure.Data.Config;

namespace FormDesignerAPI.Web.Forms;

/// <summary>
/// Validator for UpdateFormRequest.
/// Version parameters and FormDefinitionPath are optional - if not provided, current values are retained.
/// </summary>
public class UpdateFormValidator : Validator<UpdateFormRequest>
{
    public UpdateFormValidator()
    {
        RuleFor(x => x.FormId)
            .NotEmpty()
            .WithMessage("FormId is required.");

        RuleFor(x => x.FormNumber)
            .NotEmpty()
            .WithMessage("FormNumber is required.")
            .MinimumLength(7).WithMessage("FormNumber must be at least 7 characters long.")
            .MaximumLength(DataSchemaConstants.DEFAULT_FORM_NUMBER_LENGTH).WithMessage($"FormNumber must not exceed {DataSchemaConstants.DEFAULT_FORM_NUMBER_LENGTH} characters.");

        RuleFor(x => x.FormTitle)
            .NotEmpty()
            .WithMessage("FormTitle is required.");

        RuleFor(x => x.Division)
            .NotEmpty()
            .WithMessage("Division is required.");

        RuleFor(x => x.Owner)
            .NotEmpty()
            .WithMessage("Owner is required.");

        RuleFor(x => x.OwnerEmail)
            .NotEmpty()
            .WithMessage("OwnerEmail is required.")
            .EmailAddress()
            .WithMessage("OwnerEmail must be a valid email address.");

        // Version parameters are optional - if provided, all three must be present and valid
        When(x => x.VersionMajor.HasValue || x.VersionMinor.HasValue || x.VersionPatch.HasValue, () =>
        {
            RuleFor(x => x.VersionMajor)
                .NotNull()
                .WithMessage("VersionMajor is required when providing version parameters.")
                .GreaterThan(0)
                .WithMessage("VersionMajor must be greater than 0.");

            RuleFor(x => x.VersionMinor)
                .NotNull()
                .WithMessage("VersionMinor is required when providing version parameters.")
                .GreaterThanOrEqualTo(0)
                .WithMessage("VersionMinor must be greater than or equal to 0.");

            RuleFor(x => x.VersionPatch)
                .NotNull()
                .WithMessage("VersionPatch is required when providing version parameters.")
                .GreaterThanOrEqualTo(0)
                .WithMessage("VersionPatch must be greater than or equal to 0.");
        });

        // FormDefinitionPath is optional
        RuleFor(x => x.FormDefinitionPath)
            .MaximumLength(500)
            .WithMessage("FormDefinitionPath must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.FormDefinitionPath));
    }
}



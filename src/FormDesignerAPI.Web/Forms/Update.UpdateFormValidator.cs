using FluentValidation;
using FormDesignerAPI.Infrastructure.Data.Config;

namespace FormDesignerAPI.Web.Forms;

public class UpdateFormValidator : Validator<UpdateFormRequest>
{
    public UpdateFormValidator()
    {
        RuleFor(x => x.FormId)
            .Must((args, formId) => args.Id == formId)
            .WithMessage("Route and body Ids must match; cannot update Id of an existing resource.");

        // RuleFor(x => x.Id)
        //     .GreaterThan(0).WithMessage("Id must be greater than 0.");

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

        RuleFor(x => x.Version)
            .NotEmpty()
            .WithMessage("Version is required.");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required.");
    }
}


using FluentValidation;
using FormDesignerAPI.Infrastructure.Data.Config;

namespace FormDesignerAPI.Web.Forms;

public class CreateFormValidator : Validator<CreateFormRequest>
{
    public CreateFormValidator()
    {
        RuleFor(x => x.FormNumber)
            .NotEmpty()
            .WithMessage("FormNumber is required.")
            .MinimumLength(7).WithMessage("FormNumber must be at least 7 characters long.")
            .MaximumLength(DataSchemaConstants.DEFAULT_FORM_NUMBER_LENGTH).WithMessage($"FormNumber must not exceed {DataSchemaConstants.DEFAULT_FORM_NUMBER_LENGTH} characters.");

        RuleFor(x => x.FormTitle)
            .NotEmpty()
            .WithMessage("Form Title is required.");
    }
}

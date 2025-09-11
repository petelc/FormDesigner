using FluentValidation;

namespace FormDesignerAPI.Web.Forms;

public class CreateFormValidator : Validator<CreateFormRequest>
{
    public CreateFormValidator()
    {
        RuleFor(x => x.FormNumber)
            .NotEmpty()
            .WithMessage("Form Number is required.");
        RuleFor(x => x.FormTitle)
            .NotEmpty()
            .WithMessage("Form Title is required.");
    }
}

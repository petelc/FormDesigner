using FluentValidation;

namespace FormDesignerAPI.Web.Forms;

/// <summary>
/// See: https://fast-endpoints.com/docs/validation
/// </summary>
public class DeleteFormValidator : Validator<DeleteFormRequest>
{
    public DeleteFormValidator()
    {
        // Add some validation rules here in the future.
        RuleFor(x => x.FormId)
            .GreaterThan(0);
    }
}


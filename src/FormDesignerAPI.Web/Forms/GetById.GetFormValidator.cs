using FluentValidation;

namespace FormDesignerAPI.Web.Forms;

public class GetFormValidator : Validator<GetFormByIdRequest>
{
    public GetFormValidator()
    {
        // RuleFor(x => x.FormId)
        // .GreaterThan(0);
    }
}

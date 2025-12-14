using FormDesignerAPI.Core.FormContext.Aggregates;

namespace FormDesignerAPI.Core.FormContext.Specifications;

public class FormByIdSpec : Specification<Form>
{
    public FormByIdSpec(Guid formId) =>
        Query.Where(form => form.Id == formId);

}

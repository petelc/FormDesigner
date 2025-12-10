namespace FormDesignerAPI.Core.FormContext.Aggregates.Specifications;

public class FormByIdSpec : Specification<Form>
{
    public FormByIdSpec(Guid formId) =>
        Query.Where(form => form.Id == formId);

}

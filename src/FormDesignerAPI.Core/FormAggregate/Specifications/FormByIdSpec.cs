namespace FormDesignerAPI.Core.FormAggregate.Specifications;

public class FormByIdSpec : Specification<Form>
{
    public FormByIdSpec(int formId)
    {
        Query.Where(form => form.Id == formId);
    }
}

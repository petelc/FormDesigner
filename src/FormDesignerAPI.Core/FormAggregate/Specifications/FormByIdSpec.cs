namespace FormDesignerAPI.Core.FormAggregate.Specifications;

public class FormByIdSpec : Specification<Form>
{
    public FormByIdSpec(Guid FormId) =>
        Query.Where(form => form.Id == FormId);

}

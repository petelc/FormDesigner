namespace FormDesignerAPI.Core.FormAggregate.Specifications;

public class FormByIdSpec : Specification<Form>
{
    public FormByIdSpec(int FormId) =>
        Query.Where(form => form.Id == FormId);

}

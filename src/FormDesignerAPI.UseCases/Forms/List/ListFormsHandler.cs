namespace FormDesignerAPI.UseCases.Forms.List;

/// <summary>
/// Handler for listing forms
/// </summary>
public class ListFormsHandler(IListFormsQueryService queryService)
    : IRequestHandler<ListFormsQuery, Result<IEnumerable<FormDTO>>>
{
    public async Task<Result<IEnumerable<FormDTO>>> Handle(
        ListFormsQuery request,
        CancellationToken cancellationToken)
    {
        var forms = await queryService.ListFormsAsync();
        return Result.Success(forms);
    }
}

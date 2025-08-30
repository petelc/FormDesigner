namespace FormDesignerAPI.UseCases.Forms.List;

public class ListFormsQueryHandler(IListFormsQueryService _query)
  : IQueryHandler<ListFormsQueryService, Result<IEnumerable<FormDTO>>>
{
    public async Task<Result<IEnumerable<FormDTO>>> Handle(ListFormsQueryService request, CancellationToken cancellationToken)
    {
        var forms = await _query.ListFormsAsync();
        return Result.Success(forms);
    }
}

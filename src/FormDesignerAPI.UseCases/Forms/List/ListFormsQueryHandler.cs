namespace FormDesignerAPI.UseCases.Forms.List;

public class ListFormsQueryHandler(
  IListFormsQueryService _query)
  : IQueryHandler<ListFormsQuery,
  Result<IEnumerable<FormDTO>>>
{
  public async Task<Result<IEnumerable<FormDTO>>> Handle(ListFormsQuery request, CancellationToken cancellationToken)
  {
    var forms = await _query.ListFormsAsync();
    return Result.Success(forms);
  }


}

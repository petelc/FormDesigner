using FastEndpoints;
namespace FormDesignerAPI.UseCases.Forms.List;

public record ListFormsQueryService(int? Skip, int? Take) : IQuery<Result<IEnumerable<FormDTO>>>;

public record ListFormsQuery2(int? Skip, int? Take) : FastEndpoints.ICommand<Result<IEnumerable<FormDTO>>>;

public class ListFormsQueryHandler2 : CommandHandler<ListFormsQuery2, Result<IEnumerable<FormDTO>>>
{
    private readonly IListFormsQueryService _query;

    public ListFormsQueryHandler2(IListFormsQueryService query)
    {
        _query = query;
    }

    public override async Task<Result<IEnumerable<FormDTO>>> ExecuteAsync(ListFormsQuery2 request, CancellationToken cancellationToken)
    {
        var forms = await _query.ListFormsAsync();
        return Result.Success(forms);
    }
}

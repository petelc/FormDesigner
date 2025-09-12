using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.Core.Interfaces;

public interface IFormUpdateService
{
    public Task<Result> UpdateFormAsync(int formId, FormUpdateDto formUpdateDto, CancellationToken cancellationToken);
}

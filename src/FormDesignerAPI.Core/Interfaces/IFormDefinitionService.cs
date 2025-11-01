namespace FormDesignerAPI.Core.Interfaces;

public interface IFormDefinitionService
{
    public Task<Result> GenerateFormDefinitionAsync(int formId, string json, CancellationToken cancellationToken);
}

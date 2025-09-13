namespace FormDesignerAPI.Core.Interfaces;

public interface IDeleteFormService
{
    // This service and method exist to provide a place in which to fire domain events
    // when deleting this aggregate root entity
    public Task<Result> DeleteFormAsync(int formId);
}

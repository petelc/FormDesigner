namespace FormDesignerAPI.Core.Interfaces;

public interface IDeletedFormService
{
    // This service and method exist to provide a place in which to fire domain events
    // when deleting this aggregate root entity
    public Task<Result> DeleteForm(int formId);
}

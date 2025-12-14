namespace FormDesignerAPI.UseCases.Forms.List;

/// <summary>
/// Query service interface for listing forms
/// </summary>
public interface IListFormsQueryService
{
    Task<IEnumerable<FormDTO>> ListFormsAsync();
}

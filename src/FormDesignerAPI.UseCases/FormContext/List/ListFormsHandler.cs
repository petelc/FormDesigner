using FormDesignerAPI.Core.FormContext.Interfaces;
using FormDesignerAPI.Core.FormContext.Specifications;

namespace FormDesignerAPI.UseCases.FormContext.List;

/// <summary>
/// Handler for listing forms
/// </summary>
public class ListFormsHandler : IRequestHandler<ListFormsQuery, Result<IEnumerable<FormContextDTO>>>
{
    private readonly IFormRepository _repository;

    public ListFormsHandler(IFormRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<FormContextDTO>>> Handle(
        ListFormsQuery request,
        CancellationToken cancellationToken)
    {
        // Get forms based on filters
        var forms = request.ActiveOnly == true
            ? await _repository.ListAsync(new GetActiveFormsSpec(), cancellationToken)
            : await _repository.ListAsync(cancellationToken);

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchSpec = new SearchFormsByNameSpec(request.SearchTerm);
            forms = await _repository.ListAsync(searchSpec, cancellationToken);
        }

        var dtos = forms.Select(form => new FormContextDTO
        {
            Id = form.Id,
            Name = form.Name,
            Definition = new FormDefinitionDTO
            {
                Schema = form.Definition.Schema,
                Fields = form.Definition.Fields.Select(f => new FormFieldDTO
                {
                    Name = f.Name,
                    Type = f.Type,
                    Required = f.Required,
                    Label = f.Label,
                    Placeholder = f.Placeholder,
                    DefaultValue = f.DefaultValue,
                    MinLength = f.MinLength,
                    MaxLength = f.MaxLength,
                    Pattern = f.Pattern,
                    Options = f.Options
                }).ToList()
            },
            Origin = new OriginMetadataDTO
            {
                Type = form.Origin.Type.ToString(),
                ReferenceId = form.Origin.ReferenceId,
                CreatedAt = form.Origin.CreatedAt,
                CreatedBy = form.Origin.CreatedBy
            },
            IsActive = form.IsActive,
            CurrentVersion = form.CurrentVersion,
            FieldCount = form.FieldCount
        });

        return Result.Success(dtos);
    }
}

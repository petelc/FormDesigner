using FormDesignerAPI.Core.FormContext.Interfaces;

namespace FormDesignerAPI.UseCases.FormContext.Get;

/// <summary>
/// Handler for getting a form by ID
/// </summary>
public class GetFormHandler : IRequestHandler<GetFormQuery, Result<FormContextDTO>>
{
    private readonly IFormRepository _repository;

    public GetFormHandler(IFormRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<FormContextDTO>> Handle(
        GetFormQuery request,
        CancellationToken cancellationToken)
    {
        var form = await _repository.GetByIdAsync(request.FormId, cancellationToken);

        if (form == null)
        {
            return Result.NotFound($"Form with ID {request.FormId} not found");
        }

        var dto = new FormContextDTO
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
        };

        return Result.Success(dto);
    }
}

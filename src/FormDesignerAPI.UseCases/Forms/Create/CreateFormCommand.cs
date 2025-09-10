using FastEndpoints;
using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.UseCases.Forms.Create;

/// <summary>
/// Command to create a new form.
/// </summary>
/// <param name="FormNumber"></param>
/// <param name="FormTitle"></param>
/// <param name="Division"></param>
/// <param name="Owner"></param>
/// <param name="Version"></param>
/// <param name="ConfigurationPath"></param>
public record CreateFormCommand(
    string FormNumber,
    string? FormTitle = null,
    string? Division = null,
    Owner? Owner = null,
    string? Version = null,
    string? ConfigurationPath = null
) : Ardalis.SharedKernel.ICommand<Result<int>>;

public record CreateFormCommand2(string FormNumber, string? FormTitle = null, string? Division = null, Owner? Owner = null, string? Version = null, string? ConfigurationPath = null) : FastEndpoints.ICommand<Result<int>>;

public class CreateFormCommandHandler2 : CommandHandler<CreateFormCommand2, Result<int>>
{
    private readonly IRepository<Form> _repository;
    public CreateFormCommandHandler2(IRepository<Form> repository)
    {
        _repository = repository;
    }

    public override async Task<Result<int>> ExecuteAsync(CreateFormCommand2 request, CancellationToken cancellationToken)
    {
        var newForm = new Form(
            request.FormNumber,
            request.FormTitle ?? string.Empty,
            request.Division ?? string.Empty,
            request.Owner ?? new Owner("Default Owner", "default@example"),
            request.Version ?? string.Empty,
            request.ConfigurationPath ?? string.Empty);
        var createdItem = await _repository.AddAsync(newForm, cancellationToken);

        Console.WriteLine($"<<<<<<<Created form with ID: {createdItem.Id}");
        return createdItem.Id;
    }
}

// ! When creating a new form we only need to provide the FormNumber and FormTitle. 

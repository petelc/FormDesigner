using FastEndpoints;
using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.UseCases.Forms.Create;

/// <summary>
/// Command to create a new form.
/// </summary>
/// <param name="FormNumber"></param>
/// <param name="FormTitle"></param>
public record CreateFormCommand(
    string FormNumber,
    string? FormTitle = null
) : Ardalis.SharedKernel.ICommand<Result<int>>;

public record CreateFormCommand2(string FormNumber, string? FormTitle = null) : FastEndpoints.ICommand<Result<int>>;

public class CreateFormCommandHandler2 : CommandHandler<CreateFormCommand2, Result<int>>
{
    private readonly IRepository<Form> _repository;
    public CreateFormCommandHandler2(IRepository<Form> repository)
    {
        _repository = repository;
    }

    public override async Task<Result<int>> ExecuteAsync(CreateFormCommand2 request, CancellationToken cancellationToken)
    {
        var newForm = new Form(request.FormNumber);
        var createdItem = await _repository.AddAsync(newForm, cancellationToken);

        Console.WriteLine($"<<<<<<<Created form with ID: {createdItem.Id}");
        return createdItem.Id;
    }
}

// ! When creating a new form we only need to provide the FormNumber and FormTitle. 

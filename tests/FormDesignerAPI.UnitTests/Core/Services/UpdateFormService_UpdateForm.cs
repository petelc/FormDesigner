using Ardalis.Result;
using FormDesignerAPI.Core.FormAggregate;
using FormDesignerAPI.Core.Services;

namespace FormDesignerAPI.UnitTests.Core.Services;

/// <summary>
/// Tests for the FormUpdateService.UpdateFormAsync method.
/// </summary>
public class UpdateFormService_UpdateForm
{
    private readonly IRepository<Form> _repository = Substitute.For<IRepository<Form>>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly ILogger<FormUpdateService> _logger = Substitute.For<ILogger<FormUpdateService>>();

    private readonly FormUpdateService _service;

    public UpdateFormService_UpdateForm()
    {
        _service = new FormUpdateService(_repository, _mediator, _logger);
    }

    [Fact]
    public async Task UpdateForm_ValidRequest_UpdatesForm()
    {
        // Arrange
        var formId = Guid.NewGuid();
        var revisedDate = DateTime.UtcNow;

        // Create a revision for the DTO
        var formDefinition = new FormDefinition("path/to/definition.json");
        var version = FormDesignerAPI.Core.FormAggregate.Revision.Create(1, 1, 1, formDefinition);

        var formUpdateDto = new FormUpdateDto
        (
            formId,
            "New Form Number",
            "New Form Title",
            "New Division",
            "New Owner",
            "newowner@example.com",
            version,
            revisedDate
        );

        // Create an existing form using FormBuilder
        var existingForm = Form.CreateBuilder("Old Form Number")
            .WithTitle("Old Form Title")
            .WithDivision("Old Division")
            .WithOwner("Old Owner", "oldowner@example.com")
            .WithCreatedDate(DateTime.UtcNow.AddMonths(-1))
            .WithRevisedDate(DateTime.UtcNow)
            .Build();

        // Configure the repository mock
        _repository.GetByIdAsync(formId, Arg.Any<CancellationToken>())
            .Returns(existingForm);

        _repository.UpdateAsync(Arg.Any<Form>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateFormAsync(formId, formUpdateDto, CancellationToken.None);

        // Assert
        result.Status.ShouldBe(ResultStatus.Ok);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateForm_FormNotFound_ReturnsNotFound()
    {
        // Arrange
        var formId = Guid.NewGuid();
        var formDefinition = new FormDefinition("path/to/definition.json");
        var version = FormDesignerAPI.Core.FormAggregate.Revision.Create(1, 1, 1, formDefinition);

        var formUpdateDto = new FormUpdateDto
        (
            formId,
            "New Form Number",
            "New Form Title",
            "New Division",
            "New Owner",
            "newowner@example.com",
            version,
            DateTime.UtcNow
        );

        // Configure repository to return null (form not found)
        _repository.GetByIdAsync(formId, Arg.Any<CancellationToken>())
            .Returns((Form?)null);

        // Act
        var result = await _service.UpdateFormAsync(formId, formUpdateDto, CancellationToken.None);

        // Assert
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task UpdateForm_ValidRequest_PublishesDomainEvent()
    {
        // Arrange
        var formId = Guid.NewGuid();
        var formDefinition = new FormDefinition("path/to/definition.json");
        var version = FormDesignerAPI.Core.FormAggregate.Revision.Create(1, 1, 1, formDefinition);

        var formUpdateDto = new FormUpdateDto
        (
            formId,
            "New Form Number",
            "New Form Title",
            "New Division",
            "New Owner",
            "newowner@example.com",
            version,
            DateTime.UtcNow
        );

        var existingForm = Form.CreateBuilder("Old Form Number")
            .WithTitle("Old Form Title")
            .Build();

        _repository.GetByIdAsync(formId, Arg.Any<CancellationToken>())
            .Returns(existingForm);

        _repository.UpdateAsync(Arg.Any<Form>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateFormAsync(formId, formUpdateDto, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateForm_ValidRequest_CallsRepositoryUpdateAsync()
    {
        // Arrange
        var formId = Guid.NewGuid();
        var formDefinition = new FormDefinition("path/to/definition.json");
        var version = FormDesignerAPI.Core.FormAggregate.Revision.Create(1, 1, 1, formDefinition);

        var formUpdateDto = new FormUpdateDto
        (
            formId,
            "New Form Number",
            "New Form Title",
            "New Division",
            "New Owner",
            "newowner@example.com",
            version,
            DateTime.UtcNow
        );

        var existingForm = Form.CreateBuilder("Old Form Number")
            .WithTitle("Old Form Title")
            .Build();

        _repository.GetByIdAsync(formId, Arg.Any<CancellationToken>())
            .Returns(existingForm);

        _repository.UpdateAsync(Arg.Any<Form>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateFormAsync(formId, formUpdateDto, CancellationToken.None);

        // Assert
        await _repository.Received(1).UpdateAsync(Arg.Any<Form>(), Arg.Any<CancellationToken>());
    }
}


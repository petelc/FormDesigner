using Ardalis.Result;
using FormDesignerAPI.Core.FormAggregate;
using FormDesignerAPI.Core.Services;

namespace FormDesignerAPI.UnitTests.Core.Services;

/// <summary>
/// Tests for the FormDeletedService.DeleteFormAsync method.
/// </summary>
public class DeleteFormService_DeleteForm
{
    private readonly IRepository<Form> _repository = Substitute.For<IRepository<Form>>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly ILogger<FormDeletedService> _logger = Substitute.For<ILogger<FormDeletedService>>();

    private readonly FormDeletedService _service;

    public DeleteFormService_DeleteForm()
    {
        _service = new FormDeletedService(_repository, _mediator, _logger);
    }

    [Fact]
    public async Task ReturnsNotFoundGivenCantFindForm()
    {
        // Arrange
        var formId = Guid.NewGuid();
        _repository.GetByIdAsync(formId).Returns((Form?)null);

        // Act
        var result = await _service.DeleteFormAsync(formId);

        // Assert
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task ReturnsSuccessWhenFormIsDeleted()
    {
        // Arrange
        var formId = Guid.NewGuid();
        var form = Form.CreateBuilder("FD-001").Build();

        _repository.GetByIdAsync(formId).Returns(form);
        _repository.DeleteAsync(Arg.Any<Form>()).Returns(Task.CompletedTask);
        _repository.SaveChangesAsync().Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteFormAsync(formId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).DeleteAsync(Arg.Is<Form>(f => f.FormId == form.FormId));
    }

    [Fact]
    public async Task PublishesDomainEventWhenFormIsDeleted()
    {
        // Arrange
        var formId = Guid.NewGuid();
        var form = Form.CreateBuilder("FD-001").Build();

        _repository.GetByIdAsync(formId).Returns(form);
        _repository.DeleteAsync(Arg.Any<Form>()).Returns(Task.CompletedTask);
        _repository.SaveChangesAsync().Returns(Task.CompletedTask);

        // Act
        await _service.DeleteFormAsync(formId);

        // Assert
        await _mediator.Received(1).Publish(Arg.Any<object>());
    }

    [Fact]
    public async Task CallsSaveChangesAsyncAfterDelete()
    {
        // Arrange
        var formId = Guid.NewGuid();
        var form = Form.CreateBuilder("FD-001").Build();

        _repository.GetByIdAsync(formId).Returns(form);
        _repository.DeleteAsync(Arg.Any<Form>()).Returns(Task.CompletedTask);
        _repository.SaveChangesAsync().Returns(Task.CompletedTask);

        // Act
        await _service.DeleteFormAsync(formId);

        // Assert
        await _repository.Received(1).SaveChangesAsync();
    }
}


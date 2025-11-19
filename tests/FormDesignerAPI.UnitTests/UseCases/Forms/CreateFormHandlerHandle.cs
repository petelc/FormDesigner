using Ardalis.Result;
using FormDesignerAPI.Core.FormAggregate;
using FormDesignerAPI.UseCases.Forms.Create;

namespace FormDesignerAPI.UnitTests.UseCases.Forms;

/// <summary>
/// Tests for the CreateFormHandler using the FormBuilder pattern.
/// </summary>
public class CreateFormHandlerHandle
{
    private readonly string _testFormNumber = "XXX-1234";
    private readonly string _testFormTitle = "Test Form";
    private readonly string _testDivision = "Test Division";
    private readonly Owner _testOwner = new Owner("Test Owner", "testowner@example.com");
    private readonly DateTime _testCreatedDate = DateTime.UtcNow;
    private readonly DateTime _testRevisedDate = DateTime.UtcNow;

    private readonly IRepository<Form> _repository = Substitute.For<IRepository<Form>>();

    private CreateFormHandler _handler;

    public CreateFormHandlerHandle()
    {
        _handler = new CreateFormHandler(_repository);
    }

    /// <summary>
    /// Creates a minimal form with only FormNumber using FormBuilder.
    /// </summary>
    private Form CreateMinimalForm()
    {
        return Form.CreateBuilder(_testFormNumber).Build();
    }

    /// <summary>
    /// Creates a form with FormNumber and Title using FormBuilder.
    /// </summary>
    private Form CreateFormWithTitle()
    {
        return Form.CreateBuilder(_testFormNumber)
            .WithTitle(_testFormTitle)
            .Build();
    }

    /// <summary>
    /// Creates a fully configured form with all optional properties using FormBuilder.
    /// </summary>
    private Form CreateFullForm()
    {
        return Form.CreateBuilder(_testFormNumber)
            .WithTitle(_testFormTitle)
            .WithDivision(_testDivision)
            .WithOwner(_testOwner)
            .WithCreatedDate(_testCreatedDate)
            .WithRevisedDate(_testRevisedDate)
            .Build();
    }

    [Fact]
    public async Task ReturnsSuccessGivenValidFormNumber()
    {
        // Arrange
        var form = CreateMinimalForm();
        _repository.AddAsync(Arg.Any<Form>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(form));

        // Act
        var command = new CreateFormCommand(_testFormNumber);
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task ReturnsSuccessGivenValidFormNumberAndTitle()
    {
        // Arrange
        var form = CreateFormWithTitle();
        _repository.AddAsync(Arg.Any<Form>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(form));

        // Act
        var command = new CreateFormCommand(_testFormNumber, _testFormTitle);
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task ReturnsSuccessGivenAllValidParameters()
    {
        // Arrange
        var form = CreateFullForm();
        _repository.AddAsync(Arg.Any<Form>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(form));

        // Act
        var command = new CreateFormCommand(
            _testFormNumber,
            _testFormTitle,
            _testDivision,
            _testOwner,
            null,
            _testCreatedDate,
            _testRevisedDate
        );
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task CallsRepositoryAddAsyncWithFormAggregate()
    {
        // Arrange
        var form = CreateMinimalForm();
        _repository.AddAsync(Arg.Any<Form>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(form));

        // Act
        var command = new CreateFormCommand(_testFormNumber);
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Is<Form>(f => f.FormNumber == _testFormNumber), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReturnsFormIdFromRepository()
    {
        // Arrange
        var expectedForm = CreateMinimalForm();
        _repository.AddAsync(Arg.Any<Form>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedForm));

        // Act
        var command = new CreateFormCommand(_testFormNumber);
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedForm.FormId);
    }
}


using FormDesignerAPI.Core.FormAggregate;
using FormDesignerAPI.UseCases.Forms.Create;

namespace FormDesignerAPI.UnitTests.UseCases.Forms;

public class CreateFormHandlerHandle
{
    private readonly string _testFormNumber = "XXX-1234";
    private readonly string _testFormTitle = "Test Form";
    private readonly string _testDivision = "Test Division";
    private readonly Owner _testOwner = new Owner("Test Owner", "testownder@example.com");
    private readonly string _testVersion = "1.0";
    private readonly DateTime _testCreatedDate = DateTime.UtcNow;
    private readonly DateTime _testRevisedDate = DateTime.UtcNow;
    private readonly string _testConfigurationPath = "/path/to/config";

    private readonly IRepository<Form> _repository = Substitute.For<IRepository<Form>>();

    private CreateFormHandler _handler;

    public CreateFormHandlerHandle()
    {
        _handler = new CreateFormHandler(_repository);
    }


    /// ///TODO: Implement form construtor that takes in both parameters

    private Form CreateForm()
    {
        return new Form(_testFormNumber);
    }

    private Form CreateFormWithTitle()
    {
        return new Form(_testFormNumber, _testFormTitle);
    }

    private Form CreateFullForm()
    {
        return new Form(_testFormNumber, _testFormTitle, _testDivision, _testOwner, _testVersion, _testCreatedDate, _testRevisedDate, _testConfigurationPath);
    }

    [Fact]
    public async Task ReturnsSuccessGivenValidFormNumber()
    {
        // Arrange
        _repository.AddAsync(Arg.Any<Form>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CreateForm()));

        // Act
        var result = await _handler.Handle(new CreateFormCommand(_testFormNumber), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ReturnsSuccessGivenValidFormNumberAndTitle()
    {
        // Arrange
        _repository.AddAsync(Arg.Any<Form>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CreateForm()));

        // Act
        var result = await _handler.Handle(new CreateFormCommand(_testFormNumber, _testFormTitle), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ReturnsSuccessGivenAllValidParameters()
    {
        // Arrange
        _repository.AddAsync(Arg.Any<Form>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CreateFullForm()));

        // Act
        var result = await _handler.Handle(new CreateFormCommand(_testFormNumber, _testFormTitle, _testDivision, _testOwner, _testVersion, _testCreatedDate, _testRevisedDate, _testConfigurationPath), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }
}

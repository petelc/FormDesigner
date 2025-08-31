using FormDesignerAPI.Core.FormAggregate;
using FormDesignerAPI.UseCases.Forms.Create;

namespace FormDesignerAPI.UnitTests.UseCases.Forms;

public class CreateFormHandlerHandle
{
    private readonly string _testFormNumber = "XXX-1234";
    private readonly string _testFormTitle = "Test Form";

    private readonly IRepository<Form> _repository = Substitute.For<IRepository<Form>>();

    private CreateFormHandler _handler;

    public CreateFormHandlerHandle()
    {
        _handler = new CreateFormHandler(_repository);
    }

    // TODO: Implement form construtor that takes in both parameters
    private Form CreateForm()
    {
        return new Form(_testFormNumber);
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
}

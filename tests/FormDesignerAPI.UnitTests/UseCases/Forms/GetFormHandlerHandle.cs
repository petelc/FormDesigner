using FormDesignerAPI.Core.FormAggregate;
using FormDesignerAPI.Core.FormAggregate.Specifications;
using FormDesignerAPI.UseCases.Forms.Get;

namespace FormDesignerAPI.UnitTests.UseCases.Forms;

public class GetFormHandlerHandle
{
    private readonly string _testFormNumber = "XXX-1234";
    private readonly string _testFormTitle = "Test Form";
    private readonly string _testDivision = "Test Division";
    private readonly Owner _testOwner = new Owner("Test Owner", "test.owner@example.com");
    private readonly string _testVersion = "1.0";
    private readonly string _testConfigurationPath = "/path/to/config";

    private readonly IReadRepository<Form> _repository = Substitute.For<IReadRepository<Form>>();
    private GetFormHandler _handler;
    public GetFormHandlerHandle()
    {
        _handler = new GetFormHandler(_repository);
    }
    private Form CreateForm()
    {
        return new Form(_testFormNumber, _testFormTitle, _testDivision, _testOwner, _testVersion, _testConfigurationPath);
    }

    [Fact]
    public async Task ReturnsSuccessGivenValidFormId()
    {
        // Arrange
        var form = CreateForm();
        _repository.FirstOrDefaultAsync(Arg.Any<FormByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Form?>(form));

        // Act
        var result = await _handler.Handle(new GetFormQuery(form.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Id.ShouldBe(form.Id);
        result.Value.FormNumber.ShouldBe(_testFormNumber);
        result.Value.FormTitle.ShouldBe(_testFormTitle);
        result.Value.Division.ShouldBe(_testDivision);
        result.Value.Owner.ShouldBe(_testOwner.Name);
        result.Value.Version.ShouldBe(_testVersion);
        result.Value.ConfigurationPath.ShouldBe(_testConfigurationPath);
    }

}

using FormDesignerAPI.Core.FormAggregate;
using FormDesignerAPI.UseCases.Forms.Update;

namespace FormDesignerAPI.UnitTests.UseCases.Forms;

public class UpdateFormHandlerHandle
{
    private readonly string _testFormNumber = "XXX-1234";
    private readonly string _testFormTitle = "Test Form";
    private readonly string _testDivision = "Test Division";
    private readonly Owner _testOwner = new Owner("Test Owner", "test.owner@example.com");
    private readonly string _testVersion = "1.0";
    private readonly string _testConfigurationPath = "/path/to/config";

    // ...update form...
    private readonly string _updatedFormNumber = "YYY-5678";
    private readonly string _updatedFormTitle = "Updated Test Form";
    private readonly string _updatedDivision = "Updated Division";
    private readonly Owner _updatedOwner = new Owner("Updated Owner", "updated.owner@example.com");
    private readonly string _updatedOwnerName = "Updated Owner";
    private readonly string _updatedOwnerEmail = "updated.owner@example.com";
    private readonly string _updatedVersion = "2.0";
    private readonly DateTime _updatedRevisedDate = DateTime.UtcNow;
    private readonly string _updatedConfigurationPath = "/new/path/to/config";

    private readonly IRepository<Form> _repository = Substitute.For<IRepository<Form>>();
    private UpdateFormHandler _handler;

    public UpdateFormHandlerHandle()
    {
        _handler = new UpdateFormHandler(_repository);
    }

    private Form CreateForm()
    {
        return new Form(_testFormNumber, _testFormTitle, _testDivision, _testOwner, _testVersion, _testConfigurationPath);
    }



    [Fact]
    public async Task ReturnsSuccessGivenUpdatedForm()
    {
        // Arrange
        var form = CreateForm();
        _repository.GetByIdAsync(form.Id).Returns(form);

        var updatedForm = new Form(_updatedFormNumber, _updatedFormTitle, _updatedDivision, _updatedOwner, _updatedVersion, _updatedConfigurationPath);

        _repository.UpdateAsync(Arg.Any<Form>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));

        // Act
        var result = await _handler.Handle(new UpdateFormCommand(form.Id, _updatedFormNumber, _updatedFormTitle, _updatedDivision, _updatedOwnerName, _updatedOwnerEmail, _updatedVersion, _updatedRevisedDate, _updatedConfigurationPath), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(updatedForm.FormNumber, form.FormNumber);
        Assert.Equal(updatedForm.FormTitle, form.FormTitle);
        Assert.Equal(updatedForm.Division, form.Division);
        Assert.Equal(updatedForm.Owner, form.Owner);
        Assert.Equal(updatedForm.Version, form.Version);
        Assert.Equal(updatedForm.ConfigurationPath, form.ConfigurationPath);
    }

}

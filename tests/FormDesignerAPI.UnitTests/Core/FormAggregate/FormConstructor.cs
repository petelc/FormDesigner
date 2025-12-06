using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.UnitTests.Core.FormAggregate;

public class FormConstructor
{
    private readonly string _testFormNumber = "XXX1234";
    private readonly string _testFormTitle = "Test Form";
    private readonly string _testDivision = "Test Division";
    private readonly Owner _testOwner = new Owner("Test Owner", "testownder@example.com");
    private readonly string _testVersion = "1.0";
    private readonly DateTime _testCreatedDate = DateTime.UtcNow;
    private readonly DateTime _testRevisedDate = DateTime.UtcNow;
    private readonly string _testConfigurationPath = "/path/to/config";

    private Form? _testForm;

    private Form CreateForm()
    {
        return new Form(_testFormNumber, _testFormTitle);
    }

    private Form CreateFullForm()
    {
        return new Form(_testFormNumber, _testFormTitle, _testDivision, _testOwner, _testVersion, _testCreatedDate, _testRevisedDate, _testConfigurationPath);
    }

    [Fact]
    public void InitializesFormNumber()
    {
        _testForm = CreateForm();

        _testForm.FormNumber.ShouldBe(_testFormNumber);
    }

    [Fact]
    public void InitializesFormNumberandFormTitle()
    {
        _testForm = CreateForm();

        _testForm.FormNumber.ShouldBe(_testFormNumber);
        _testForm.FormTitle.ShouldBe(_testFormTitle);
    }

    [Fact]
    public void InitializesAllProperties()
    {
        _testForm = CreateFullForm();

        _testForm.FormNumber.ShouldBe(_testFormNumber);
        _testForm.FormTitle.ShouldBe(_testFormTitle);
        _testForm.Division.ShouldBe(_testDivision);
        _testForm.Owner.ShouldNotBeNull();
        _testForm.Owner!.Name.ShouldBe(_testOwner.Name);
        _testForm.Owner.Email.ShouldBe(_testOwner.Email);
        _testForm.Version.ShouldBe(_testVersion);
        _testForm.ConfigurationPath.ShouldBe(_testConfigurationPath);
    }
}

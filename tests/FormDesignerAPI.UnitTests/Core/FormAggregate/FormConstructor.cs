using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.UnitTests.Core.FormAggregate;

public class FormConstructor
{
    private readonly string _testFormNumber = "XXX1234";
    private readonly string _testFormTitle = "Test Form";

    private Form? _testForm;

    private Form CreateForm()
    {
        return new Form(_testFormNumber, _testFormTitle);
    }

    [Fact]
    public void InitializesFormNumber()
    {
        _testForm = CreateForm();

        _testForm.FormNumber.ShouldBe(_testFormNumber);
    }
}

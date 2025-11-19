using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.UnitTests.Core.FormAggregate;

/// <summary>
/// Tests for Form construction using the FormBuilder pattern.
/// </summary>
public class FormConstructor
{
    private readonly string _testFormNumber = "XXX1234";
    private readonly string _testFormTitle = "Test Form";
    private readonly string _testDivision = "Test Division";
    private readonly Owner _testOwner = new Owner("Test Owner", "testowner@example.com");
    private readonly DateTime _testCreatedDate = DateTime.UtcNow;
    private readonly DateTime _testRevisedDate = DateTime.UtcNow;

    private Form? _testForm;

    /// <summary>
    /// Creates a minimal form with only FormNumber (required).
    /// </summary>
    private Form CreateMinimalForm()
    {
        return Form.CreateBuilder(_testFormNumber).Build();
    }

    /// <summary>
    /// Creates a fully configured form with all optional properties.
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
    public void InitializesFormNumber()
    {
        _testForm = CreateMinimalForm();

        _testForm.FormNumber.ShouldBe(_testFormNumber);
    }

    [Fact]
    public void InitializesFormNumberWithTitle()
    {
        _testForm = Form.CreateBuilder(_testFormNumber)
            .WithTitle(_testFormTitle)
            .Build();

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
        _testForm.CreatedDate.ShouldBe(_testCreatedDate);
        _testForm.RevisedDate.ShouldBe(_testRevisedDate);
    }

    [Fact]
    public void FormHasInitialDraftStatus()
    {
        _testForm = CreateMinimalForm();

        _testForm.Status.ShouldBe(FormStatus.Draft);
    }

    [Fact]
    public void FormHasUniqueFormId()
    {
        var form1 = CreateMinimalForm();
        var form2 = CreateMinimalForm();

        form1.FormId.ShouldNotBe(form2.FormId);
    }

    [Fact]
    public void CanUpdateFormTitle()
    {
        _testForm = CreateMinimalForm();
        var newTitle = "Updated Title";

        _testForm.UpdateFormTitle(newTitle);

        _testForm.FormTitle.ShouldBe(newTitle);
    }

    [Fact]
    public void CanUpdateDivision()
    {
        _testForm = CreateMinimalForm();
        var newDivision = "New Division";

        _testForm.UpdateDivision(newDivision);

        _testForm.Division.ShouldBe(newDivision);
    }

    [Fact]
    public void CanSetOwner()
    {
        _testForm = CreateMinimalForm();
        var newOwner = new Owner("New Owner", "newowner@example.com");

        _testForm.SetOwner(newOwner.Name, newOwner.Email);

        _testForm.Owner.ShouldNotBeNull();
        _testForm.Owner!.Name.ShouldBe(newOwner.Name);
        _testForm.Owner.Email.ShouldBe(newOwner.Email);
    }

    [Fact]
    public void BuilderReturnsThisForMethodChaining()
    {
        var builder = Form.CreateBuilder(_testFormNumber);
        var result = builder
            .WithTitle(_testFormTitle)
            .WithDivision(_testDivision)
            .WithOwner(_testOwner);

        result.ShouldNotBeNull();

        var form = result.Build();
        form.FormTitle.ShouldBe(_testFormTitle);
        form.Division.ShouldBe(_testDivision);
    }
}


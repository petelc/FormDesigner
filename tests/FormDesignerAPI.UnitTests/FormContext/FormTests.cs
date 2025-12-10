using FormDesignerAPI.Core.FormContext.Aggregates;
using FormDesignerAPI.Core.FormContext.ValueObjects;
using FormDesignerAPI.Core.FormContext.Events;
using Xunit;
using FluentAssertions;

namespace FormDesignerAPI.UnitTests.FormContext;

public class FormTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateForm()
    {
        // Arrange
        var name = "Patient Intake Form";
        var fields = new List<FormField>
        {
        new FormField { Name = "firstName", Type = "text", Required = true, Label = "First Name" },
        new FormField { Name = "lastName", Type = "text", Required = true, Label = "Last Name" }
        };
        var definition = FormDefinition.FromFields(fields);
        var origin = OriginMetadata.Manual("admin@test.com");

        // Act
        var form = Form.Create(name, definition, origin, "admin@test.com");

        // Assert
        form.Should().NotBeNull();
        form.Id.Should().NotBeEmpty();
        form.Name.Should().Be(name);
        form.Definition.Should().Be(definition);
        form.Origin.Should().Be(origin);
        form.IsActive.Should().BeTrue();
        form.Revisions.Should().HaveCount(1);
        form.CurrentVersion.Should().Be(1);
        form.FieldCount.Should().Be(2);
    }

    [Fact]
    public void Create_WithValidData_ShouldRaiseFormCreatedEvent()
    {
        // Arrange
        var name = "Test Form";
        var fields = new List<FormField>
        {
        new FormField { Name = "field1", Type = "text", Required = true }
        };
        var definition = FormDefinition.FromFields(fields);
        var origin = OriginMetadata.Manual("admin@test.com");

        // Act
        var form = Form.Create(name, definition, origin, "admin@test.com");

        // Assert
        form.DomainEvents.Should().HaveCount(1);
        var domainEvent = form.DomainEvents.First();
        domainEvent.Should().BeOfType<FormCreatedEvent>();

        var formCreatedEvent = (FormCreatedEvent)domainEvent;
        formCreatedEvent.FormId.Should().Be(form.Id);
        formCreatedEvent.Name.Should().Be(name);
        formCreatedEvent.CreatedBy.Should().Be("admin@test.com");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        var fields = new List<FormField>
        {
        new FormField { Name = "field1", Type = "text", Required = true }
        };
        var definition = FormDefinition.FromFields(fields);
        var origin = OriginMetadata.Manual("admin@test.com");
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        Form.Create("", definition, origin, "admin@test.com"));
    }

    [Fact]
    public void CreateRevision_WithValidData_ShouldCreateNewRevision()
    {
        // Arrange
        var form = CreateTestForm();
        var newFields = new List<FormField>
        {
            new FormField { Name = "firstName", Type = "text", Required = true },
            new FormField { Name = "email", Type = "email", Required = true, Label = "Email" }
        };
        var newDefinition = FormDefinition.FromFields(newFields);
        // Act
        form.CreateRevision(newDefinition, "Added email field", "admin@test.com");

        // Assert
        form.Revisions.Should().HaveCount(2);
        form.CurrentVersion.Should().Be(2);
        form.Definition.Should().Be(newDefinition);
        form.DomainEvents.Should().HaveCount(2); // FormCreated + FormRevisionCreated
    }

    [Fact]
    public void Rename_WithNewName_ShouldUpdateNameAndRaiseEvent()
    {
        // Arrange
        var form = CreateTestForm();
        var oldName = form.Name;
        var newName = "Updated Form Name";
        // Act
        form.Rename(newName, "admin@test.com");

        // Assert
        form.Name.Should().Be(newName);
        form.DomainEvents.Should().Contain(e => e is FormRenamedEvent);

        var renameEvent = form.DomainEvents.OfType<FormRenamedEvent>().First();
        renameEvent.OldName.Should().Be(oldName);
        renameEvent.NewName.Should().Be(newName);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var form = CreateTestForm();
        // Act
        form.Deactivate();

        // Assert
        form.IsActive.Should().BeFalse();
    }

    [Fact]
    public void CreateRevision_OnInactiveForm_ShouldThrowException()
    {
        // Arrange
        var form = CreateTestForm();
        form.Deactivate();
        var newFields = new List<FormField>
        {
            new FormField { Name = "field1", Type = "text", Required = true }
        };
        var newDefinition = FormDefinition.FromFields(newFields);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        form.CreateRevision(newDefinition, "test", "admin@test.com"));
    }

    // Helper method
    private Form CreateTestForm()
    {
        var fields = new List<FormField>
        {
            new FormField { Name = "firstName", Type = "text", Required = true, Label = "First Name" }
        };
        var definition = FormDefinition.FromFields(fields);
        var origin = OriginMetadata.Manual("admin@test.com");
        return Form.Create("Test Form", definition, origin, "admin@test.com");
    }
}
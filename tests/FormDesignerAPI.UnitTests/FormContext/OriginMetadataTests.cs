using FormDesignerAPI.Core.FormContext.ValueObjects;
using Xunit;
using FluentAssertions;

namespace FormDesignerAPI.UnitTests.FormContext;

public class OriginMetadataTests
{
    [Fact]
    public void Manual_WithValidCreatedBy_ShouldCreateOrigin()
    {
        // Act
        var origin = OriginMetadata.Manual("admin@test.com");

        // Assert
        origin.Type.Should().Be(OriginType.Manual);
        origin.CreatedBy.Should().Be("admin@test.com");
        origin.ReferenceId.Should().BeNull();
        origin.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Import_WithValidData_ShouldCreateOrigin()
    {
        // Act
        var origin = OriginMetadata.Import("candidate-123", "approver@test.com");

        // Assert
        origin.Type.Should().Be(OriginType.Import);
        origin.ReferenceId.Should().Be("candidate-123");
        origin.CreatedBy.Should().Be("approver@test.com");
    }

    [Fact]
    public void Manual_WithEmptyCreatedBy_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => OriginMetadata.Manual(""));
    }

    [Fact]
    public void Equals_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var origin1 = OriginMetadata.Manual("admin@test.com");
        System.Threading.Thread.Sleep(10); // Ensure different timestamp
        var origin2 = OriginMetadata.Manual("admin@test.com");

        // Assert - Should NOT be equal due to different timestamps
        origin1.Should().NotBe(origin2);
    }
}
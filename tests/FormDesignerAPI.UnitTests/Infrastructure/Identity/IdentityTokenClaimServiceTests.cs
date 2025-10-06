// using System.Threading.Tasks;
// using FormDesignerAPI.Infrastructure.Identity;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.Extensions.Options;
// using Moq;
// using Xunit;

// public class IdentityTokenClaimServiceTests
// {
//     [Fact]
//     public void Constructor_SetsJwtSecretKey_FromOptions()
//     {
//         // Arrange
//         var userManagerMock = new Mock<UserManager<ApplicationUser>>(
//             Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
//         var options = Options.Create(new AuthSettings { JWT_SECRET_KEY = "TestSecretKey" });

//         // Act
//         var service = new IdentityTokenClaimService(userManagerMock.Object, options);

//         // Assert
//         // Use reflection to check the private field (or test via public API in real scenarios)
//         Assert.NotNull(service);

//         // Use reflection to verify the JWT secret key was set correctly
//         var jwtSecretKeyField = typeof(IdentityTokenClaimService)
//             .GetField("_jwtSecretKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//         Assert.NotNull(jwtSecretKeyField);
//         var jwtSecretKeyValue = jwtSecretKeyField.GetValue(service) as string;
//         Assert.Equal("TestSecretKey", jwtSecretKeyValue);
//     }
// }

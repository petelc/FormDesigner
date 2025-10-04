using Microsoft.AspNetCore.Identity;

namespace FormDesignerAPI.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Division { get; set; }
    public string? JobTitle { get; set; }
    public string? Supervisor { get; set; }
    //public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
}

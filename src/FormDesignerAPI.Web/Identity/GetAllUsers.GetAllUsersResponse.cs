using FormDesignerAPI.Core.Entities;

namespace FormDesignerAPI.Web.Identity;

public class GetAllUsersResponse
{
    public List<UserDto> Users { get; set; } = new();
}

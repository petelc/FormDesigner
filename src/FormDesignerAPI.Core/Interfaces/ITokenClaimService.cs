namespace FormDesignerAPI.Core.Interfaces;

public interface ITokenClaimService
{
    Task<string> GetTokenAsync(string userName);
}

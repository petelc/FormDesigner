namespace FormDesignerAPI.Web.Common;

/// <summary>
/// Base class used by API responses
/// </summary>
public class BaseResponse : BaseMessage
{
    public BaseResponse(Guid correlationId) : base()
    {
        base._correlationId = correlationId;
    }

    public BaseResponse() { }
}

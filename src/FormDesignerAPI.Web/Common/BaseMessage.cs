namespace FormDesignerAPI.Web.Common;

/// <summary>
/// Base class used by API requests
/// </summary>
public abstract class BaseMessage
{
    /// <summary>
    /// Unique identifier used by logging.
    /// </summary>
    protected Guid _correlationId = Guid.NewGuid();
    public Guid CorrelationId => _correlationId;

}

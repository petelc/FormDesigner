namespace FormDesignerAPI.Core.FormContext.ValueObjects;

/// <summary>
/// Represents how a form was created
/// </summary>
public enum OriginType
{
    /// <summary>
    /// Manually created by a user
    /// </summary>
    Manual,

    /// <summary>
    /// Created from an imported PDF
    /// </summary>
    Import,

    /// <summary>
    /// Created via API
    /// </summary>
    API,

    /// <summary>
    /// Created from a template
    /// </summary>
    Template
}
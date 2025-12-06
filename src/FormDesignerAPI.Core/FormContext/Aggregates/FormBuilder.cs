namespace FormDesignerAPI.Core.FormContext.Aggregates;

/// <summary>
/// Builder pattern for creating Form instances with fluent syntax
/// </summary>
public class FormBuilder
{
    private readonly Form _form;

    internal FormBuilder(string formNumber)
    {
        _form = new Form(formNumber);
    }

    /// <summary>
    /// Sets the form title
    /// </summary>
    public FormBuilder WithTitle(string title)
    {
        _form.UpdateFormTitle(title);
        return this;
    }

    /// <summary>
    /// Sets the division
    /// </summary>
    public FormBuilder WithDivision(string division)
    {
        _form.UpdateDivision(division);
        return this;
    }

    /// <summary>
    /// Sets the form owner
    /// </summary>
    public FormBuilder WithOwner(string name, string email)
    {
        _form.SetOwner(name, email);
        return this;
    }

    /// <summary>
    /// Sets the form owner using Owner value object
    /// </summary>
    public FormBuilder WithOwner(Owner owner)
    {
        _form.SetOwner(owner.Name, owner.Email);
        return this;
    }

    /// <summary>
    /// Sets the creation date
    /// </summary>
    public FormBuilder WithCreatedDate(DateTime createdDate)
    {
        _form.SetCreatedDate(createdDate);
        return this;
    }

    /// <summary>
    /// Sets the revision date
    /// </summary>
    public FormBuilder WithRevisedDate(DateTime revisedDate)
    {
        _form.SetRevisedDate(revisedDate);
        return this;
    }

    /// <summary>
    /// Adds a revision to the form
    /// </summary>
    public FormBuilder WithRevision(FormRevision revision)
    {
        _form.AddRevision(revision);
        return this;
    }

    /// <summary>
    /// Adds multiple revisions to the form
    /// </summary>
    public FormBuilder WithRevisions(params FormRevision[] revisions)
    {
        foreach (var revision in revisions)
        {
            _form.AddRevision(revision);
        }
        return this;
    }

    /// <summary>
    /// Builds and returns the configured Form instance
    /// </summary>
    public Form Build()
    {
        return _form;
    }
}
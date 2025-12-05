namespace FormDesignerAPI.Core.FormContext.Aggregates;

public class FormStatus : SmartEnum<FormStatus>
{
    public static readonly FormStatus Draft = new(nameof(Draft), 1);
    public static readonly FormStatus Published = new(nameof(Published), 2);
    public static readonly FormStatus Archived = new(nameof(Archived), 3);
    public static readonly FormStatus NotSet = new(nameof(NotSet), 4);

    protected FormStatus(string name, int value) : base(name, value)
    {
    }

}

namespace FormDesignerAPI.Core.FormAggregate;

public class FormDefinition : EntityBase<Guid>
{
    public Guid FormDefinitionId { get; set; }
    public string ConfigurationPath { get; private set; } = string.Empty;

    public FormDefinition(string configurationPath)
    {
        FormDefinitionId = Guid.NewGuid();
        SetConfigurationPath(configurationPath);
    }

    #region  Get and Set Configuration Path
    public void SetConfigurationPath(string configurationPath)
    {
        ConfigurationPath = Guard.Against.NullOrEmpty(configurationPath, nameof(configurationPath));
    }

    public string GetConfigurationPath()
    {
        return ConfigurationPath;
    }
    #endregion

    #region methods 

    public FormDefinition CreateFormDefinition(string configurationPath)
    {
        return new FormDefinition(configurationPath);
    }

    public FormDefinition UpdateConfigurationPath(string configurationPath)
    {
        SetConfigurationPath(configurationPath);
        return this;
    }

    #endregion
}

using FormDesignerAPI.UseCases.Interfaces;
using Microsoft.Extensions.Logging;

namespace FormDesignerAPI.Infrastructure.DocumentIntelligence;

/// <summary>
/// Mock implementation of IFormExtractor for development/testing
/// Simulates Azure Document Intelligence responses without requiring Azure credentials
/// </summary>
public class MockFormExtractorService : IFormExtractor
{
    private readonly ILogger<MockFormExtractorService> _logger;

    public MockFormExtractorService(ILogger<MockFormExtractorService> logger)
    {
        _logger = logger;
    }

    public async Task<string> DetectFormTypeAsync(
        string pdfPath,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock: Detecting form type for {Path}", pdfPath);

        // Simulate some processing time
        await Task.Delay(500, cancellationToken);

        // Analyze filename or return a default type
        var fileName = Path.GetFileNameWithoutExtension(pdfPath).ToLowerInvariant();

        if (fileName.Contains("application"))
            return "application";
        if (fileName.Contains("registration"))
            return "registration";
        if (fileName.Contains("survey"))
            return "survey";
        if (fileName.Contains("contact"))
            return "contact";

        return "generic";
    }

    public async Task<ExtractedFormStructure> ExtractFormStructureAsync(
        string pdfPath,
        string formType,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Mock: Extracting form structure from {Path}, Type: {FormType}",
            pdfPath,
            formType);

        // Simulate processing time
        await Task.Delay(1000, cancellationToken);

        // Return mock data based on form type
        var structure = formType.ToLowerInvariant() switch
        {
            "contact" => CreateContactFormStructure(),
            "application" => CreateApplicationFormStructure(),
            "registration" => CreateRegistrationFormStructure(),
            "survey" => CreateSurveyFormStructure(),
            _ => CreateGenericFormStructure()
        };

        _logger.LogInformation(
            "Mock: Extracted {FieldCount} fields, {TableCount} tables",
            structure.Fields.Count,
            structure.Tables.Count);

        return structure;
    }

    private ExtractedFormStructure CreateContactFormStructure()
    {
        return new ExtractedFormStructure
        {
            Fields = new List<ExtractedField>
            {
                new ExtractedField
                {
                    Name = "FirstName",
                    Label = "First Name",
                    Type = "text",
                    IsRequired = true,
                    MaxLength = 50
                },
                new ExtractedField
                {
                    Name = "LastName",
                    Label = "Last Name",
                    Type = "text",
                    IsRequired = true,
                    MaxLength = 50
                },
                new ExtractedField
                {
                    Name = "Email",
                    Label = "Email Address",
                    Type = "email",
                    IsRequired = true,
                    MaxLength = 100,
                    ValidationPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$"
                },
                new ExtractedField
                {
                    Name = "Phone",
                    Label = "Phone Number",
                    Type = "tel",
                    IsRequired = false,
                    MaxLength = 20,
                    ValidationPattern = @"^\+?[\d\s\-\(\)]+$"
                },
                new ExtractedField
                {
                    Name = "Message",
                    Label = "Message",
                    Type = "textarea",
                    IsRequired = true,
                    MaxLength = 1000
                }
            },
            Tables = new List<ExtractedTable>(),
            Warnings = new List<string>()
        };
    }

    private ExtractedFormStructure CreateApplicationFormStructure()
    {
        return new ExtractedFormStructure
        {
            Fields = new List<ExtractedField>
            {
                new ExtractedField
                {
                    Name = "FullName",
                    Label = "Full Legal Name",
                    Type = "text",
                    IsRequired = true,
                    MaxLength = 100
                },
                new ExtractedField
                {
                    Name = "DateOfBirth",
                    Label = "Date of Birth",
                    Type = "date",
                    IsRequired = true
                },
                new ExtractedField
                {
                    Name = "SSN",
                    Label = "Social Security Number",
                    Type = "text",
                    IsRequired = true,
                    ValidationPattern = @"^\d{3}-\d{2}-\d{4}$",
                    MaxLength = 11
                },
                new ExtractedField
                {
                    Name = "Email",
                    Label = "Email Address",
                    Type = "email",
                    IsRequired = true,
                    ValidationPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$"
                },
                new ExtractedField
                {
                    Name = "Address",
                    Label = "Street Address",
                    Type = "text",
                    IsRequired = true,
                    MaxLength = 200
                },
                new ExtractedField
                {
                    Name = "City",
                    Label = "City",
                    Type = "text",
                    IsRequired = true,
                    MaxLength = 50
                },
                new ExtractedField
                {
                    Name = "State",
                    Label = "State",
                    Type = "select",
                    IsRequired = true,
                    Options = new List<string> { "AL", "AK", "AZ", "CA", "FL", "NY", "TX" }
                },
                new ExtractedField
                {
                    Name = "ZipCode",
                    Label = "ZIP Code",
                    Type = "text",
                    IsRequired = true,
                    ValidationPattern = @"^\d{5}(-\d{4})?$",
                    MaxLength = 10
                },
                new ExtractedField
                {
                    Name = "EmploymentStatus",
                    Label = "Employment Status",
                    Type = "select",
                    IsRequired = true,
                    Options = new List<string> { "Employed", "Self-Employed", "Unemployed", "Student", "Retired" }
                }
            },
            Tables = new List<ExtractedTable>
            {
                new ExtractedTable
                {
                    RowCount = 3,
                    ColumnCount = 4,
                    Cells = new List<ExtractedCell>
                    {
                        new ExtractedCell { Content = "Employer", RowIndex = 0, ColumnIndex = 0, IsHeader = true },
                        new ExtractedCell { Content = "Position", RowIndex = 0, ColumnIndex = 1, IsHeader = true },
                        new ExtractedCell { Content = "From", RowIndex = 0, ColumnIndex = 2, IsHeader = true },
                        new ExtractedCell { Content = "To", RowIndex = 0, ColumnIndex = 3, IsHeader = true }
                    }
                }
            },
            Warnings = new List<string>
            {
                "Detected employment history table - consider using a repeating section"
            }
        };
    }

    private ExtractedFormStructure CreateRegistrationFormStructure()
    {
        return new ExtractedFormStructure
        {
            Fields = new List<ExtractedField>
            {
                new ExtractedField
                {
                    Name = "Username",
                    Label = "Username",
                    Type = "text",
                    IsRequired = true,
                    MaxLength = 20,
                    ValidationPattern = @"^[a-zA-Z0-9_]+$"
                },
                new ExtractedField
                {
                    Name = "Email",
                    Label = "Email",
                    Type = "email",
                    IsRequired = true,
                    ValidationPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$"
                },
                new ExtractedField
                {
                    Name = "Password",
                    Label = "Password",
                    Type = "password",
                    IsRequired = true,
                    MaxLength = 100
                },
                new ExtractedField
                {
                    Name = "ConfirmPassword",
                    Label = "Confirm Password",
                    Type = "password",
                    IsRequired = true
                },
                new ExtractedField
                {
                    Name = "AgreeToTerms",
                    Label = "I agree to the Terms and Conditions",
                    Type = "checkbox",
                    IsRequired = true
                }
            },
            Tables = new List<ExtractedTable>(),
            Warnings = new List<string>()
        };
    }

    private ExtractedFormStructure CreateSurveyFormStructure()
    {
        return new ExtractedFormStructure
        {
            Fields = new List<ExtractedField>
            {
                new ExtractedField
                {
                    Name = "Satisfaction",
                    Label = "Overall Satisfaction",
                    Type = "radio",
                    IsRequired = true,
                    Options = new List<string> { "Very Satisfied", "Satisfied", "Neutral", "Dissatisfied", "Very Dissatisfied" }
                },
                new ExtractedField
                {
                    Name = "Recommend",
                    Label = "Would you recommend our service?",
                    Type = "radio",
                    IsRequired = true,
                    Options = new List<string> { "Yes", "No", "Maybe" }
                },
                new ExtractedField
                {
                    Name = "Features",
                    Label = "Which features do you use?",
                    Type = "checkbox",
                    IsRequired = false,
                    Options = new List<string> { "Feature A", "Feature B", "Feature C", "Feature D" }
                },
                new ExtractedField
                {
                    Name = "Comments",
                    Label = "Additional Comments",
                    Type = "textarea",
                    IsRequired = false,
                    MaxLength = 500
                }
            },
            Tables = new List<ExtractedTable>(),
            Warnings = new List<string>()
        };
    }

    private ExtractedFormStructure CreateGenericFormStructure()
    {
        return new ExtractedFormStructure
        {
            Fields = new List<ExtractedField>
            {
                new ExtractedField
                {
                    Name = "Field1",
                    Label = "Generic Field 1",
                    Type = "text",
                    IsRequired = false,
                    MaxLength = 100
                },
                new ExtractedField
                {
                    Name = "Field2",
                    Label = "Generic Field 2",
                    Type = "text",
                    IsRequired = false,
                    MaxLength = 100
                },
                new ExtractedField
                {
                    Name = "Field3",
                    Label = "Generic Field 3",
                    Type = "text",
                    IsRequired = false,
                    MaxLength = 100
                }
            },
            Tables = new List<ExtractedTable>(),
            Warnings = new List<string>
            {
                "Could not determine specific form type - extracted basic fields"
            }
        };
    }
}

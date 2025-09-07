using System;
using System.ComponentModel.DataAnnotations;
using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.Web.Forms;

public class CreateFormRequest
{
    public const string Route = "/Forms";

    [Required]
    public string? FormNumber { get; set; }

    [Required]
    public string? FormTitle { get; set; }

    public string? Division { get; set; }
    public Owner? Owner { get; set; }
    public string? Version { get; set; }
    public string? ConfigurationPath { get; set; }

}

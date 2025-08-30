Form Definition
[Key]
[Required]
public int Id { get; set; }

    [Required]
    [MaxLength(10)]
    public string FormNumber { get; set; } = null!;

    [Required]
    public string FormTitle { get; set; } = null!;

    [Required]
    public string FormOwnerDivision { get; set; } = null!;

    [Required]
    public string FormOwner { get; set; } = null!;

    [Required]
    public string Version { get; set; } = null!;


    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime RevisedDate { get; set; } = DateTime.UtcNow;

    public string? ConfigurationPath { get; set; }

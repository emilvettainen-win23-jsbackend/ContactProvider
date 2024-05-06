using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactProvider.Entities;

public class ContactEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string FullName { get; set; } = null!;

    [Required]
    public string Email { get; set; } = null!;
    public string? Service { get; set; }

    [Required]
    public string Message { get; set; } = null!;

    [Column(TypeName = "datetime2")]
    public DateTime Created { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime Updated { get; set; }
}

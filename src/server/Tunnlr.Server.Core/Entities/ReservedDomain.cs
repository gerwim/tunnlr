using System.ComponentModel.DataAnnotations;

namespace Tunnlr.Server.Core.Entities;

public class ReservedDomain
{
    [Required]
    public required string UserId { get; set; }

    [Required]
    public User User { get; set; } = null!;
    
    [Required]
    [Key]
    public required string Domain { get; set; }
}
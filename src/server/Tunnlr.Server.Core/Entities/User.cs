using System.ComponentModel.DataAnnotations;

namespace Tunnlr.Server.Core.Entities;

public class User
{
    [Required]
    [Key]
    public required string UserId { get; set; }
}
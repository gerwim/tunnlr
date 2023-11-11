using System.ComponentModel.DataAnnotations;

namespace Tunnlr.Server.Core.Entities;

public class User
{
    [Required]
    public required string UserId { get; set; }
}
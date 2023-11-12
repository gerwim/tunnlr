using System.ComponentModel.DataAnnotations;

namespace Tunnlr.Client.Persistence.Models;

public class ReservedDomain
{
    [Required]
    [Key]
    public required string Domain { get; set; }

    public Guid? TunnelId { get; set; }
    public Tunnel? Tunnel { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tunnlr.Client.Persistence.Models;

public class Tunnel
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public required string TargetUri { get; set; }

    public ReservedDomain? ReservedDomain { get; set; }

    [ForeignKey("TunnelIdRequestInterceptor")]
    public ICollection<InterceptorEntity> RequestInterceptors { get; protected set; } = new List<InterceptorEntity>();

    [ForeignKey("TunnelIdResponseInterceptor")]
    public ICollection<InterceptorEntity> ResponseInterceptors { get; protected set; } = new List<InterceptorEntity>();
}
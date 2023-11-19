using System.Collections.Concurrent;
using Tunnlr.Client.Persistence.Models;

namespace Tunnlr.Client.Core.Models;

public class Tunnel
{
    public required Guid TunnelId { get; set; }
    public required string TargetUri { get; set; }

    public byte[]? SecurityKey { get; set; }
    public TunnelStatus Status { get; set; }
    public string? ServedFrom { get; set; } // TODO: make this "ServedFrom" / "ServedFromHostname" consistent throughout codebase

    public ReservedDomain? ReservedDomain { get; set; }

    public List<InterceptorEntity> RequestInterceptors { get; set; } = new();
    public List<InterceptorEntity> ResponseInterceptors { get; set; } = new();

    public CancellationTokenSource? CancellationTokenSource { get; set; }

    public ConcurrentDictionary<Request, Response?> Requests { get; set; } = new();
    
    public event EventHandler<EventArgs>? ChangedEvent;
    public void NotifyChanged(object? sender, EventArgs e)
        => ChangedEvent?.Invoke(sender, e);
}

public enum TunnelStatus
{
    Stopped,
    Started
}
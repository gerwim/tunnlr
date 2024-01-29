using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Tunnlr.Client.Features.Interceptors.Core;

namespace Tunnlr.Client.Persistence.Models;

public class InterceptorEntity
{
    public InterceptorEntity()
    {
        
    }
    
    [SetsRequiredMembers]
    public InterceptorEntity(IInterceptor interceptor, Dictionary<string, string> values)
    {
        TypeName = interceptor.GetType().AssemblyQualifiedName ?? throw new ArgumentNullException(nameof(interceptor));
        Values = values;
    }
    
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    public required string TypeName { get; set; }
    public Dictionary<string, string> Values { get; protected set; } = new();
    public bool Enabled { get; set; } = false;
}
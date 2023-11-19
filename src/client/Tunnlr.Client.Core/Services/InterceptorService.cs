using Microsoft.EntityFrameworkCore;
using Tunnlr.Client.Features.Interceptors.Core;
using Tunnlr.Client.Persistence;
using Tunnlr.Client.Persistence.Models;

namespace Tunnlr.Client.Core.Services;

public class InterceptorService
{
    private readonly TunnlrClientDbContext _tunnlrClientDbContext;

    public InterceptorService(TunnlrClientDbContext tunnlrClientDbContext)
    {
        _tunnlrClientDbContext = tunnlrClientDbContext;
    }

    public void AddInterceptor(Guid tunnelId, InterceptorEntity interceptorEntity)
    {
        var tunnel = _tunnlrClientDbContext.Tunnels
            .Include(x => x.RequestInterceptors)
            .Include(x => x.ResponseInterceptors)
            .AsTracking()
            .FirstOrDefault(x => x.Id == tunnelId);

        var instance = Activator.CreateInstance(Type.GetType(interceptorEntity.TypeName)!);
        switch (instance)
        {
            case IRequestInterceptor:
                tunnel?.RequestInterceptors.Add(interceptorEntity);
                break;
            case IResponseInterceptor:
                tunnel?.ResponseInterceptors.Add(interceptorEntity);
                break;
        }

        _tunnlrClientDbContext.SaveChanges();
    }
    
    public async Task SetEnabledState(Guid id, bool enabled)
    {
        var entity = await _tunnlrClientDbContext.Interceptors
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);
        if (entity is null) return;

        entity.Enabled = enabled;
        await _tunnlrClientDbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    
    public async Task DeleteInterceptor(Guid id)
    {
        var entity = await _tunnlrClientDbContext.Interceptors
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);
        if (entity is null) return;
        
        _tunnlrClientDbContext.Interceptors.Remove(entity);
        await _tunnlrClientDbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public IEnumerable<InterceptorEntity> GetInterceptorEntities(Guid tunnelId)
    {
        var tunnel = _tunnlrClientDbContext.Tunnels
            .Include(x => x.RequestInterceptors)
            .Include(x => x.ResponseInterceptors)
            .FirstOrDefault(x => x.Id == tunnelId);
        
        return tunnel?.RequestInterceptors.Concat(tunnel.ResponseInterceptors) ?? Array.Empty<InterceptorEntity>();
    }
}
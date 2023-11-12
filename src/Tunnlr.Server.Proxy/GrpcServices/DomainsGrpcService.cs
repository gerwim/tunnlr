using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Tunnlr.Common.Protobuf;
using Tunnlr.Server.Core.Entities;
using Tunnlr.Server.Persistence;

namespace Tunnlr.Server.Proxy.GrpcServices;

[Authorize]
public class DomainsGrpcService : Domains.DomainsBase
{
    private readonly TunnlrDbContext _tunnlrDbContext;

    public DomainsGrpcService(TunnlrDbContext tunnlrDbContext)
    {
        _tunnlrDbContext = tunnlrDbContext;
    }

    public override async Task<CreateReservedDomainResponse> CreateReservedDomain(CreateReservedDomainRequest request, ServerCallContext context)
    {
        var userId = context.GetHttpContext().User.Identity?.Name;
        if (userId is null) throw new RpcException(new Status(StatusCode.Unauthenticated, "User is unauthenticated"));

        var user = await _tunnlrDbContext.Users.FindAsync(userId).ConfigureAwait(false) ?? _tunnlrDbContext.Add(new User
        {
            UserId = userId,
        }).Entity;
        
        var domain = new ReservedDomain
        {
            UserId = user.UserId,
            Domain = $"{request.DomainPrefix}-{Guid.NewGuid().ToString()}",
        };
        _tunnlrDbContext.Add(domain);
        await _tunnlrDbContext.SaveChangesAsync().ConfigureAwait(false);

        return new CreateReservedDomainResponse
        {
            Domain = domain.Domain,
        };
    }

    public override Task<GetReservedDomainsResponse> GetReservedDomains(Empty request, ServerCallContext context)
    {
        var userId = context.GetHttpContext().User.Identity?.Name;
        if (userId is null) throw new RpcException(new Status(StatusCode.Unauthenticated, "User is unauthenticated"));

        var domains = _tunnlrDbContext.ReservedDomains.Where(x => x.UserId == userId).AsEnumerable();

        var response = new GetReservedDomainsResponse();
        response.Domains.AddRange(domains.Select(x => x.Domain));
        
        return Task.FromResult(response);
    }

    public override async Task<Empty> DeleteReservedDomain(DeleteReservedDomainRequest request, ServerCallContext context)
    {
        var userId = context.GetHttpContext().User.Identity?.Name;
        if (userId is null) throw new RpcException(new Status(StatusCode.Unauthenticated, "User is unauthenticated"));

        var domain = await _tunnlrDbContext.ReservedDomains.AsTracking().FirstOrDefaultAsync(x => x.UserId == userId && x.Domain == request.Domain).ConfigureAwait(false);

        if (domain is not null)
        {
            _tunnlrDbContext.ReservedDomains.Remove(domain);
            await _tunnlrDbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        return new Empty();
    }
}
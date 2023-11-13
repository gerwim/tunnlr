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
    private readonly TunnlrServerDbContext _tunnlrServerDbContext;

    public DomainsGrpcService(TunnlrServerDbContext tunnlrServerDbContext)
    {
        _tunnlrServerDbContext = tunnlrServerDbContext;
    }

    public override async Task<CreateReservedDomainResponse> CreateReservedDomain(CreateReservedDomainRequest request, ServerCallContext context)
    {
        var userId = context.GetHttpContext().User.Identity?.Name;
        if (userId is null) throw new RpcException(new Status(StatusCode.Unauthenticated, "User is unauthenticated"));

        var user = await _tunnlrServerDbContext.Users.FindAsync(userId).ConfigureAwait(false) ?? _tunnlrServerDbContext.Add(new User
        {
            UserId = userId,
        }).Entity;
        
        var requestedPrefix = Core.Helpers.Domains.CleanInput(request.DomainPrefix);
        var domain = Core.Helpers.Domains.ValidateAndConvertDomainLength($"{requestedPrefix}-{Guid.NewGuid().ToString()}");
        
        var reservedDomain = new ReservedDomain
        {
            UserId = user.UserId,
            Domain = domain,
        };
        _tunnlrServerDbContext.Add(reservedDomain);
        await _tunnlrServerDbContext.SaveChangesAsync().ConfigureAwait(false);

        return new CreateReservedDomainResponse
        {
            Domain = reservedDomain.Domain,
        };
    }

    public override Task<GetReservedDomainsResponse> GetReservedDomains(Empty request, ServerCallContext context)
    {
        var userId = context.GetHttpContext().User.Identity?.Name;
        if (userId is null) throw new RpcException(new Status(StatusCode.Unauthenticated, "User is unauthenticated"));

        var domains = _tunnlrServerDbContext.ReservedDomains.Where(x => x.UserId == userId).AsEnumerable();

        var response = new GetReservedDomainsResponse();
        response.Domains.AddRange(domains.Select(x => x.Domain));
        
        return Task.FromResult(response);
    }

    public override async Task<Empty> DeleteReservedDomain(DeleteReservedDomainRequest request, ServerCallContext context)
    {
        var userId = context.GetHttpContext().User.Identity?.Name;
        if (userId is null) throw new RpcException(new Status(StatusCode.Unauthenticated, "User is unauthenticated"));

        var domain = await _tunnlrServerDbContext.ReservedDomains.AsTracking().FirstOrDefaultAsync(x => x.UserId == userId && x.Domain == request.Domain).ConfigureAwait(false);

        if (domain is not null)
        {
            _tunnlrServerDbContext.ReservedDomains.Remove(domain);
            await _tunnlrServerDbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        return new Empty();
    }
}
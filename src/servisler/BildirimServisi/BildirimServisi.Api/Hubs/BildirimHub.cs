using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BildirimServisi.Api.Hubs;

[Authorize(Policy = "AdminVeyaITPersoneli")]
public sealed class BildirimHub : Hub
{
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DeluxBarberShop.API.Hubs;

[Authorize]
public class BarberShopHub : Hub { }

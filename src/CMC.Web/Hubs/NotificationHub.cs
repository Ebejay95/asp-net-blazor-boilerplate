using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CMC.Web.Hubs;

[Authorize]
public class NotificationHub : Hub { }

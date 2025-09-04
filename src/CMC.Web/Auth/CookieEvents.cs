// src/CMC.Web/Auth/CookieEvents.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CMC.Web.Auth;

public sealed class CookieEvents : CookieAuthenticationEvents
{
    public override Task SigningIn(CookieSigningInContext context)
    {
        context.Properties.IsPersistent = true;
        context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);

        var claims = context.Principal?.Claims?.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>();
        Console.WriteLine("🔐 SigningIn claims: " + string.Join(", ", claims));

        return Task.CompletedTask;
    }

    public override Task SignedIn(CookieSignedInContext context)
    {
        var claims = context.Principal?.Claims?.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>();
        Console.WriteLine("✅ SignedIn claims: " + string.Join(", ", claims));
        return Task.CompletedTask;
    }

    public override Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var name = context.Principal?.Identity?.Name ?? "(unknown)";
        Console.WriteLine("🔍 Validate principal for: " + name);

        var claims = context.Principal?.Claims?.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>();
        Console.WriteLine("🔎 Current claims: " + string.Join(", ", claims));

        if (context.Principal?.Identity?.IsAuthenticated != true)
        {
            Console.WriteLine("❌ Principal invalid -> reject");
            context.RejectPrincipal();
        }
        else
        {
            Console.WriteLine("✅ Principal valid");
        }
        return Task.CompletedTask;
    }
}

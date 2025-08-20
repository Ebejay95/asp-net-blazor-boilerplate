// src/CMC.Web/Auth/CookieEvents.cs
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CMC.Web.Auth;

public sealed class CookieEvents : CookieAuthenticationEvents
{
    public override Task SigningIn(CookieSigningInContext context)
    {
        context.Properties.IsPersistent = true;
        context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);
        Console.WriteLine("🔐 User signing in: " + context.Principal?.Identity?.Name);
        return Task.CompletedTask;
    }

    public override Task SignedIn(CookieSignedInContext context)
    {
        Console.WriteLine("✅ User signed in: " + context.Principal?.Identity?.Name);
        return Task.CompletedTask;
    }

    public override Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        Console.WriteLine("🔍 Validate principal: " + context.Principal?.Identity?.Name);
        if (context.Principal?.Identity?.IsAuthenticated != true)
        {
            Console.WriteLine("❌ Principal invalid");
            context.RejectPrincipal();
        }
        else
        {
            Console.WriteLine("✅ Principal valid");
        }
        return Task.CompletedTask;
    }
}

using Microsoft.AspNetCore.Authorization;

namespace CMC.Web.Auth
{
    public class MfaSetupRequirement : IAuthorizationRequirement
    {
    }

    public class MfaVerifiedRequirement : IAuthorizationRequirement
    {
    }

    public class NoMfaVerifiedRequirement : IAuthorizationRequirement
    {
    }

    public class MfaSetupHandler : AuthorizationHandler<MfaSetupRequirement>
    {
        private readonly ILogger<MfaSetupHandler> _logger;

        public MfaSetupHandler(ILogger<MfaSetupHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MfaSetupRequirement requirement)
        {
            var user = context.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                _logger.LogDebug("RequireMfaSetup: User not authenticated");
                context.Fail();
                return Task.CompletedTask;
            }

            var mfaVerified = user.FindFirst("mfa_verified")?.Value;
            var allowed = mfaVerified != "true";

            _logger.LogDebug("RequireMfaSetup: mfa_verified={MfaVerified}, allowed={Allowed}", mfaVerified, allowed);

            if (allowed)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }

    public class MfaVerifiedHandler : AuthorizationHandler<MfaVerifiedRequirement>
    {
        private readonly ILogger<MfaVerifiedHandler> _logger;

        public MfaVerifiedHandler(ILogger<MfaVerifiedHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MfaVerifiedRequirement requirement)
        {
            var user = context.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var mfaVerified = user.FindFirst("mfa_verified")?.Value;
            var allowed = mfaVerified == "true";

            if (allowed)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }

    public class NoMfaVerifiedHandler : AuthorizationHandler<NoMfaVerifiedRequirement>
    {
        private readonly ILogger<NoMfaVerifiedHandler> _logger;

        public NoMfaVerifiedHandler(ILogger<NoMfaVerifiedHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            NoMfaVerifiedRequirement requirement)
        {
            var user = context.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var mfaVerified = user.FindFirst("mfa_verified")?.Value;
            var allowed = mfaVerified != "true";

            if (allowed)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}

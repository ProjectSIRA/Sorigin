using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace Sorigin.Authorization
{
    public class RequireScopeHandler : AuthorizationHandler<ScopeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeRequirement requirement)
        {
            var scopeClaim = context.User.FindFirst(c => c.Type == "scope" && c.Issuer == requirement.Issuer);
            if (scopeClaim == null || string.IsNullOrEmpty(scopeClaim.Value))
                return Task.CompletedTask;

            if (scopeClaim.Value.Split(' ').Any(s => s == requirement.Scope))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
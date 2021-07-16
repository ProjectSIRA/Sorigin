using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Sorigin
{
    public static class Extensions
    {
        public static Guid? GetID(this ClaimsPrincipal principal)
        {
            var userIdClaim = principal.FindFirst(c => c.Type == ClaimTypes.NameIdentifier) ?? principal.FindFirst(c => c.Type == "sub");
            if (userIdClaim != null && !string.IsNullOrEmpty(userIdClaim.Value))
            {
                bool didParse = Guid.TryParse(userIdClaim.Value, out Guid result);
                return didParse ? result : null;
            }
            return null;
        }

        // https://sandrino.dev/blog/aspnet-core-5-jwt-authorization
        public static AuthenticationBuilder AddJwtBearerConfiguration(this AuthenticationBuilder builder, string issuer, string audience, string key)
        {
            return builder.AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = issuer,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = new TimeSpan(0, 0, 30),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
                options.Events = new JwtBearerEvents()
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        // Ensure we always have an error and error description.
                        if (string.IsNullOrEmpty(context.Error))
                            context.Error = "invalid_token";
                        if (string.IsNullOrEmpty(context.ErrorDescription))
                            context.ErrorDescription = "This request requires a valid JWT access token to be provided";

                        // Add some extra context for expired tokens.
                        if (context.AuthenticateFailure != null && context.AuthenticateFailure.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            var authenticationException = (context.AuthenticateFailure as SecurityTokenExpiredException)!;
                            context.Response.Headers.Add("x-token-expired", authenticationException.Expires.ToString("o"));
                            context.ErrorDescription = $"The token expired on {authenticationException.Expires:o}";
                        }

                        return context.Response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            error = context.Error,
                            error_description = context.ErrorDescription
                        }));
                    }
                };
            });
        }
    }
}
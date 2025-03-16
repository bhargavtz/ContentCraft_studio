using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Threading.Tasks;

namespace ContentCraft_studio.Middleware
{
    public class EmailVerificationMiddleware
    {
        private readonly RequestDelegate _next;

        public EmailVerificationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (IsEmailVerificationError(ex))
                {
                    context.Response.Redirect("/Account/VerifyEmail");
                    return;
                }
                throw;
            }
        }

        private bool IsEmailVerificationError(Exception ex)
        {
            if (ex is AuthenticationFailureException authEx)
            {
                if (authEx.InnerException is OpenIdConnectProtocolException openIdEx)
                {
                    return openIdEx.Message.Contains("access_denied", StringComparison.OrdinalIgnoreCase) &&
                           openIdEx.Message.Contains("verify your email", StringComparison.OrdinalIgnoreCase);
                }
            }
            return false;
        }
    }

    public static class EmailVerificationMiddlewareExtensions
    {
        public static IApplicationBuilder UseEmailVerificationHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<EmailVerificationMiddleware>();
        }
    }
}
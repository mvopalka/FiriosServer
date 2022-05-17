using Firios.Data;
using FiriosServer.Data;
using Microsoft.EntityFrameworkCore;

namespace FiriosServer;

public class AuthMiddleware
{
    private const string SESSION_NAME = "Session";
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, FiriosSuperLightContext dbcontext, IConfiguration configuration)
    {
        if (context.Request.Cookies.ContainsKey(SESSION_NAME))
        {
            var session = context.Request.Cookies[SESSION_NAME];
            var userBrowserData = dbcontext.UserBrowserDatas.Include(i => i.UserEntity)
                .FirstOrDefault(browserData => browserData.Session == session);
            if (userBrowserData != null)
            {
                context.Items.Add("user", userBrowserData.UserEntity.Email);
                context.Items.Add("PWA_ServerKey", configuration["Vapid:publicKey"]);
                if (userBrowserData.UserEntity.Position == FiriosConstants.VELITEL_JEDNOTKY)
                {
                    context.Items.Add("admin", true);
                }
            }
        }

        // Call the next delegate/middleware in the pipeline.
        await _next(context);
    }
}

public static class RequestCultureMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthMiddleware>();
    }
}
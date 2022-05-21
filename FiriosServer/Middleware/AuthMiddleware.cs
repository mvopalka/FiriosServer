using Firios.Data;
using FiriosServer.Data;
using Microsoft.EntityFrameworkCore;

namespace FiriosServer;

public class AuthMiddleware
{
    private const string user_string = "user";
    private const string pwa_serverkey_string = "PWA_ServerKey";
    private const string admin_string = "admin";
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
                if (!context.Items.ContainsKey(user_string))
                {
                    context.Items.Add(user_string, userBrowserData.UserEntity.Email);
                }

                if (!context.Items.ContainsKey(pwa_serverkey_string))
                {
                    context.Items.Add(pwa_serverkey_string, configuration["Vapid:publicKey"]);
                }
                if (userBrowserData.UserEntity.Position == FiriosConstants.VELITEL_JEDNOTKY)
                {
                    if (!context.Items.ContainsKey(admin_string))
                    {
                        context.Items.Add(admin_string, true);
                    }
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
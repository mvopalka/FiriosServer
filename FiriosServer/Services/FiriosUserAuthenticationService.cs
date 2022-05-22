using FiriosServer.Data;
using FiriosServer.Entity;
using Microsoft.EntityFrameworkCore;

namespace FiriosServer.Services;

public class FiriosUserAuthenticationService
{
    private FiriosSuperLightContext _context;

    public FiriosUserAuthenticationService(FiriosSuperLightContext context)
    {
        _context = context;
    }

    private UserEntity GetUserFromSession(string session)
    {
        var userBrowserData = _context.UserBrowserDatas.Include(i => i.UserEntity)
            .FirstOrDefault(browserData => browserData.Session == session);
        if (userBrowserData == null || userBrowserData.UserEntity == null)
        {
            return null;
        }
        return userBrowserData.UserEntity;
    }

    public bool ValidateUser(UserEntity userEntity, IEnumerable<string> roles)
    {
        foreach (var role in roles)
        {
            if (role == userEntity.Position)
                return true;
        }

        return false;
    }
    public bool ValidateUser(string session, IEnumerable<string> roles)
    {
        var userEntity = GetUserFromSession(session);
        if (userEntity == null)
        {
            return false;
        }
        foreach (var role in roles)
        {
            if (role == userEntity.Position)
                return true;
        }

        return false;
    }
    public bool ValidateUser(HttpRequest httpRequest, IEnumerable<string> roles)
    {
        var session = httpRequest.Cookies[FiriosConstants.SESSION_NAME];
        if (string.IsNullOrEmpty(session))
        {
            return false;
        }
        var userEntity = GetUserFromSession(session);
        if (userEntity == null)
        {
            return false;
        }
        foreach (var role in roles)
        {
            if (role == userEntity.Position)
                return true;
        }

        return false;
    }
    public UserEntity? GetUserFromRequest(HttpRequest httpRequest)
    {
        var session = httpRequest.Cookies[FiriosConstants.SESSION_NAME];
        if (string.IsNullOrEmpty(session))
        {
            return null;
        }
        var userEntity = GetUserFromSession(session);
        if (userEntity == null)
        {
            return null;
        }

        return userEntity;
    }
}
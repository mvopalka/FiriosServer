using Microsoft.AspNetCore.Mvc;

namespace FiriosServer.Extension;

public static class FiriosExtensions
{
    public static string GetControllerName<T>() where T : Controller
    {
        return typeof(T).Name.Replace("Controller", "");
    }
}
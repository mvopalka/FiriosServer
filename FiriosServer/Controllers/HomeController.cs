using Firios.Data;
using FiriosServer.Data;
using FiriosServer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FiriosServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FiriosSuperLightContext _context;
        private readonly FiriosAuthenticationService _authenticationService;

        public HomeController(ILogger<HomeController> logger, FiriosSuperLightContext context, FiriosAuthenticationService authenticationService)
        {
            _logger = logger;
            _context = context;
            _authenticationService = authenticationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> UserConfirmAction(Guid? id)
        {
            if (!_authenticationService.ValidateUser(Request, new List<string>() { "Hasič" }))
            {
                return RedirectToRoute(nameof(UserController), nameof(UserController.Login));
            }
            if (id == null)
            {
                return NotFound();
            }
            var incidentEntity = await _context.IncidentEntity.FindAsync(id);
            if (incidentEntity == null)
            {
                return NotFound();
            }
            return View(incidentEntity);
        }

        //public IActionResult Technic()
        //{
        //    return View();
        //}

        //public IActionResult UserAdministration()
        //{
        //    return View();
        //}

        //public IActionResult Accounting()
        //{
        //    return View();
        //}

        public async Task<IActionResult> InteractiveIncident(Guid? id)
        {
            if (!_authenticationService.ValidateUser(Request, new List<string>() { "Hasič" }))
            {
                return RedirectToRoute(nameof(UserController), nameof(UserController.Login));
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
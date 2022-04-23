using Firios.Data;
using FiriosServer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FiriosServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FiriosSuperLightContext _context;

        public HomeController(ILogger<HomeController> logger, FiriosSuperLightContext context)
        {
            _logger = logger;
            _context = context;
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
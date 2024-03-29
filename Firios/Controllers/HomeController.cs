﻿using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Firios.Data;
using Firios.Extension;
using Firios.Models;
using Firios.Services;

namespace Firios.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FiriosSuperLightContext _context;
        private readonly FiriosUserAuthenticationService _userAuthenticationService;
        private readonly FiriosSourceAuthentificationService _sourceAuthentificationService;

        public HomeController(ILogger<HomeController> logger,
            FiriosSuperLightContext context,
            FiriosUserAuthenticationService userAuthenticationService,
            FiriosSourceAuthentificationService sourceAuthentificationService)
        {
            _logger = logger;
            _context = context;
            _userAuthenticationService = userAuthenticationService;
            _sourceAuthentificationService = sourceAuthentificationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        public async Task<IActionResult> Notifiers()
        {
            if (!_userAuthenticationService.ValidateUser(Request,
                    new List<string>()
                    {
                        FiriosConstants.VELITEL_JEDNOTKY
                    }))
            {
                return RedirectToAction(nameof(UserController.Login), FiriosExtensions.GetControllerName<UserController>());
            }

            var model = _sourceAuthentificationService.GetIds();

            return View(model);
        }

        public async Task<IActionResult> NotifierAdd(string id)
        {
            if (!_userAuthenticationService.ValidateUser(Request,
                    new List<string>()
                    {
                        FiriosConstants.VELITEL_JEDNOTKY
                    }))
            {
                return RedirectToAction(nameof(UserController.Login), FiriosExtensions.GetControllerName<UserController>());
            }

            _sourceAuthentificationService.GenerateSignature(id);

            return RedirectToAction(nameof(Notifiers));
        }

        public async Task<IActionResult> UserConfirmAction(Guid? id)
        {
            if (!_userAuthenticationService.ValidateUser(Request,
                    new List<string>()
                    {
                        FiriosConstants.HASIC,
                        FiriosConstants.STROJNIK,
                        FiriosConstants.VELITEL,
                        FiriosConstants.VELITEL_JEDNOTKY
                    }))
            {
                return RedirectToAction(nameof(UserController.Login), FiriosExtensions.GetControllerName<UserController>());
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
            if (!_userAuthenticationService.ValidateUser(Request,
                    new List<string>()
                    {
                        FiriosConstants.HASIC,
                        FiriosConstants.STROJNIK,
                        FiriosConstants.VELITEL,
                        FiriosConstants.VELITEL_JEDNOTKY,
                        FiriosConstants.MONITOR
                    }))
            {
                return RedirectToAction(nameof(UserController.Login), FiriosExtensions.GetControllerName<UserController>());
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
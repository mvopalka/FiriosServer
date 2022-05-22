#nullable disable
using Firios.Data;
using Firios.Extension;
using Firios.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firios.Controllers
{
    public class IncidentController : Controller
    {
        private readonly FiriosSuperLightContext _context;
        private readonly FiriosUserAuthenticationService _userAuthenticationService;

        public IncidentController(FiriosSuperLightContext context, FiriosUserAuthenticationService userAuthenticationService)
        {
            _context = context;
            _userAuthenticationService = userAuthenticationService;
        }

        // GET: Incident
        public async Task<IActionResult> Index()
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
            var incidents = await _context.IncidentEntity.ToListAsync();
            incidents.Sort((x, y) => y.Date.CompareTo(x.Date));
            return View(incidents);
        }

        // GET: Incident/Details/5
        public async Task<IActionResult> Details(Guid? id)
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

            var incidentEntity = await _context.IncidentEntity
                .Include(i => i.Users)
                .ThenInclude(i => i.UserEntity)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (incidentEntity == null)
            {
                return NotFound();
            }

            incidentEntity.Users.Sort((x, y) =>
                string.Compare(y.UserEntity.SecondName, x.UserEntity.SecondName, StringComparison.Ordinal));

            return View(incidentEntity);
        }

        public async Task<IActionResult> ChangeIncidentStatus(Guid id)
        {
            if (!_userAuthenticationService.ValidateUser(Request,
                    new List<string>()
                    {
                        FiriosConstants.VELITEL_JEDNOTKY
                    }))
            {
                return RedirectToAction(nameof(UserController.Login), FiriosExtensions.GetControllerName<UserController>());
            }
            var incident = await _context.IncidentEntity.FirstOrDefaultAsync(i => i.Id == id);
            if (incident == null)
            {
                return RedirectToAction(nameof(Index));
            }

            incident.IsActive = !incident.IsActive;
            _context.Update(incident);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = id });

        }

        // TODO VELITEL_JEDNOTKY should be able to disable incident

        //// GET: Incident/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Incident/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Mpd,Region,Address,SubType,Type,ObjectName,AdditionalInformation,Level,IsActive,Date,Id")] IncidentEntity incidentEntity)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        incidentEntity.Id = Guid.NewGuid();
        //        _context.Add(incidentEntity);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(incidentEntity);
        //}

        //// GET: Incident/Edit/5
        //public async Task<IActionResult> Edit(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var incidentEntity = await _context.IncidentEntity.FindAsync(id);
        //    if (incidentEntity == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(incidentEntity);
        //}

        //// POST: Incident/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(Guid id, [Bind("Mpd,Region,Address,SubType,Type,ObjectName,AdditionalInformation,Level,IsActive,Date,Id")] IncidentEntity incidentEntity)
        //{
        //    if (id != incidentEntity.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(incidentEntity);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!IncidentEntityExists(incidentEntity.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(incidentEntity);
        //}

        //// GET: Incident/Delete/5
        //public async Task<IActionResult> Delete(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var incidentEntity = await _context.IncidentEntity
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (incidentEntity == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(incidentEntity);
        //}

        //// POST: Incident/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(Guid id)
        //{
        //    var incidentEntity = await _context.IncidentEntity.FindAsync(id);
        //    _context.IncidentEntity.Remove(incidentEntity);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool IncidentEntityExists(Guid id)
        {
            return _context.IncidentEntity.Any(e => e.Id == id);
        }
    }
}

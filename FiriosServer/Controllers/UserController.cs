#nullable disable
using Firios.Data;
using Firios.Entity;
using FiriosServer.Data;
using FiriosServer.Models.InputModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FiriosServer.Controllers
{
    public class UserController : Controller
    {
        // TODO: Check all password inputs for safe password
        private readonly FiriosSuperLightContext _context;
        private readonly FiriosAuthenticationService _authenticationService;

        public UserController(FiriosSuperLightContext context, FiriosAuthenticationService authenticationService)
        {
            _context = context;
            _authenticationService = authenticationService;
        }

        public IActionResult Login()
        {
            if (!_context.UserEntity.Any())
            {
                return RedirectToAction(nameof(Create));
            }
            return View();
        }

        // POST: User/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Titules,FirstName,MiddleName,SecondName,Email,Password,ConfirmPassword,Position,Id")] UserLoginModel userLoginModel)
        {
            if (ModelState.IsValid)
            {
                var userEntity = _context.UserEntity.Include(i => i.BrowserData).FirstOrDefault(user => user.Email == userLoginModel.Email);
                if (userEntity != null && userLoginModel.IsValidPassword(userEntity))
                {
                    string session = userLoginModel.GenerateSessionId(userEntity.Id.ToString());
                    Response.Cookies.Append(FiriosConstants.SESSION_NAME, session);
                    UserBrowserData browserData = new UserBrowserData
                    {
                        Session = session,
                        UserEntity = userEntity
                    };
                    userEntity.BrowserData.Append(browserData);
                    _context.Add(browserData);
                    _context.Update(userEntity);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));

                }
            }
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            if (!Request.Cookies.ContainsKey(FiriosConstants.SESSION_NAME) && string.IsNullOrEmpty(Request.Cookies[FiriosConstants.SESSION_NAME]))
            {
                return RedirectToAction(nameof(Login));
            }
            var userBrowserData = _context.UserBrowserDatas.Include(i => i.UserEntity)
                .FirstOrDefault(browserData => browserData.Session == Request.Cookies[FiriosConstants.SESSION_NAME]);
            if (userBrowserData != null)
            {
                _context.Remove(userBrowserData);
                await _context.SaveChangesAsync();
                Response.Cookies.Delete(FiriosConstants.SESSION_NAME);
            }
            return Redirect(nameof(Index));
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            if (!_authenticationService.ValidateUser(Request,
                    new List<string>()
                    {
                        FiriosConstants.HASIC,
                        FiriosConstants.STROJNIK,
                        FiriosConstants.VELITEL,
                        FiriosConstants.VELITEL_JEDNOTKY
                    }))
            {
                return RedirectToAction(nameof(Login));
            }
            var users = await _context.UserEntity.ToListAsync();
            users.Sort((x, y) => String.Compare(x.SecondName, y.SecondName));
            return View(users);
        }


        // GET: User/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            // TODO Chose permissions for this action
            if (!_authenticationService.ValidateUser(Request,
                    new List<string>()
                    {
                        FiriosConstants.HASIC,
                        FiriosConstants.STROJNIK,
                        FiriosConstants.VELITEL,
                        FiriosConstants.VELITEL_JEDNOTKY
                    }))
            {
                return RedirectToAction(nameof(Login));
            }
            if (id == null)
            {
                return NotFound();
            }

            var userEntity = await _context.UserEntity
                .Include(i => i.Incidents)
                .ThenInclude(i => i.IncidentEntity)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userEntity == null)
            {
                return NotFound();
            }

            userEntity.Incidents.Sort((x, y) => y.IncidentEntity.Date.CompareTo(x.IncidentEntity.Date));

            return View(userEntity);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            if (!_context.UserEntity.Any())
            {
                return View();
            }
            if (!_authenticationService.ValidateUser(Request,
                    new List<string>()
                    {
                        FiriosConstants.VELITEL_JEDNOTKY
                    }))
            {
                return RedirectToAction(nameof(Login));
            }
            return View();
        }

        // POST: User/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Titules,FirstName,MiddleName,SecondName,Email,Password,ConfirmPassword,Position,Id")] UserRegistrationModel userRegistrationModel)
        {
            if (!_context.UserEntity.Any())
            {
                var userEntity = userRegistrationModel.ToUserEntity();
                userEntity.Position = FiriosConstants.VELITEL_JEDNOTKY;
                _context.Add(userEntity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Login));
            }
            if (!_authenticationService.ValidateUser(Request,
                    new List<string>()
                    {
                        FiriosConstants.VELITEL_JEDNOTKY
                    }))
            {
                return RedirectToAction(nameof(Login));
            }
            if (ModelState.IsValid &&
                (userRegistrationModel.Position == FiriosConstants.VELITEL_JEDNOTKY ||
                 userRegistrationModel.Position == FiriosConstants.HASIC ||
                 userRegistrationModel.Position == FiriosConstants.STROJNIK ||
                 userRegistrationModel.Position == FiriosConstants.VELITEL ||
                 userRegistrationModel.Position == FiriosConstants.MONITOR))
            {
                if (await _context.UserEntity.FirstOrDefaultAsync(i => i.Email == userRegistrationModel.Email) != null)
                {
                    ViewBag.Error = "Email už existuje";
                    return View(userRegistrationModel);
                }
                var userEntity = userRegistrationModel.ToUserEntity();
                _context.Add(userEntity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userRegistrationModel);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (!_authenticationService.ValidateUser(Request,
                    new List<string>()
                    {
                        FiriosConstants.VELITEL_JEDNOTKY
                    }))
            {
                return RedirectToAction(nameof(Login));
            }
            if (id == null)
            {
                return NotFound();
            }

            var userEntity = await _context.UserEntity.FindAsync(id);
            if (userEntity == null)
            {
                return NotFound();
            }
            return View(UserEditModel.From(userEntity));
        }

        // POST: User/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Titules,FirstName,MiddleName,SecondName,Email,Password,ConfirmPassword,Position")] UserEditModel userEditModel)
        {
            // TODO let user to change for example password
            if (!_authenticationService.ValidateUser(Request,
                    new List<string>()
                    {
                        FiriosConstants.VELITEL_JEDNOTKY
                    }))
            {
                return RedirectToAction(nameof(Login));
            }
            //if (id != userEditModel.Id)
            //{
            //    return NotFound();
            //}
            var userEntity = await _context.UserEntity.FindAsync(id);
            if (ModelState.IsValid &&
                (userEditModel.Position == FiriosConstants.VELITEL_JEDNOTKY ||
                 userEditModel.Position == FiriosConstants.HASIC ||
                 userEditModel.Position == FiriosConstants.STROJNIK ||
                 userEditModel.Position == FiriosConstants.VELITEL))
            {
                if (userEntity.Position == FiriosConstants.VELITEL_JEDNOTKY &&
                    !String.IsNullOrEmpty(userEditModel.Position) &&
                    userEditModel.Position != FiriosConstants.VELITEL_JEDNOTKY)
                {
                    if (_context.UserEntity.Count(i => i.Position == FiriosConstants.VELITEL_JEDNOTKY) < 2)
                    {
                        return View(UserEditModel.From(userEntity));
                    }
                }
                userEntity = userEditModel.ToUserEntity(userEntity);
                try
                {
                    _context.Update(userEntity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserEntityExists(userEntity.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(UserEditModel.From(userEntity));
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (!_authenticationService.ValidateUser(Request,
                    new List<string>()
                    {
                        FiriosConstants.VELITEL_JEDNOTKY
                    }))
            {
                return RedirectToAction(nameof(Login));
            }

            if (id == null)
            {
                return NotFound();
            }

            var userEntity = await _context.UserEntity
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userEntity == null)
            {
                return NotFound();
            }

            return View(userEntity);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (!_authenticationService.ValidateUser(Request,
                    new List<string>()
                    {
                        FiriosConstants.VELITEL_JEDNOTKY
                    }))
            {
                return RedirectToAction(nameof(Login));
            }
            var userEntity = await _context.UserEntity.FindAsync(id);
            if (userEntity.Position == FiriosConstants.VELITEL_JEDNOTKY && _context.UserEntity.Count(i => i.Position == FiriosConstants.VELITEL_JEDNOTKY) < 2)
            {
                return RedirectToAction(nameof(Index));
            }
            _context.UserEntity.Remove(userEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserEntityExists(Guid id)
        {
            return _context.UserEntity.Any(e => e.Id == id);
        }

        //private UserEntity GetUserFromSession(string session)
        //{
        //    var userBrowserData = _context.UserBrowserDatas.Include(i => i.UserEntity)
        //        .FirstOrDefault(browserData => browserData.Session == session);
        //    return userBrowserData.UserEntity;
        //}

        //public bool ValidateUser(UserEntity userEntity, IEnumerable<string> roles)
        //{
        //    foreach (var role in roles)
        //    {
        //        if (role == userEntity.Position)
        //            return true;
        //    }

        //    return false;
        //}
        //public bool ValidateUser(string session, IEnumerable<string> roles)
        //{
        //    var userEntity = GetUserFromSession(session);
        //    foreach (var role in roles)
        //    {
        //        if (role == userEntity.Position)
        //            return true;
        //    }

        //    return false;
        //}
        //public bool ValidateUser(HttpRequest httpRequest, IEnumerable<string> roles)
        //{
        //    var session = httpRequest.Cookies[SESSION_NAME];
        //    if (string.IsNullOrEmpty(session))
        //    {
        //        return false;
        //    }
        //    var userEntity = GetUserFromSession(session);
        //    if (userEntity == null)
        //    {
        //        return false;
        //    }
        //    foreach (var role in roles)
        //    {
        //        if (role == userEntity.Position)
        //            return true;
        //    }

        //    return false;
        //}

        public async Task<IActionResult> ChangeUserPassword()
        {
            var user = _authenticationService.GetUserFromRequest(Request);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View();
        }

        // POST: User/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserPassword([Bind("PasswordOld,Password,ConfirmPassword")] UserChangePasswordModel changePasswordModel)
        {
            var user = _authenticationService.GetUserFromRequest(Request);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (ModelState.IsValid && changePasswordModel.IsValidPassword(user))
            {
                user = changePasswordModel.ToUserEntity(user);
                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            if (!changePasswordModel.IsValidPassword(user))
            {

            }
            ViewBag.Error = "Špatné staré heslo";
            return View();

        }
    }
}

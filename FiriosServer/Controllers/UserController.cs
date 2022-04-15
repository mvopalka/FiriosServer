#nullable disable
using Firios.Data;
using Firios.Entity;
using FiriosServer.Models.InputModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FiriosServer.Controllers
{
    public class UserController : Controller
    {
        private string SESSION_NAME = "Session";

        // TODO: Check all password inputs for safe password
        private readonly FiriosSuperLightContext _context;

        public UserController(FiriosSuperLightContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
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
                    Response.Cookies.Append(SESSION_NAME, session);
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

        // GET: User
        public async Task<IActionResult> Index()
        {
            return View(await _context.UserEntity.ToListAsync());
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
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

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Titules,FirstName,MiddleName,SecondName,Email,Password,ConfirmPassword,Position,Id")] UserRegistrationModel userRegistrationModel)
        {
            if (ModelState.IsValid)
            {
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
            //if (id != userEditModel.Id)
            //{
            //    return NotFound();
            //}
            var userEntity = await _context.UserEntity.FindAsync(id);
            if (ModelState.IsValid)
            {
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
            var userEntity = await _context.UserEntity.FindAsync(id);
            _context.UserEntity.Remove(userEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserEntityExists(Guid id)
        {
            return _context.UserEntity.Any(e => e.Id == id);
        }
    }
}

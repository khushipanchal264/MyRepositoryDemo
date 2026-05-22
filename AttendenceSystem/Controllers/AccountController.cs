using AttendenceSystem.Data;
using AttendenceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendenceSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        //[HttpPost]
        //public IActionResult Login(Employee user)
        //{
        //    var emp = _context.Employees
        //        .FirstOrDefault(x => x.Email == user.Email && x.Password == user.Password);

        //    if (emp != null)
        //    {
        //        // 👉 Create new login session
        //        _context.UserSessions.Add(new UserSession
        //        {
        //            EmployeeId = emp.Id,
        //            LoginTime = DateTime.Now,
        //            LogoutTime = null
        //        });

        //        _context.SaveChanges();

        //        // 👉 Store session (optional but useful)
        //        HttpContext.Session.SetInt32("EmployeeId", emp.Id);

        //        return RedirectToAction("Dashboard");
        //    }

        //    ViewBag.Error = "Invalid Email or Password";
        //    return View();
        //}
        //use for login
        [HttpPost]
        public IActionResult Login(Employee user)
        {
            var info = _context.Employees
                .FirstOrDefault(x => x.Email == user.Email && x.Password == user.Password);
            if (info != null)
            {
                HttpContext.Session.SetInt32("EmpId", info.Id);
                //HttpContext.Session.SetInt32("EmpId", info.Id);
                HttpContext.Session.SetString("UserName", info.Name);
                HttpContext.Session.SetString("UserEmail", info.Email);
                // ✅ GET GROUP NAME
                var groupName = _context.Groups
                    .Where(g => g.Id == info.GroupId)
                    .Select(g => g.GroupName)
                    .FirstOrDefault();
                HttpContext.Session.SetString("UserGroup", groupName ?? "N/A");

                // ❌ REMOVE THIS CHECK (VERY IMPORTANT)
                // existingSession logic should NOT block new login

                var session = new UserSession
                {
                    EmployeeId = info.Id,
                    LoginTime = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                };

                _context.UserSessions.Add(session);
                _context.SaveChanges();

                return RedirectToAction("Dashboard", "Home");
            }

            return View();
        }
        //public IActionResult Login(Employee user)
        //{
        //    var info = _context.Employees
        //        .FirstOrDefault(x => x.Email == user.Email && x.Password == user.Password);

        //    if (info != null)
        //    {
        //        // ✅ Store session
        //        HttpContext.Session.SetInt32("EmpId", info.Id);
        //        HttpContext.Session.SetString("UserName", info.Name);

        //        var existingSession = _context.UserSessions
        //            .FirstOrDefault(x => x.EmployeeId == info.Id && x.LogoutTime == null);

        //        if (existingSession == null)
        //        {
        //            var session = new UserSession
        //            {
        //                EmployeeId = info.Id,

        //                LoginTime = DateTime.UtcNow,
        //                CreatedDate = DateTime.UtcNow
        //            };

        //            _context.UserSessions.Add(session);
        //            _context.SaveChanges();
        //        }

        //        return RedirectToAction("Dashboard", "Home");
        //    }

        //    ViewBag.Error = "Invalid credentials";
        //    return View();
        //}

        //public IActionResult Logout()
        //{
        //    int? empId = HttpContext.Session.GetInt32("EmpId");

        //    if (empId != null)
        //    {
        //        var lastSession = _context.UserSessions
        //            .Where(x => x.EmployeeId == empId && x.LogoutTime == null)
        //            .OrderByDescending(x => x.LoginTime)
        //            .FirstOrDefault();

        //        if (lastSession != null)
        //        {
        //            lastSession.LogoutTime = DateTime.UtcNow;

        //            // ✅ Calculate working time
        //            lastSession.TotalTime = lastSession.LogoutTime.Value - lastSession.LoginTime;

        //            try
        //            {
        //                _context.SaveChanges();
        //            }
        //            catch (DbUpdateException ex)
        //            {

        //            }
        //        }
        //    }

        //    HttpContext.Session.Clear();

        //    return RedirectToAction("Login");
        //}
        public IActionResult Logout()
        {
            int? empId = HttpContext.Session.GetInt32("EmpId");
            if (empId != null)
            {
                // ✅ Get ONLY latest active session
                var session = _context.UserSessions
                    .Where(x => x.EmployeeId == empId && x.LogoutTime == null)
                    .OrderByDescending(x => x.LoginTime)
                    .FirstOrDefault();
                if (session != null)
                {
                    session.LogoutTime = DateTime.UtcNow;
                    // ✅ Calculate total time
                    session.TotalTime = session.LogoutTime.Value - session.LoginTime;

                    _context.SaveChanges();
                }
            }
            // ✅ Clear session
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}
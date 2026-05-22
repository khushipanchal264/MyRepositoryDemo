using AttendenceSystem.Data;
using AttendenceSystem.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;
namespace AttendenceSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Dashboard(int? year, int? month, int? groupId, int? employeeId)
        {
            var now = DateTime.UtcNow;
            int safeYear = year ?? now.Year;
            int safeMonth = month ?? now.Month;
            int daysInMonth = DateTime.DaysInMonth(safeYear, safeMonth);
            var ist = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            ViewBag.Years = Enumerable.Range(now.Year - 5, 6).Reverse().ToList();
            ViewBag.Months = Enumerable.Range(1, 12).ToList();
            ViewBag.Groups = _context.Groups.ToList();

            var employees = _context.Employees
                .Include(e => e.Group)
                .Where(e => !groupId.HasValue || e.GroupId == groupId)
                .ToList();

            if (employeeId.HasValue)
                employees = employees.Where(e => e.Id == employeeId.Value).ToList();  
            var sessions = _context.UserSessions
                .AsEnumerable()
                .Where(x =>
                {
                    var local = TimeZoneInfo.ConvertTime(x.LoginTime, ist);
                    return local.Year == safeYear && local.Month == safeMonth;
                })
                .ToList();

            var result = new List<DashboardViewModel>();
            int count = 1;

            foreach (var emp in employees)
            {
                var dailyHours = new Dictionary<int, TimeSpan>();
                TimeSpan totalMonth = TimeSpan.Zero;

                var empSessions = sessions.Where(x => x.EmployeeId == emp.Id).ToList();
                bool isRunning = empSessions.Any(x => x.LogoutTime == null);

                var groupedData = empSessions
    .Where(x => x.TotalTime.HasValue)
    .GroupBy(x => TimeZoneInfo.ConvertTimeFromUtc(x.LoginTime, ist).Day)
    .ToDictionary(
        g => g.Key,
        g => g.Aggregate(TimeSpan.Zero, (sum, x) => sum + (x.TotalTime ?? TimeSpan.Zero))
    );

                for (int day = 1; day <= daysInMonth; day++)
                {
                    if (groupedData.TryGetValue(day, out var ts))
                    {
                        dailyHours[day] = ts;
                        totalMonth += ts;
                    }
                    else
                    {
                        dailyHours[day] = TimeSpan.Zero;
                    }
                }

                result.Add(new DashboardViewModel
                {
                    SerialNumber = count++,
                    EmployeeName = emp.Name,
                    GroupName = emp.Group?.GroupName,
                    DailyHours = dailyHours,
                    TotalWorkingHours = totalMonth,
                    Sessions = empSessions,
                    Status = isRunning ? "Running" : null,
                    Month = safeMonth,
                    Year = safeYear
                });
            }

            return View(result);
        }
        //        public IActionResult Dashboard(int? year, int? month, int? groupId, int? employeeId)
        //        {
        //            var now = DateTime.UtcNow;
        //            int safeYear = year ?? now.Year;
        //            int safeMonth = month ?? now.Month;

        //            int daysInMonth = DateTime.DaysInMonth(safeYear, safeMonth);

        //            ViewBag.Years = Enumerable.Range(now.Year - 5, 6).Reverse().ToList();
        //            ViewBag.Months = Enumerable.Range(1, 12).ToList();
        //            ViewBag.Groups = _context.Groups.ToList();

        //            var employees = _context.Employees
        //                .Include(e => e.Group)
        //                .Where(e => !groupId.HasValue || e.GroupId == groupId)
        //                .ToList();

        //            if (employeeId.HasValue)
        //                employees = employees.Where(e => e.Id == employeeId.Value).ToList();

        //            // ✅ TIMEZONE FIX
        //            var ist = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        //            // ✅ FIXED SESSION FILTER
        //            var sessions = _context.UserSessions
        //                .AsEnumerable()
        //                .Where(x =>
        //                {
        //                    var local = TimeZoneInfo.ConvertTimeFromUtc(x.LoginTime, ist);
        //                    return local.Year == safeYear &&
        //                           local.Month == safeMonth &&
        //                           x.TotalTime != null;
        //                })
        //                .ToList();

        //            var result = new List<DashboardViewModel>();
        //            int count = 1;

        // ✅ ONLY ONE LOOP (FINAL CORRECT)
        //foreach (var emp in employees)
        //{
        //    var dailyHours = new Dictionary<int, TimeSpan>();
        //    TimeSpan totalMonth = TimeSpan.Zero;

        //    var empSessions = sessions.Where(x => x.EmployeeId == emp.Id).ToList();

        //    for (int day = 1; day <= daysInMonth; day++)
        //    {
        //        var date = new DateTime(safeYear, safeMonth, day);

        //        var totalDayTime = empSessions
        //            .Where(x =>
        //            {
        //                var local = TimeZoneInfo.ConvertTimeFromUtc(x.LoginTime, ist);
        //                return local.Date == date.Date;
        //            })
        //            .Aggregate(TimeSpan.Zero,
        //                (sum, x) => sum + (x.TotalTime ?? TimeSpan.Zero));

        //        dailyHours[day] = totalDayTime;
        //        totalMonth += totalDayTime;
        //    }

        //    result.Add(new DashboardViewModel
        //    {
        //        SerialNumber = count++,
        //        EmployeeName = emp.Name,
        //        GroupName = emp.Group?.GroupName,
        //        DailyHours = dailyHours,
        //        TotalWorkingHours = totalMonth,
        //        Sessions = empSessions,
        //        Status = null,
        //        Month = safeMonth,
        //        Year = safeYear
        //    });
        //}
        //    var ist = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        //    // ... later reuse `ist` (do not redeclare)
        //    var groupedData = empSessions
        //        .GroupBy(x => TimeZoneInfo.ConvertTimeFromUtc(x.LoginTime, ist).Day)

        //    foreach (var emp in employees)
        //    {
        //        var dailyHours = new Dictionary<int, TimeSpan>();
        //        TimeSpan totalMonth = TimeSpan.Zero;

        //        var empSessions = sessions.Where(x => x.EmployeeId == emp.Id).ToList();

        //        // ✅ GROUP BY DAY (MAIN FIX)
        //        var groupedData = empSessions
        //            .GroupBy(x => TimeZoneInfo.ConvertTimeFromUtc(x.LoginTime, ist).Day)
        //            .ToDictionary(
        //                g => g.Key,
        //                g => g.Sum(x => x.TotalTime ?? TimeSpan.Zero)
        //            );

        //        for (int day = 1; day <= daysInMonth; day++)
        //        {
        //            if (groupedData.ContainsKey(day))
        //            {
        //                dailyHours[day] = groupedData[day];
        //                totalMonth += groupedData[day];
        //            }
        //            else
        //            {
        //                dailyHours[day] = TimeSpan.Zero;
        //            }
        //        }

        //        result.Add(new DashboardViewModel
        //        {
        //            SerialNumber = count++,
        //            EmployeeName = emp.Name,
        //            GroupName = emp.Group?.GroupName,
        //            DailyHours = dailyHours,
        //            TotalWorkingHours = totalMonth,
        //            Sessions = empSessions,
        //            Status = null,
        //            Month = safeMonth,
        //            Year = safeYear
        //        });
        //    }

        //    return View(result);
        //}
        //            var sessions = _context.UserSessions
        //    .Where(x => x.LoginTime.Year == safeYear &&
        //                x.LoginTime.Month == safeMonth &&
        //                x.TotalTime != null &&
        //                x.LogoutTime != null)
        //    .ToList();

        //foreach (var emp in employees)
        //{
        //    var dailyHours = new Dictionary<int, TimeSpan>();
        //    TimeSpan totalMonth = TimeSpan.Zero;

        //    var empSessions = sessions.Where(x => x.EmployeeId == emp.Id).ToList();

        //    var grouped = empSessions
        //        .GroupBy(x => x.LoginTime.Date)
        //        .ToDictionary(
        //            g => g.Key.Day,
        //            g => g.Sum(x => x.TotalTime ?? TimeSpan.Zero)
        //        );

        //    for (int day = 1; day <= daysInMonth; day++)
        //    {
        //        if (grouped.ContainsKey(day))
        //        {
        //            dailyHours[day] = grouped[day];
        //            totalMonth += grouped[day];
        //        }
        //        else
        //        {
        //            dailyHours[day] = TimeSpan.Zero;
        //        }
        //    }

        //                result.Add(new DashboardViewModel
        //                {
        //                    SerialNumber = count++,
        //                    EmployeeName = emp.Name,
        //                    GroupName = emp.Group?.GroupName,
        //                    DailyHours = dailyHours,
        //                    TotalWorkingHours = totalMonth,
        //                    Sessions = empSessions,
        //                    Status = null,
        //                    Month = safeMonth,
        //                    Year = safeYear
        //                });

        //        return View(result);
        //}
        // code for dashboard
        //public IActionResult Dashboard(int? year, int? month, int? groupId, int? employeeId)
        //{
        //    var now = DateTime.Now;

        //    // sanitize inputs
        //    int safeYear = (year.HasValue && year.Value >= 2000 && year.Value <= now.Year + 1) ? year.Value : now.Year;
        //    int safeMonth = (month.HasValue && month.Value >= 1 && month.Value <= 12) ? month.Value : now.Month;

        //    int daysInMonth = DateTime.DaysInMonth(safeYear, safeMonth);

        //    // Dropdowns
        //    ViewBag.Years = Enumerable.Range(now.Year - 5, 6).Reverse().ToList();
        //    ViewBag.Months = Enumerable.Range(1, 12).ToList();
        //    ViewBag.Groups = _context.Groups.ToList();

        //    var employees = _context.Employees
        //        .Include(e => e.Group)
        //        .Where(e => !groupId.HasValue || e.GroupId == groupId)
        //        .ToList();

        //    if (employeeId.HasValue)
        //        employees = employees.Where(e => e.Id == employeeId.Value).ToList();

        //    var sessions = _context.UserSessions
        //        .AsEnumerable() // required for ToLocalTime()
        //        .Where(x => x.LoginTime.ToLocalTime().Year == safeYear &&
        //                    x.LoginTime.ToLocalTime().Month == safeMonth)
        //        .ToList();

        //    var result = new List<DashboardViewModel>();
        //    int count = 1;

        //    foreach (var emp in employees)
        //    {
        //        var dailyHours = new Dictionary<int, TimeSpan>();
        //        TimeSpan totalMonth = TimeSpan.Zero;

        //        var empSessions = sessions.Where(x => x.EmployeeId == emp.Id).ToList();
        //        bool isRunning = empSessions.Any(x => x.LogoutTime == null);

        //        for (int day = 1; day <= daysInMonth; day++)
        //        {
        //            var date = new DateTime(safeYear, safeMonth, day);

        //            var daySessions = empSessions
        //                .Where(x => x.LoginTime.ToLocalTime().Date == date.Date)
        //                .ToList();

        //            TimeSpan totalDayTime = TimeSpan.Zero;

        //            foreach (var s in daySessions)
        //            {
        //                var login = s.LoginTime.ToLocalTime();
        //                if (s.LogoutTime != null)
        //                {
        //                    var logout = s.LogoutTime.Value.ToLocalTime();
        //                    totalDayTime += (logout - login);
        //                }
        //                else
        //                {
        //                    totalDayTime += (DateTime.Now - login); // running
        //                }
        //            }

        //            dailyHours[day] = totalDayTime;
        //            totalMonth += totalDayTime;
        //        }

        //        result.Add(new DashboardViewModel
        //        {
        //            SerialNumber = count++,
        //            EmployeeName = emp.Name,
        //            GroupName = emp.Group?.GroupName,
        //            DailyHours = dailyHours,
        //            TotalWorkingHours = totalMonth,
        //            Sessions = empSessions,
        //            Status = isRunning ? "Running" : null,

        //            // ✅ ADD THESE (MUST)
        //            Month = safeMonth,
        //            Year = safeYear
        //        });
        //    }

        //    return View(result);
        //}

        //public IActionResult Dashboard(int? year, int? month, int? groupId, int? employeeId)
        //{
        //    year ??= DateTime.UtcNow.Year;
        //    month ??= DateTime.UtcNow.Month;

        //    ViewBag.Years = Enumerable.Range(DateTime.UtcNow.Year - 5, 6).Reverse().ToList();
        //    ViewBag.Months = Enumerable.Range(1, 12).ToList();
        //    ViewBag.Groups = _context.Groups.ToList();

        //    int daysInMonth = DateTime.DaysInMonth(year.Value, month.Value);

        //    var employees = _context.Employees.Include(e => e.Group).AsQueryable();
        //    if (groupId.HasValue) employees = employees.Where(e => e.GroupId == groupId);
        //    if (employeeId.HasValue) employees = employees.Where(e => e.Id == employeeId);

        //    var result = new List<DashboardViewModel>();

        //    foreach (var emp in employees.ToList())
        //    {
        //        var vm = new DashboardViewModel
        //        {
        //            EmployeeName = emp.Name,
        //            GroupName = emp.Group?.GroupName,
        //            //DailyHours = new List<TimeSpan>()
        //        };

        //        for (int day = 1; day <= daysInMonth; day++)
        //        {
        //            // create UTC start/end for the day
        //            var startUtc = new DateTime(year.Value, month.Value, day, 0, 0, 0, DateTimeKind.Utc);
        //            var endUtc = startUtc.AddDays(1);

        //            var sessions = _context.UserSessions
        //                .Where(x => x.EmployeeId == emp.Id && x.CreatedDate >= startUtc && x.CreatedDate < endUtc)
        //                .ToList();

        //            TimeSpan totalDayTime = TimeSpan.Zero;

        //            foreach (var s in sessions)
        //            {
        //                var login = s.LoginTime; // should be stored as UTC
        //                var logout = s.LogoutTime ?? DateTime.UtcNow;
        //                totalDayTime += (logout - login);
        //            }

        //            //vm.DailyHours.Add(totalDayTime);
        //        }

        //        //vm.TotalWorkingHours = vm.DailyHours.Aggregate(TimeSpan.Zero, (sum, val) => sum + val);
        //        result.Add(vm);
        //    }

        //    return View(result);
        //}
    }
}

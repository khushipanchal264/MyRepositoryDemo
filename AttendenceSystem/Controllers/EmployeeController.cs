using AttendenceSystem.Data;
using AttendenceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

public class EmployeeController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmployeeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult MonthlyReport(int? year, int? month)
    {
        int empId = HttpContext.Session.GetInt32("UserId") ?? 1;

        var reportList = new List<AttendanceReportVM>();

        year ??= DateTime.UtcNow.Year;
        month ??= DateTime.UtcNow.Month;

        int daysInMonth = DateTime.DaysInMonth(year.Value, month.Value);

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year.Value, month.Value, day);

            TimeSpan totalDayTime = TimeSpan.Zero;

            var daySessions = _context.UserSessions
                .Where(x => x.EmployeeId == empId &&
                            x.LoginTime.Date == date.Date)
                .ToList();

            foreach (var s in daySessions)
            {
                // ✅ ONLY COMPLETED SESSIONS
                if (s.LoginTime != null && s.LogoutTime != null)
                {
                    totalDayTime += (s.LogoutTime.Value - s.LoginTime);
                }
            }

            string totalHours =
                $"{(int)totalDayTime.TotalHours:D2}:{totalDayTime.Minutes:D2}";

            reportList.Add(new AttendanceReportVM
            {
                Date = date,
                TotalHours = totalHours
            });
        }

        return View(reportList);
    }
}
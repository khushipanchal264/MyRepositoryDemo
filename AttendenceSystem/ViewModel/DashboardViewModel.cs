using AttendenceSystem.Models;

namespace AttendenceSystem.ViewModel
{
    public class DashboardViewModel
    {
        public int SerialNumber { get; set; }
        public string EmployeeName { get; set; }
        public string GroupName { get; set; }

        // 🔥 Day-wise hours
        public Dictionary<int, TimeSpan> DailyHours { get; set; }

        public TimeSpan TotalWorkingHours { get; set; }

        public List<UserSession> Sessions { get; set; }
        public string Status { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}

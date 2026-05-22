using System.ComponentModel.DataAnnotations;

namespace AttendenceSystem.Models
{
    public class UserSession
    {
        [Key]
        public int Id { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public TimeSpan? TotalTime { get; set; } 
        public DateTime CreatedDate { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}

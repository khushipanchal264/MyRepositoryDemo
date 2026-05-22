using System.ComponentModel.DataAnnotations;

namespace AttendenceSystem.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
    }
}

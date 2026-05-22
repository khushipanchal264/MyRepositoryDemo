using System.ComponentModel.DataAnnotations;

namespace AttendenceSystem.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        public string GroupName { get; set; }

    }
}

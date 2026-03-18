namespace GymAppFresh.Models;

public class StaffTimeOff
{
        public int Id { get; set; }
        public int GymStaffId { get; set; }
        public GymStaff GymStaff { get; set; }

        public DateOnly Date { get; set; }           // DateTime for EF Core compatibility
        public TimeSpan? Start { get; set; }         // null ise tüm gün
        public TimeSpan? End { get; set; }           // null ise tüm gün
        public string? Reason { get; set; } 
}   
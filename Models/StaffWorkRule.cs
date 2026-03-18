namespace GymAppFresh.Models;

public class StaffWorkRule
{
    public int Id { get; set; }
    public int GymStaffId { get; set; }
    public GymStaff GymStaff { get; set; }

    public DayOfWeek DayOfWeek { get; set; } // System.DayOfWeek
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
    public bool IsActive { get; set; } = true;
}

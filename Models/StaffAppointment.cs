namespace GymAppFresh.Models;

public class StaffAppointment
{
    public int Id { get; set; }

    public int GymStaffId { get; set; }
    public GymStaff GymStaff { get; set; }

    public int MemberId { get; set; }
    public Member Member { get; set; }

    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Confirmed;
    public string? Notes { get; set; }
}

public enum AppointmentStatus
{
    Pending=0,
    Confirmed=1,
    Cancelled=2,
}
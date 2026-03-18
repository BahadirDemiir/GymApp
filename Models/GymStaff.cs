namespace GymAppFresh.Models
{
    public class GymStaff
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public bool IsActive { get; set; }

        public TimeSpan? ShiftStart { get; set; }
        public TimeSpan? ShiftEnd { get; set; }

        public int GymLocationId { get; set; }
        public GymLocation GymLocation { get; set; }    

        public int RoleId { get; set; }
        public Role Role { get; set; }
        
        public int? MemberId { get; set; }
        public Member? Member { get; set; }

        public ICollection<StaffWorkRule> StaffWorkRules { get; set; } = new List<StaffWorkRule>();
        public ICollection<StaffTimeOff> StaffTimeOffs { get; set; }= new List<StaffTimeOff>();
        public ICollection<StaffAppointment> StaffAppointments { get; set; }= new List<StaffAppointment>();
    
    }
}
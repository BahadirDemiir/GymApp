namespace GymAppFresh.Models;

public enum PacketType{
    Classic=1,
    Gold=2,
    Platinum=3
}

public class Membership
{
    public int MembershipId { get; set; }
    public string Name { get; set; } 
    public decimal Price { get; set; } 

    public PacketType PacketType { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }

    public bool? IsActive { get; set; } = true;
    
    public List<Member> Members { get; set; } = new List<Member>();
}

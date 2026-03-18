using System.ComponentModel.DataAnnotations;

namespace GymAppFresh.Models;

public class GymLocation
{
    public int Id { get; set; }
    public int currentMember {get; set;}
    [Required,MaxLength(100)]
    public string Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
    
    public TimeSpan WOpenTime { get; set; }

    public TimeSpan WCloseTime{ get; set; }
    public string? Images { get; set; }
    
    public bool IsActive { get; set; }

    
    public int CityId { get; set; }
    public City City { get; set; }
    public ICollection<GymImage> GymImages { get; set; } = new List<GymImage>();
    public ICollection<GymStaff> GymStaffs { get; set; }=new List<GymStaff>();

    public ICollection<Member> Members { get; set; } = new List<Member>();

}       

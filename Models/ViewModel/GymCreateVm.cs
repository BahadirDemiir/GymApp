using Microsoft.AspNetCore.Mvc.Rendering;


namespace GymAppFresh.Models.ViewModel {
public class GymCreateVm
{
    // Gym fields
    public string Name { get; set; } = null!;
    public int CurrentMember { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
    public bool IsActive { get; set; } = true;
    public TimeSpan WOpenTime { get; set; }
    public TimeSpan WCloseTime { get; set; }

    // City kısımları
    
    public int CityId { get; set; }
    public IEnumerable<SelectListItem?> Cities { get; set; }
    

    // Files
        public List<IFormFile>? Photos { get; set; }
}
}
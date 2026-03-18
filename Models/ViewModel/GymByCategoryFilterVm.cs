using Microsoft.AspNetCore.Mvc.Rendering;
using GymAppFresh.Models;

namespace GymAppFresh.Models.ViewModel
{
    public class GymByCategoryFilterVm
    {
        public List<GymLocation> Gyms { get; set; } = new();
        public List<City> Cities { get; set; } = new();
        public int? SelectedCityId { get; set; }
    }
}

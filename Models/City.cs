using System.ComponentModel.DataAnnotations;

namespace GymAppFresh.Models
{
    public class City
    {
        [Key]
        public int CityId { get; set; }
        [Required,MaxLength(100)]
        public required string Name { get; set; }

        public ICollection<GymLocation> Gyms { get; set; } = new List<GymLocation>();
    }
}

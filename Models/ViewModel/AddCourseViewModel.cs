using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymManagementSystem.Models.ViewModel {
    public class AddCourseViewModel {
        
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public double Duration { get; set; }
        public int CoachId { get; set; }
        public string Image { get; set; }

        public int CategoryId { get; set; }
        public string Level { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public int MaxStudents { get; set; }
        public int CurrentStudents { get; set; }
        public decimal Price { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }=Enumerable.Empty<SelectListItem>();
    }
}
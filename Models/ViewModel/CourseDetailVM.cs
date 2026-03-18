using GymAppFresh.Models;

namespace GymAppFresh.Models.ViewModel
{
    public class CourseDetailVM
    {
        public Course Course { get; set; }
        public Category Category { get; set; }
        
        public CourseDetailVM()
        {
            Course = new Course();
            Category = new Category();
        }
        
        public CourseDetailVM(Course course, Category category)
        {
            Course = course;
            Category = category;
        }
    }
}
using System.ComponentModel.DataAnnotations;

namespace GymAppFresh.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Course> Courses { get; set; }
        public List<Coach> Coaches { get; set; }


    }
}
using System.ComponentModel.DataAnnotations;

namespace GymAppFresh.Models
{
    public class Coach {
        [Required(ErrorMessage="Id numarasını giriniz")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Adınızı giriniz")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Resmi giriniz")]
        public string Image { get; set; }  
        [Required(ErrorMessage = "Biyografiyi giriniz")]
        public string Bio { get; set; }
        [Required(ErrorMessage = "Instagram URL'sini giriniz")]
        public string InstagramUrl { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public int StudentCount { get; set; }
        public int ExperienceYears { get; set; } 
        public string Level { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public List<Course> Courses { get; set; }
        
    }
}
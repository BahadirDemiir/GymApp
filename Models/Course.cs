using System.ComponentModel.DataAnnotations;

namespace GymAppFresh.Models {
    public class Course
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kurs adını giriniz")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama giriniz")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Başlangıç tarihini giriniz")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Süre giriniz")]
        public double Duration { get; set; }

        public int? CoachId { get; set; }
        public Coach? Coach { get; set; }

        [Required(ErrorMessage = "Resim URL'sini giriniz")]
        public string Image { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori seçiniz")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string? Level { get; set; }

        public decimal Rating { get; set; }

        public int ReviewCount { get; set; }

        public string? CoachName { get; set; }

        public int MaxStudents { get; set; }

        public int CurrentStudents { get; set; }

        public decimal Price { get; set; } 
    }   
}
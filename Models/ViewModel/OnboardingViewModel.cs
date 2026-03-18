using System.ComponentModel.DataAnnotations;
using GymAppFresh.Models;

namespace GymAppFresh.Models.ViewModel
{
    public class OnboardingViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Boy alanı zorunludur.")]
        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır.")]
        public decimal Height { get; set; }

        [Required(ErrorMessage = "Kilo alanı zorunludur.")]
        [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır.")]
        public decimal Weight { get; set; }

        [Required(ErrorMessage = "Cinsiyet seçimi zorunludur.")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Fitness hedefi seçimi zorunludur.")]
        public FitnessGoal FitnessGoal { get; set; }

        [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Haftada kaç gün egzersiz yapıyorsunuz? zorunludur.")]
        public daysPerWeek daysPerWeek { get; set; }
    }
}

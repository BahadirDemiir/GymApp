using System.ComponentModel.DataAnnotations;

namespace GymAppFresh.Models.ViewModel
{
    public class EditProfileViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Telefon alanı zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string Phone { get; set; }

        [Range(0, 500, ErrorMessage = "Kilo 0-500 kg arasında olmalıdır.")]
        public decimal? Weight { get; set; }

        [Required(ErrorMessage = "Spor salonu seçimi zorunludur.")]
        public int? GymLocationId { get; set; }
    }
}

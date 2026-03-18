using System.ComponentModel.DataAnnotations;

namespace GymAppFresh.Models {
    public enum Gender {
        Male=1,
        Female=2
    }

    public enum FitnessGoal {
        LoseWeight=1,
        GainWeight=2,
        MaintainWeight=3,
        ImproveHealth=4,
        GainMuscle=5,
        LoseFat=6
    }

    public enum daysPerWeek {
        OneDay=1,
        TwoDays=2,
        ThreeDays=3,
        FourDays=4,
        FiveDays=5,
        SixDays=6,
        SevenDays=7
    }

    public class Member
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string Password { get; set; }

        public bool IsAdmin { get; set; }
        public bool IsOnboardingCompleted { get; set; } = false;

        public Gender? Gender { get; set; }

        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public DateTime? BirthDate { get; set; }
        public FitnessGoal? FitnessGoal { get; set; }
        public daysPerWeek? daysPerWeek { get; set; }

        public Membership? Membership { get; set; }
        public int? MembershipId { get; set; }

        public GymLocation? GymLocation { get; set; }
        public int? GymLocationId { get; set; }
    }
}

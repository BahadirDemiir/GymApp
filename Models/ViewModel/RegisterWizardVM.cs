// ViewModels/RegisterWizardVM.cs
using System.ComponentModel.DataAnnotations;

namespace GymAppFresh.Models.ViewModel;

public class RegisterWizardVM
{
    // Step 1
    [Required(ErrorMessage="Telefon numarası gereklidir")]
    public string Phone { get; set; }

    // Step 2
    [Required(ErrorMessage="Bir salon seçmelisin")]
    public int? SelectedGymId { get; set; }
    public string SelectedGymName { get; set; }

    // Step 3
    [Required(ErrorMessage="Bir membership seçmelisin")]
    public int? SelectedMembershipId { get; set; }
    public string SelectedMembershipName { get; set; }
    public decimal? SelectedMembershipPrice { get; set; }

    // Step 4
    [Required, StringLength(60)]
    public string FirstName { get; set; }

    [Required, StringLength(60)]
    public string LastName { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    public string? Password { get; set; }

    // Step management (UI)
    public int CurrentStep { get; set; } = 1;
}

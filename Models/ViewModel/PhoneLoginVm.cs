using System.ComponentModel.DataAnnotations;

namespace GymAppFresh.Models.ViewModel {    
public class PhoneLoginVm { 
    [Required] public string Phone { get; set; }
    public string CountryCode { get; set; } = "+90";
    public string CountryName { get; set; } = "Türkiye";
    public bool KeepLoggedIn { get; set; }  
}
}
// Controllers/RegisterController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymAppFresh.Models;
using GymAppFresh.Models.ViewModel;
using GymAppFresh.Infrastructure;

namespace GymAppFresh.Controllers;

public class RegisterController : Controller
{
    private readonly DataContext context;
    private const string KEY = "reg_wizard";

     public RegisterController(DataContext context)
        {
            this.context = context;
        }

    // ===== Helpers =====
    private RegisterWizardVM GetVM()
        => HttpContext.Session.GetObject<RegisterWizardVM>(KEY) ?? new RegisterWizardVM();

    private void SaveVM(RegisterWizardVM vm)
        => HttpContext.Session.SetObject(KEY, vm);


    // ===== Step 1: Phone =====
    [HttpGet]
    public IActionResult Phone()
    {
        var vm = GetVM();
        vm.CurrentStep = Math.Max(vm.CurrentStep, 1);
        return View(vm);
    }

    [HttpPost]
    public IActionResult Phone(RegisterWizardVM input)
    {
        Console.WriteLine($"[DEBUG] Phone POST - Input: {input?.Phone}");
        
        // ModelState'i temizle ve sadece Phone field'ını validate et
        ModelState.Clear();
        
        if (string.IsNullOrWhiteSpace(input?.Phone))
        {
            ModelState.AddModelError("Phone", "Telefon numarası gereklidir");
            return View(input);
        }

        Console.WriteLine("[DEBUG] Phone validation passed, proceeding...");
        var vm = GetVM();
        vm.Phone = input.Phone;
        vm.CurrentStep = 2;
        SaveVM(vm);
        Console.WriteLine($"[DEBUG] Redirecting to Gym - Step: {vm.CurrentStep}, Phone: {vm.Phone}");
        return RedirectToAction("Gym");
    }

    // ===== Step 2: Gym selection =====
    [HttpGet]
    public async Task<IActionResult> Gym()
    {
        var vm = GetVM();             // önce VM'i çek
        Console.WriteLine($"[DBG] step={vm.CurrentStep}, phone={vm.Phone}");
        if (vm.CurrentStep < 2) return RedirectToAction("Phone"); // Guard yerine düz if: akış net görünsün

        ViewBag.Gyms = await context.GymLocations
            .Select(g => new { g.Id, g.Name, CityName = g.City.Name })
            .ToListAsync();

        return View(vm);
    }


    [HttpPost]
    public async Task<IActionResult> Gym(int? SelectedGymId)
    {
        var vm = GetVM();
        if (vm.CurrentStep < 2) return RedirectToAction("Phone");

        if (SelectedGymId == null)
        {
            ModelState.AddModelError("", "Bir salon seçmelisin");
            return await Gym();
        }

        var gym = await context.GymLocations.FindAsync(SelectedGymId);
        if (gym == null)
        {
            ModelState.AddModelError("", "Seçtiğin salon bulunamadı");
            return await Gym();
        }

        vm.SelectedGymId = gym.Id;
        vm.SelectedGymName = gym.Name;
        vm.CurrentStep = 3;
        SaveVM(vm);
        return RedirectToAction("Membership");
    }

    // ===== Step 3: Membership selection =====
    [HttpGet]
    public async Task<IActionResult> Membership()
    {
        ViewBag.Memberships = await context.Memberships
            .Select(m => new { m.MembershipId, m.Name, m.Price })
            .ToListAsync();

        var vm = GetVM();
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Membership(int? SelectedMembershipId)
    {
        var vm = GetVM();
        if (vm.CurrentStep < 3) return RedirectToAction("Phone");

        if (SelectedMembershipId == null)
        {
            ModelState.AddModelError("", "Bir membership seçmelisin");
            return await Membership();
        }

        var ms = await context.Memberships.FindAsync(SelectedMembershipId);
        if (ms == null)
        {
            ModelState.AddModelError("", "Seçtiğin membership bulunamadı");
            return await Membership();
        }

        vm.SelectedMembershipId = ms.MembershipId;
        vm.SelectedMembershipName = ms.Name;
        vm.SelectedMembershipPrice = ms.Price;
        vm.CurrentStep = 4;
        SaveVM(vm);
        return RedirectToAction("Details");
    }

    // ===== Step 4: Personal details =====
    [HttpGet]
    public IActionResult Details()
    {
        var vm = GetVM();
        if (vm.CurrentStep < 4) return RedirectToAction("Phone");
        return View(vm);
    }

    [HttpPost]
    public IActionResult Details(RegisterWizardVM input)
    {
        Console.WriteLine($"[DEBUG] Details POST - FirstName: {input?.FirstName}, LastName: {input?.LastName}, Email: {input?.Email}");
        
        var vm = GetVM();
        if (vm.CurrentStep < 4) return RedirectToAction("Phone");

        // ModelState'i temizle ve sadece bu step'teki field'ları validate et
        ModelState.Clear();
        
        if (string.IsNullOrWhiteSpace(input?.FirstName))
        {
            ModelState.AddModelError("FirstName", "Ad gereklidir");
        }
        if (string.IsNullOrWhiteSpace(input?.LastName))
        {
            ModelState.AddModelError("LastName", "Soyad gereklidir");
        }
        if (string.IsNullOrWhiteSpace(input?.Email))
        {
            ModelState.AddModelError("Email", "E-mail gereklidir");
        }
        else if (!input.Email.Contains("@"))
        {
            ModelState.AddModelError("Email", "Geçerli bir e-mail adresi giriniz");
        }
        if (string.IsNullOrWhiteSpace(input?.Password))
        {
            ModelState.AddModelError("Password", "Şifre gereklidir");
        }

        Console.WriteLine($"[DEBUG] ModelState.IsValid: {ModelState.IsValid}");
        if (!ModelState.IsValid)
        {
            Console.WriteLine("[DEBUG] ModelState is invalid:");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"[DEBUG] Validation Error: {error.ErrorMessage}");
            }
            return View(input);
        }

        Console.WriteLine("[DEBUG] Details validation passed, proceeding...");
        vm.FirstName = input.FirstName;
        vm.LastName  = input.LastName;
        vm.Email     = input.Email;
        vm.Password  = input.Password;
        vm.CurrentStep = 5;
        SaveVM(vm); 
        Console.WriteLine($"[DEBUG] Redirecting to Review - Step: {vm.CurrentStep}");
        return RedirectToAction("Review");
    }

    // ===== Step 5: Review & Confirm =====
    [HttpGet]
    public IActionResult Review()
    {
        var vm = GetVM();
        if (vm.CurrentStep < 5) return RedirectToAction("Phone");
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm()
    {
        var vm = GetVM();
        if (vm.CurrentStep < 5) return RedirectToAction("Phone");

        // Member kaydını oluştur
        var member = new Member
        {
            Name = $"{vm.FirstName} {vm.LastName}",
            Email = vm.Email,
            Phone = vm.Phone,   
            Password = vm.Password,
            // Seçilen gym ve membership bilgilerini kaydet
            GymLocationId = vm.SelectedGymId,
            MembershipId = vm.SelectedMembershipId
        };

        context.Members.Add(member);    
        await context.SaveChangesAsync();

        HttpContext.Session.Remove(KEY);

        TempData["reg_ok"] = "Kayıt başarıyla oluşturuldu 💃";
        return RedirectToAction("Success");
    }

    [HttpGet]
    public IActionResult Success() => View();
}

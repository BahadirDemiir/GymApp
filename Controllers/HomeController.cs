using Microsoft.AspNetCore.Mvc;
using GymAppFresh.Models;
using GymAppFresh.Models.ViewModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace GymAppFresh.Controllers;

public class HomeController : Controller
{
    private readonly PasswordHasher<Member> _hasher = new();
    private DataContext context;

    public HomeController(DataContext context)
    {
        this.context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(Member member)
    {
        if (!ModelState.IsValid)
        {
            return View(member);
        }

        if (context.Members.Any(m => m.Email == member.Email))
        {
            ModelState.AddModelError("Email", "Bu email adresi zaten kullanılıyor.");
            return View(member);
        }

        var hashed = _hasher.HashPassword(member, member.Password);
        member.Password = hashed;

        context.Members.Add(member);
        context.SaveChanges();

        TempData["SuccessMessage"] = "Kayıt başarılı! Şimdi giriş yapabilirsiniz.";
        return RedirectToAction(nameof(Login), new { returnUrl = "/Home/Index" });
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        var user = context.Members.FirstOrDefault(m => m.Email == email);
        if (user == null)
        {
            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View();
        }

        bool ok = false;
        bool looksHashed = !string.IsNullOrEmpty(user.Password) && user.Password.StartsWith("AQAAAA"); 

        if (looksHashed)
        {
            var verify = _hasher.VerifyHashedPassword(user, user.Password, password ?? "");
            ok = verify == PasswordVerificationResult.Success || verify == PasswordVerificationResult.SuccessRehashNeeded;
        }
        else
        {
            ok = user.Password == password;
            if (ok)
            {
                user.Password = _hasher.HashPassword(user, password ?? "");
                context.SaveChanges();
            }
        }

        if (!ok)
        {
            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View();
        }

        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Name ?? user.Email),
        new Claim(ClaimTypes.Email, user.Email),
    };
        if (user.IsAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        if (context.GymStaffs.Any(s => s.MemberId == user.Id))
        {
            claims.Add(new Claim("IsStaff", "true"));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        // Check if user needs to complete onboarding
        if (!user.IsOnboardingCompleted)
        {
            return RedirectToAction("Onboarding", "Home");
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index");
    }

    // Debug action - remove this in production
    [HttpGet]
    public IActionResult DebugUsers()
    {
        var users = context.Members.ToList();
        return View(users);
    }

    // Test database connection
    [HttpGet]
    public IActionResult TestDb()
    {
        try
        {
            var userCount = context.Members.Count();
            var allUsers = context.Members.ToList();

            ViewBag.UserCount = userCount;
            ViewBag.Users = allUsers;
            ViewBag.ConnectionStatus = "Connected";

            return View();
        }
        catch (Exception ex)
        {
            ViewBag.ConnectionStatus = "Error";
            ViewBag.ErrorMessage = ex.Message;
            return View();
        }
    }

    [Authorize]
    [HttpGet]
    public IActionResult Onboarding()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userEmail))
        {
            return RedirectToAction("Login");
        }

        var member = context.Members.FirstOrDefault(m => m.Email == userEmail);
        if (member == null || member.IsOnboardingCompleted)
        {
            return RedirectToAction("Index");
        }

        var viewModel = new OnboardingViewModel
        {
            Id = member.Id
        };

        return View(viewModel);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Onboarding(OnboardingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userEmail))
        {
            return RedirectToAction("Login");
        }

        var member = context.Members.FirstOrDefault(m => m.Email == userEmail);
        if (member == null)
        {
            return RedirectToAction("Login");
        }

        // Update member with onboarding data
        member.Height = model.Height;
        member.Weight = model.Weight;
        member.Gender = model.Gender;
        member.FitnessGoal = model.FitnessGoal;
        member.BirthDate = model.BirthDate;
        member.daysPerWeek = model.daysPerWeek;
        member.IsOnboardingCompleted = true;

        context.SaveChanges();

        TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla tamamlandı! Hoş geldiniz!";
        return RedirectToAction("Index");
    }


    }


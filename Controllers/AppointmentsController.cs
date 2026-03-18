using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using GymAppFresh.Models;
using System.Runtime.InteropServices;


namespace GymAppFresh.Controllers;

public class AppointmentsController : Controller
{
    private readonly DataContext _ctx;

    // Win/Linux uyumlu TZ
    private static readonly TimeZoneInfo _tz =
        GetTz();

    private static TimeZoneInfo GetTz()
    {
        try
        {
            // Windows
            return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        }
        catch
        {
            // Linux/macOS
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        }
    }

    public AppointmentsController(DataContext ctx) => _ctx = ctx;

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> My()
    {
        var memberIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(memberIdStr, out var memberId)) return Challenge();

        var nowUtc = DateTime.UtcNow;

        // 1) DB'den sadece TRANSLATE edilebilir alanları çek (UTC olarak)
        var raw = await _ctx.StaffAppointments
            .AsNoTracking()
            .Include(a => a.GymStaff).ThenInclude(s => s.GymLocation)
            .Where(a => a.MemberId == memberId && a.EndUtc >= nowUtc)
            .OrderBy(a => a.StartUtc)
            .Select(a => new
            {
                a.Id,
                a.StartUtc,
                a.EndUtc,
                StaffName = a.GymStaff.FullName,
                LocationName = a.GymStaff.GymLocation.Name
            })
            .ToListAsync();

        // 2) TimeZoneInfo'yu yerel değişkene al
        TimeZoneInfo tz;
        try
        {
            // Windows
            tz = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        }
        catch
        {
            // Linux/macOS
            tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        }

        // 3) Materialize EDİLMİŞ listede local time'a çevir
        var list = raw.Select(a => new MyApptVM
        {
            Id = a.Id,
            // EF çoğu zaman Kind=Unspecified döndürür; UTC olduğunu belirtiyoruz:
            StartLocal = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(a.StartUtc, DateTimeKind.Utc), tz),
            EndLocal = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(a.EndUtc, DateTimeKind.Utc), tz),
            StaffName = a.StaffName,
            LocationName = a.LocationName
        }).ToList();

        return View(list);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var memberIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(memberIdStr, out var memberId)) return Challenge();

        var appt = await _ctx.StaffAppointments
            .FirstOrDefaultAsync(a => a.Id == id && a.MemberId == memberId);

        if (appt == null) return NotFound();
        if (appt.StartUtc <= DateTime.UtcNow)
        {
            TempData["Error"] = "Başlamış/geçmiş randevu iptal edilemez.";
            return RedirectToAction(nameof(My));
        }

        _ctx.StaffAppointments.Remove(appt);
        await _ctx.SaveChangesAsync();

        TempData["Success"] = "Randevun iptal edildi.";
        return RedirectToAction(nameof(My));
    }
}

public class MyApptVM
{
    public int Id { get; set; }
    public DateTime StartLocal { get; set; }
    public DateTime EndLocal { get; set; }
    public string StaffName { get; set; }
    public string LocationName { get; set; }
    public string DayLabel => StartLocal.ToString("dddd, dd MMMM yyyy", new CultureInfo("tr-TR"));
    public string TimeRange => $"{StartLocal:HH\\:mm} – {EndLocal:HH\\:mm}";
}

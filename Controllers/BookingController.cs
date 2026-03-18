using GymAppFresh.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GymAppFresh.Controllers;

[Authorize]
public class BookingController : Controller
{
    private readonly DataContext _ctx;
    private readonly TimeZoneInfo _tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");

    public BookingController(DataContext ctx) => _ctx = ctx;

    public IActionResult Index(int staffId, DateTime date, int memberId)
    {
        return View((staffId, date, memberId));
    }

    // /Booking/Slots?staffId=5&date=2025-09-10&slotMinutes=30
    public IActionResult Slots(int staffId, DateOnly date, int slotMinutes = 30)
    {
        var staff = _ctx.GymStaffs
        .Include(s => s.StaffWorkRules)
        .Include(s => s.StaffTimeOffs)
        .FirstOrDefault(s => s.Id == staffId && s.IsActive);

        if (staff == null) return NotFound();

        var day = date.DayOfWeek;
        var rules = staff.StaffWorkRules.Where(r => r.DayOfWeek == day).ToList();

        var dayStartLocal = date.ToDateTime(TimeOnly.MinValue);
        var dayEndLocal = date.ToDateTime(new TimeOnly(23, 59, 59));

        var startUtc = TimeZoneInfo.ConvertTimeToUtc(dayStartLocal, _tz);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(dayEndLocal, _tz);

        var appts = _ctx.StaffAppointments
        .Where(a => a.GymStaffId == staffId &&
        a.StartUtc < endUtc &&
        a.EndUtc > startUtc)
        .ToList();

        var offs = staff.StaffTimeOffs.Where(o => o.Date == date).ToList();

        var slots = StaffAvailabilityService.BuildDailySlots(
            rules, offs, appts, date,
            TimeSpan.FromMinutes(slotMinutes), _tz);

        var vm = slots.Select(s => new
        {
            Start = s.start.ToString(@"hh\:mm"),
            End = s.end.ToString(@"hh\:mm")
        });

        return Json(vm);

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Book(int staffId, DateTime date, string startHHmm, int memberId, int slotMinutes = 30)
    {
        var tz = _tz;
        var startLocal = date.Date.Add(TimeOnly.ParseExact(startHHmm, "HH:mm").ToTimeSpan());
        var endLocal = startLocal.AddMinutes(slotMinutes);

        var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);

        // Çakışma kontrolü
        var hasOverlap = _ctx.StaffAppointments.Any(a =>
            a.GymStaffId == staffId &&
            a.Status != AppointmentStatus.Cancelled &&
            a.StartUtc < endUtc && a.EndUtc > startUtc);

        if (hasOverlap)
        {
            ModelState.AddModelError("", "Bu saat dilimi dolu görünüyor, başka bir slot dener misin?");
            return BadRequest(ModelState);
        }

        _ctx.StaffAppointments.Add(new StaffAppointment
        {
            GymStaffId = staffId,
            MemberId = memberId,
            StartUtc = startUtc,
            EndUtc = endUtc,
            Status = AppointmentStatus.Confirmed
        });
        _ctx.SaveChanges();

        return Ok(new { Message = "Randevun alındı ", Start = startLocal, End = endLocal });
    }
}

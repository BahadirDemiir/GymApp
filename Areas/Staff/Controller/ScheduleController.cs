using System.Security.Claims;
using GymAppFresh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymAppFresh.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Policy = "Staff")]
    public class ScheduleController : Controller
    {
        private readonly DataContext _ctx;
        public ScheduleController(DataContext ctx) => _ctx = ctx;

        private int CurrentMemberId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private GymStaff? LoadSelf()
        {
            return _ctx.GymStaffs
                .Include(s => s.StaffWorkRules)
                .Include(s => s.StaffTimeOffs)
                .FirstOrDefault(s => s.MemberId == CurrentMemberId && s.IsActive);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var staff = LoadSelf();
            if (staff == null) return Forbid();
            return View(ScheduleVM.FromEntity(staff));
        }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveWeekly(ScheduleVM vm)
    {
        var staff = LoadSelf();
        if (staff == null) return Forbid();

        // ---- Güvenli koleksiyon ----
        var days = (vm?.Days ?? new List<ScheduleVM.DayRow>())
                .Where(d => d != null) // binder’ın eklediği null’ları at
                .ToList();

        if (days.Count == 0)
        {
            ModelState.AddModelError("", "Form verisi alınamadı (Days boş).");
            var back0 = ScheduleVM.FromEntity(LoadSelf()!);
            return View("Index", back0);
        }


        // String değerleri boolean'a dönüştür
        foreach (var d in days)
        {
            // Radio button'dan gelen string değeri boolean'a çevir
            if (d.IsActive.ToString().ToLower() == "false")
            {
                d.IsActive = false;
            }
            else if (d.IsActive.ToString().ToLower() == "true")
            {
                d.IsActive = true;
            }
        }

        // Validasyon (sadece aktif satırlara)
        foreach (var d in days.Where(x => x.IsActive))
        {
            if (!TimeSpan.TryParse(d.StartHHmm, out var start) ||
                !TimeSpan.TryParse(d.EndHHmm, out var end) ||
                end <= start)
            {
                ModelState.AddModelError("", $"{d.Day}: Saatler geçersiz (bitiş > başlangıç olmalı).");
            }
        }

        if (!ModelState.IsValid)
        {
            var fresh = LoadSelf();
            var back = fresh != null ? ScheduleVM.FromEntity(fresh) : new ScheduleVM();
            back.Days = days; // Kullanıcının girdisi korunur
            return View("Index", back);
        }

        // Eski kuralları sil
        var old = _ctx.StaffWorkRules.Where(r => r.GymStaffId == staff.Id).ToList();
        _ctx.StaffWorkRules.RemoveRange(old);

        // Yeni kuralları hazırla
        var newRules = new List<StaffWorkRule>();
        foreach (var d in days.Where(x => x.IsActive))
        {
            var start = TimeSpan.Parse(d.StartHHmm);
            var end   = TimeSpan.Parse(d.EndHHmm);

            newRules.Add(new StaffWorkRule
            {
                GymStaffId = staff.Id,
                DayOfWeek  = d.Day,
                Start      = start,
                End        = end,
                IsActive   = true
            });
        }

        if (newRules.Count == 0)
        {
            TempData["ok"] = "Hiç aktif gün yok; değişiklik yapılmadı.";
            return RedirectToAction(nameof(Index));
        }

        _ctx.StaffWorkRules.AddRange(newRules);
        var affected = _ctx.SaveChanges(); // kaç satır yazıldı


        TempData["ok"] = $"{newRules.Count} gün kaydedildi (etkilenen satır: {affected}).";
        return RedirectToAction(nameof(Index));
    }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddTimeOff(AddTimeOffVM vm)
        {
            var staff = LoadSelf();
            if (staff == null) return Forbid();

            // Validate time range if both start and end are provided
            if (!string.IsNullOrWhiteSpace(vm.StartHHmm) && !string.IsNullOrWhiteSpace(vm.EndHHmm))
            {
                var start = TimeSpan.Parse(vm.StartHHmm);
                var end = TimeSpan.Parse(vm.EndHHmm);
                if (end <= start) 
                    ModelState.AddModelError("", "İzin bitiş saati başlangıç saatinden büyük olmalı.");
            }

            // Check if date is in the past
            if (vm.Date < DateOnly.FromDateTime(DateTime.Today))
                ModelState.AddModelError("", "Geçmiş tarihli izin talebi oluşturulamaz.");

            if (!ModelState.IsValid) return RedirectToAction(nameof(Index));

            _ctx.StaffTimeOffs.Add(new StaffTimeOff
            {
                GymStaffId = staff.Id,
                Date = vm.Date,
                Start = string.IsNullOrWhiteSpace(vm.StartHHmm) ? null : TimeSpan.Parse(vm.StartHHmm),
                End = string.IsNullOrWhiteSpace(vm.EndHHmm) ? null : TimeSpan.Parse(vm.EndHHmm),
                Reason = vm.Reason
            });
            _ctx.SaveChanges();
            TempData["ok"] = "İzin talebi başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteTimeOff(int id)
        {
            var staff = LoadSelf();
            if (staff == null) return Forbid();

            var timeOff = _ctx.StaffTimeOffs.FirstOrDefault(x => x.Id == id && x.GymStaffId == staff.Id);
            if (timeOff != null)
            {
                _ctx.StaffTimeOffs.Remove(timeOff);
                _ctx.SaveChanges();
                TempData["ok"] = "İzin talebi silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }

    // ====== View Models ======
    public class ScheduleVM
    {
        public List<DayRow> Days { get; set; } = new();
        public List<TimeOffRow> TimeOffs { get; set; } = new();

        public static ScheduleVM FromEntity(GymStaff s)
        {
            var vm = new ScheduleVM();
            
            // Initialize all days of the week in a specific order
            var daysOfWeek = new[] { 
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, 
                DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday 
            };
            
            foreach (DayOfWeek d in daysOfWeek)
            {
                var rule = s.StaffWorkRules.FirstOrDefault(x => x.DayOfWeek == d);
                vm.Days.Add(new DayRow
                {
                    Day = d,
                    IsActive = rule != null,
                    StartHHmm = rule?.Start.ToString(@"hh\:mm") ?? "09:00",
                    EndHHmm = rule?.End.ToString(@"hh\:mm") ?? "17:00"
                });
            }

            // Load time-off requests
            vm.TimeOffs = s.StaffTimeOffs
                .OrderByDescending(t => t.Date)
                .Select(t => new TimeOffRow
                {
                    Id = t.Id,
                    Date = t.Date,
                    Start = t.Start?.ToString(@"hh\:mm"),
                    End = t.End?.ToString(@"hh\:mm"),
                    Reason = t.Reason
                }).ToList();

            return vm;
        }

        public class DayRow
        {
            public DayOfWeek Day { get; set; }
            public bool IsActive { get; set; }
            public string StartHHmm { get; set; } = "09:00";
            public string EndHHmm { get; set; } = "17:00";
        }
    }

    public class AddTimeOffVM
    {
        public DateOnly Date { get; set; }
        public string? StartHHmm { get; set; } // If empty, it's a full day off
        public string? EndHHmm { get; set; }
        public string? Reason { get; set; }
    }


    public class TimeOffRow
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public string? Start { get; set; }
        public string? End { get; set; }
        public string? Reason { get; set; }
    }
}

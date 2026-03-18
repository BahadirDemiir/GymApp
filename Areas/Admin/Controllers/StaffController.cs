using GymAppFresh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GymAppFresh.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StaffController : Controller
    {
        private readonly DataContext _ctx;
        public StaffController(DataContext ctx) => _ctx = ctx;

        // LIST: Üyeleri ve varsa Staff durumunu göster
        [HttpGet]
        public IActionResult List()
        {
            var members = _ctx.Members
                .Select(m => new MemberRow{
                    MemberId = m.Id,
                    Name = m.Name,
                    Email = m.Email,
                    IsStaff = _ctx.GymStaffs.Any(s => s.MemberId == m.Id)
                })
                .OrderByDescending(x => x.IsStaff)
                .ThenBy(x => x.Name)
                .ToList();

            return View(members);
        }

        // GET: Promote formu (rol ve salon seçtir)
        [HttpGet]
        public IActionResult Promote(int memberId)
        {
            var m = _ctx.Members.FirstOrDefault(x => x.Id == memberId);
            if (m == null) return NotFound();

            // Check if member is already staff
            var existingStaff = _ctx.GymStaffs.FirstOrDefault(s => s.MemberId == memberId);

            var vm = new PromoteToStaffVM{
                MemberId = memberId,
                MemberName = m.Name ?? m.Email,
                RoleId = existingStaff?.RoleId ?? 0,
                GymLocationId = existingStaff?.GymLocationId ?? 0,
                Roles = _ctx.Roles.Select(r => new Option{ Id = r.Id, Name = r.Name }).ToList(),
                Gyms  = _ctx.GymLocations.Select(g => new Option{ Id = g.Id, Name = g.Name }).ToList()
            };
            return View(vm);
        }

        // POST: Promote → GymStaff kaydı oluştur/güncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Promote(PromoteToStaffVM vm)
        {
            // Repopulate dropdowns if validation fails
            if (!ModelState.IsValid)
            {
                vm.Roles = _ctx.Roles.Select(r => new Option{ Id = r.Id, Name = r.Name }).ToList();
                vm.Gyms = _ctx.GymLocations.Select(g => new Option{ Id = g.Id, Name = g.Name }).ToList();
                return View(vm);
            }

            var member = _ctx.Members.FirstOrDefault(m => m.Id == vm.MemberId);
            if (member == null) 
            {
                TempData["error"] = "Member bulunamadı.";
                return RedirectToAction(nameof(List));
            }

            try
            {
                var staff = _ctx.GymStaffs.FirstOrDefault(s => s.MemberId == vm.MemberId);
                if (staff == null)
                {
                    staff = new GymStaff{
                        FullName = member.Name ?? member.Email,
                        Email = member.Email,
                        Phone = member.Phone,
                        IsActive = true,
                        MemberId = member.Id,
                        RoleId = vm.RoleId,
                        GymLocationId = vm.GymLocationId
                    };
                    _ctx.GymStaffs.Add(staff);
                    TempData["ok"] = "Kullanıcı staff olarak eklendi.";
                }
                else
                {
                    staff.IsActive = true;
                    staff.RoleId = vm.RoleId;
                    staff.GymLocationId = vm.GymLocationId;
                    _ctx.GymStaffs.Update(staff);
                    TempData["ok"] = "Kullanıcı staff bilgileri güncellendi.";
                }

                _ctx.SaveChanges();
                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(List));
            }
        }

        // (Opsiyonel) Staff pasifleştir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deactivate(int memberId)
        {
            var staff = _ctx.GymStaffs.FirstOrDefault(s => s.MemberId == memberId);
            if (staff != null)
            {
                staff.IsActive = false;
                _ctx.SaveChanges();
                TempData["ok"] = "Staff pasifleştirildi.";
            }
            return RedirectToAction(nameof(List));
        }
    }

    // ====== ViewModels ======
    public class MemberRow
    {
        public int MemberId { get; set; }
        public string? Name { get; set; }
        public string Email { get; set; }
        public bool IsStaff { get; set; }
    }

    public class PromoteToStaffVM
    {
        public int MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rol seçimi zorunludur")]
        public int RoleId { get; set; }
        
        [Required(ErrorMessage = "Salon seçimi zorunludur")]
        public int GymLocationId { get; set; }

        public List<Option> Roles { get; set; } = new();
        public List<Option> Gyms  { get; set; } = new();
    }

    public class Option { public int Id { get; set; } public string Name { get; set; } }
}

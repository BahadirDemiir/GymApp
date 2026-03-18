using GymAppFresh.Models;
using GymAppFresh.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace GymAppFresh.Controllers {

    [Authorize]
    [Route("/Profile")]
    public class ProfileController : Controller
    {
        private DataContext context;

        public ProfileController(DataContext context)
        {
            this.context = context;
        }

        [HttpGet("Profile")]
        public IActionResult Profile()
        {
            var memberEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(memberEmail))
                return NotFound();

            var enrollments = context.Enrollment
                .Include(e => e.Member)
                .Include(e => e.Course)
                .Where(e => e.Member.Email == memberEmail)
                .OrderByDescending(e => e.CourseId)
                .ToList();

            return View(enrollments);
        }

        [HttpGet("EditProfile")]
        public async Task<IActionResult> EditProfile(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await context.Members.FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
            {
                return NotFound();
            }

            var viewModel = new EditProfileViewModel
            {
                Id = member.Id,
                Phone = member.Phone,
                Weight = member.Weight,
                GymLocationId = member.GymLocationId
            };

            // Get gym locations for dropdown
            ViewBag.GymLocations = context.GymLocations
                .Where(g => g.IsActive)
                .Select(g => new { Value = g.Id, Text = g.Name })
                .ToList();



            return View(viewModel);
        }

        [HttpGet("BookAppointment")]
        public IActionResult BookAppointment()
        {
            var memberEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(memberEmail))
                return NotFound();

            var member = context.Members.FirstOrDefault(m => m.Email == memberEmail);
            if (member == null)
                return NotFound();
            

            var ptStaffs = member.GymLocationId.HasValue 
                ? context.GymStaffs
                    .Include(s => s.GymLocation)
                    .Where(s => s.RoleId == 2 && s.IsActive && s.GymLocationId == member.GymLocationId)
                    .ToList()
                : new List<GymStaff>(); // Empty list if no gym location
            
            // If member has no gym location, show message
            if (!member.GymLocationId.HasValue)
            {
                TempData["ErrorMessage"] = "Önce bir spor salonu seçmeniz gerekiyor. Lütfen profil bilgilerinizi güncelleyin.";
            }

            ViewBag.MemberId = member.Id;
            ViewBag.MemberGymLocationId = member.GymLocationId;
            
            return View(ptStaffs);
        }

        [HttpPost("EditProfile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(int id, EditProfileViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingMember = await context.Members.FirstOrDefaultAsync(m => m.Id == id);
                    if (existingMember == null)
                    {
                        return NotFound();
                    }

                    existingMember.Phone = model.Phone;
                    existingMember.Weight = model.Weight;
                    existingMember.GymLocationId = model.GymLocationId;

                    context.Update(existingMember);
                    await context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
                    return RedirectToAction("Profile");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!context.Members.Any(m => m.Id == model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Güncelleme sırasında bir hata oluştu: " + ex.Message;
                    return View(model);
                }
            }
            
            return View(model);
        }
        }    }

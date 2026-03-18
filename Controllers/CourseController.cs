using Microsoft.AspNetCore.Mvc;
using GymAppFresh.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GymAppFresh.Controllers;

public class CourseController : Controller
{
    private DataContext context;

    public CourseController(DataContext context)
    {
        this.context = context;
    }

    public IActionResult Courses(string? filter)
    {
        ViewData["FilterOptions"] = new[] { "Tümü" }
                .Concat(context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => c.Name)
                .ToList())
                .ToArray();

        ViewData["CurrentFilter"] = string.IsNullOrWhiteSpace(filter) ? "all" : filter;

        var query = context.Courses.Include(c => c.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter) && !string.Equals(filter, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(c => c.Category != null && c.Category.Name == filter);
        }

        var courses = query.ToList();
        return View(courses);
    }

    [HttpGet]
    public IActionResult CourseDetails(int id)
    {
        var course = context.Courses.FirstOrDefault(c => c.Id == id);
        if (course == null)
        {
            return NotFound();
        }
        return View(course);
    }
    
    [Authorize]
    public IActionResult CourseDetail(int id)
    {
        var course = context.Courses.Include(c => c.Category).FirstOrDefault(c => c.Id == id);
        if (course == null)
        {
            return NotFound();
        }

        // Get current user's ID from authentication claims
        var memberId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        ViewBag.MemberId = memberId;

        // Check if user is already enrolled
        bool isEnrolled = false;
        if (!string.IsNullOrEmpty(memberId) && int.TryParse(memberId, out int memberIdInt))
        {
            isEnrolled = context.Enrollment.Any(e => e.MemberId == memberIdInt && e.CourseId == id);
        }
        ViewBag.IsEnrolled = isEnrolled;

        // Get actual current enrollment count
        var actualCurrentStudents = context.Enrollment.Count(e => e.CourseId == id && e.Status == "Approved");
        course.CurrentStudents = actualCurrentStudents;

        var vm = new GymAppFresh.Models.ViewModel.CourseDetailVM
        {
            Course = course,
            Category = course.Category
        };
        return View(vm);
    }
    
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll([FromForm] int courseId)
        {
            // Get current user's ID from authentication claims
            var memberIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out int memberId))
                return Json(new { ok = false, message = "Kullanıcı bilgisi bulunamadı. Lütfen tekrar giriş yapın." });

            var course = await context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course is null)
                return Json(new { ok = false, message = $"Kurs bulunamadı (id={courseId})." });

            var member = await context.Members.FirstOrDefaultAsync(m => m.Id == memberId);
            if (member is null)
                return Json(new { ok = false, message = $"Üye bulunamadı (id={memberId})." });

            // Zaten kayıtlı mı? (any status)
            var already = await context.Enrollment
                .AnyAsync(e => e.MemberId == member.Id && e.CourseId == course.Id);
            if (already)
                return Json(new { ok = false, message = "Bu kursa zaten kayıtlısın 💅" });

            // Kapasite kontrolü (double-check to prevent race conditions)
            var current = await context.Enrollment
                .CountAsync(e => e.CourseId == course.Id && e.Status == "Approved");
            if (course.MaxStudents > 0 && current >= course.MaxStudents)
                return Json(new { ok = false, message = "Kontenjan dolu 🥺" });

            context.Enrollment.Add(new Enrollment
            {
                MemberId = member.Id,
                CourseId = course.Id,
                Status = "Approved"
            });

            // Update course current students count
            course.CurrentStudents = current + 1;
            await context.SaveChangesAsync();

            return Json(new { ok = true, message = "Kaydın alındı! 🎉" });
        }

        // Helper method to get current enrollment count for a course
        private async Task<int> GetCurrentEnrollmentCount(int courseId)
        {
            return await context.Enrollment
                .CountAsync(e => e.CourseId == courseId && e.Status == "Approved");
        }
    }


    

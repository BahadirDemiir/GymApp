using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GymAppFresh.Models;
using GymAppFresh.Models.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GymAppFresh.Areas.Admin.Controllers {
    [Area("Admin")]

public class CourseController : Controller
{
    private DataContext context;

    public CourseController(DataContext context)
    {
        this.context = context;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult AddCourse()
    {
        try
        {
            ViewBag.Categories = context.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            ViewBag.Coaches = context.Coaches.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            return View(new Course { StartDate = DateTime.Today });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddCourse GET: {ex.Message}");
            return View("Error");
        }
    }


    [Authorize(Roles = "Admin")]
    [HttpPost]
    public IActionResult AddCourse(Course course, IFormFile photo)
    {
        if (photo != null)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "courses", photo.FileName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                photo.CopyTo(stream);
            }
            course.Image = "/images/courses/" + photo.FileName;
        }
        context.Courses.Add(course);
        context.SaveChanges();
        return RedirectToAction("Courses");
    }
}
}           
    

    
    
    
    
    
    
    
    
    

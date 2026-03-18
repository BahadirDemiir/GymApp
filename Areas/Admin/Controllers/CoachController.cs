using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GymAppFresh.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GymAppFresh.Areas.Admin.Controllers {
[Area("Admin")]

public class CoachController : Controller
{
        private DataContext context;
        
        public CoachController(DataContext context)
        {
            this.context = context;
        }
        

    [Authorize(Roles="Admin")]
    [HttpGet]
    public IActionResult AddCoach() {
        ViewBag.Categories=context.Categories.Select(c=>new SelectListItem{
            Value=c.Id.ToString(),
            Text=c.Name
        }).ToList();
        return View();
    }

    [Authorize(Roles="Admin")]
    [HttpPost]
    public IActionResult AddCoach(Coach coach, IFormFile photo) {
        if (photo != null) {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "coachs", photo.FileName);
            using (var stream = new FileStream(path, FileMode.Create)) {
                photo.CopyTo(stream);
            }
            coach.Image = "/images/coachs/" + photo.FileName;
        }
        context.Coaches.Add(coach); 
        context.SaveChanges();
        return RedirectToAction("Coachs");
    }
}
}
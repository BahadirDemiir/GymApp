using Microsoft.AspNetCore.Mvc;
using GymAppFresh.Models;
using Microsoft.EntityFrameworkCore;

namespace GymAppFresh.Controllers;

public class CoachController : Controller
{ 
        private DataContext context;
        

        
        public CoachController(DataContext context)
    {
        this.context = context;
    }
        
        public IActionResult Coachs(string? filter) {
            ViewData["FilterOptions"] = new[] { "Tümü" }
                    .Concat(context.Categories
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .Select(c => c.Name)
                    .ToList())
                    .ToArray();  

            ViewData["CurrentFilter"]=string.IsNullOrWhiteSpace(filter)? "all" : filter;

            var query=context.Coaches.Include(c=>c.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter) && !string.Equals(filter, "all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(c => c.Category != null && c.Category.Name == filter);
            }
            var coachs = query.ToList();
            return View(coachs);
        }
}

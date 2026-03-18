using Microsoft.AspNetCore.Mvc;
using GymAppFresh.Models;
using Microsoft.EntityFrameworkCore;

namespace GymAppFresh.Controllers {
    public class GymController : Controller
    {

        private readonly DataContext context;

        public GymController(DataContext context)
        {
            this.context = context;
        }


        public IActionResult gymList()
        {
            var gyms = context.GymLocations.Include(g => g.City).OrderBy(g => g.Name).ToList();
            var cities = context.Cities.OrderBy(c => c.Name).ToList();
            ViewBag.Cities = cities;
            return View(gyms);
        }

        public IActionResult GymDetails(int id)
        {
            var details = context.GymLocations.Include(g => g.GymStaffs).FirstOrDefault(g => g.Id == id);
            if (details == null)
            {
                return NotFound();
            }
            return View(details);
        }

        public IActionResult GymRegister(int id)
        {

            return View();
        }

        public IActionResult Map()
        {
            return View();
        }

    }
}
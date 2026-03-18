using Microsoft.AspNetCore.Mvc;
using GymAppFresh.Models;

namespace GymAppFresh.Controllers
{
    [ApiController]
    [Route("api/gyms")]
    
    public class GymsApiController : ControllerBase
    {
        private readonly DataContext _context; 
        public GymsApiController(DataContext context)
        {
            _context = context;
        }

        
        [HttpGet]
        public IActionResult GetGyms([FromQuery] bool activeOnly = true, [FromQuery] int? cityId = null)
        {
            var query = _context.GymLocations.AsQueryable();

            if (activeOnly)
                query = query.Where(g => g.IsActive);

            if (cityId.HasValue)
                query = query.Where(g => g.CityId == cityId.Value);

            var result = query
                .Select(g => new
                {
                    g.Id,
                    g.Name,
                    g.Latitude,
                    g.Longitude,
                    g.IsActive,
                    g.OpenTime,
                    g.CloseTime,
                    g.currentMember,
                    CityName = g.City.Name
                })
                .ToList();

            return Ok(result);
        }

        [HttpGet("cities")]
        public IActionResult GetCities()
        {
            var cities = _context.Cities
                .OrderBy(c => c.Name)
                .Select(c => new { c.CityId, c.Name })
                .ToList();

            return Ok(cities);
        }

        
        [HttpPost("{id}/toggle")]
        public IActionResult Toggle(int id)
        {
            var gym = _context.GymLocations.Find(id);
            if (gym == null)
                return NotFound();

            gym.IsActive = !gym.IsActive;
            _context.SaveChanges();

            return Ok(new { gym.Id, gym.IsActive });
        }
    }
}

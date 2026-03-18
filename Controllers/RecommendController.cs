using GymAppFresh.Services;
using GymAppFresh.Models;   
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GymAppFresh.Controllers
{
    [Authorize]
    [Route("/Recommend")]
public class RecommendController : Controller
{
    private readonly DataContext _db;
    private readonly RecommenderService _rec;

    public RecommendController(DataContext db)
    {
        _db = db;
        _rec = new RecommenderService(db);
    }

    [HttpGet]   
    public async Task<IActionResult> Index()
    {
        // Get current user's email from claims
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userEmail))
        {
            return RedirectToAction("Login", "Home");
        }

        // Find member by email
        var member = await _db.Members.FirstOrDefaultAsync(x => x.Email == userEmail);
        if (member is null) 
        {
            return NotFound("Member not found. Please make sure you are logged in.");
        }

        var suggestions = await _rec.SuggestForMemberAsync(member);
        return View(suggestions);
    }
    }   
}

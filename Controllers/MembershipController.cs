using Microsoft.AspNetCore.Mvc;
using GymAppFresh.Models;
using System.Security.Claims;

namespace GymAppFresh.Controllers;

public class MembershipController : Controller
{
    private DataContext context;

    public MembershipController(DataContext context)
    {
        this.context = context;
    }

    public IActionResult Membership()
    {
        var membership = context.Memberships.ToList();
        return View(membership);
    }

    public IActionResult Buy(int membershipId,string? promoCode)
    {
        var userIdStr=User.FindFirst(ClaimTypes.NameIdentifier);
        if(string.IsNullOrEmpty(userIdStr?.Value))
        {
            return RedirectToAction("Login","Home");
        }
        
        var plan=context.Memberships.FirstOrDefault(x=>(int)x.PacketType==membershipId);
        if(plan==null)
        {
            return NotFound();
        }
        
        var member = context.Members.FirstOrDefault(x => x.Id == int.Parse(userIdStr.Value));
        if(member == null)
        {
            return NotFound();
        }
        
        member.MembershipId = (int)plan.PacketType;
        member.Membership = plan;

        context.SaveChanges();
        return RedirectToAction("Membership","Membership");
    }
}

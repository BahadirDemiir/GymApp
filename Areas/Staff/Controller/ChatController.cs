using System.Security.Claims;
using GymAppFresh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymAppFresh.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Policy = "Staff")]
    public class ChatController : Controller
    {
        private readonly DataContext _ctx;
        public ChatController(DataContext ctx) => _ctx = ctx;

        private int CurrentMemberId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);


        [HttpGet]
        public async Task<IActionResult> Inbox()
        {
            var staff = await _ctx.GymStaffs
                .FirstOrDefaultAsync(s => s.MemberId == CurrentMemberId && s.IsActive);
            if (staff == null) return Forbid();

            var items = await _ctx.ChatThreads
                .AsNoTracking()
                .Where(t => t.GymStaffId == staff.Id)
                .OrderByDescending(t => t.UpdatedAt) // ✅ sıralamayı projection'dan önce yap
                .Select(t => new ThreadListItemVM(
                    t.Id,                                  // ThreadId
                    t.MemberId,                            // MemberId
                    _ctx.Members.Where(m => m.Id == t.MemberId)
                            .Select(m => m.Name)
                            .FirstOrDefault()
                        ?? ("Üye #" + t.MemberId.ToString()), // ✅ tip garantisi
                    _ctx.ChatMessages.Where(m => m.ThreadId == t.Id)
                                    .OrderByDescending(m => m.Id)
                                    .Select(m => m.Body)
                                    .FirstOrDefault(),        // LastMessage
                    t.UpdatedAt,                               // LastAt
                    _ctx.ChatMessages.Where(m => m.ThreadId == t.Id
                                            && m.SenderType == ChatSenderType.Member
                                            && !m.IsReadByStaff)
                                    .Count()                  // UnreadForStaff
                ))
                .ToListAsync();

            return View(items); // @model List<ThreadListItemVM>
        }

        // /Staff/Chat/Thread/5  (id = threadId)
        [HttpGet]
        public async Task<IActionResult> Thread(int id)
        {
            var staff = await _ctx.GymStaffs.FirstOrDefaultAsync(s => s.MemberId == CurrentMemberId && s.IsActive);
            if (staff == null) return Forbid();

            var thread = await _ctx.ChatThreads.FirstOrDefaultAsync(t => t.Id == id && t.GymStaffId == staff.Id);
            if (thread == null) return NotFound();

            // Mesajları al
            var messages = await _ctx.ChatMessages
                .Where(m => m.ThreadId == id)
                .OrderBy(m => m.Id)
                .ToListAsync();

            // Okundu işaretle (member’dan gelenleri staff okudu)
            var unread = await _ctx.ChatMessages
                .Where(m => m.ThreadId == id && m.SenderType == ChatSenderType.Member && !m.IsReadByStaff)
                .ToListAsync();
            if (unread.Count > 0)
            {
                foreach (var m in unread) m.IsReadByStaff = true;
                await _ctx.SaveChangesAsync();
            }

            ViewBag.ThreadId = id;
            return View("Thread", messages); // Areas/Staff/Views/Chat/Thread.cshtml
        }

    }
}


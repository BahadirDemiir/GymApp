using System.Security.Claims;
using GymAppFresh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymAppFresh.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly DataContext _ctx;
    public ChatController(DataContext ctx) => _ctx = ctx;

    private int CurrentMemberId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsStaffUser => User.HasClaim("Policy", "Staff") || User.HasClaim("IsStaff", "true");

    // Üye tarafı: PT profiline "Mesaj at" tıklayınca
    [HttpGet]
    public async Task<IActionResult> Thread(int staffId)
    {
        // Eğer bu sayfayı sadece member açacaksa:
        if (IsStaffUser) return Forbid();

        var memberId = CurrentMemberId;

        // staff'ı çek (Include sadece navigasyon içindir, Id için gerek yok)
        var staff = await _ctx.GymStaffs
            .FirstOrDefaultAsync(s => s.Id == staffId && s.IsActive);
        if (staff == null) return NotFound();

        var member = await _ctx.Members
            .FirstOrDefaultAsync(m => m.Id == memberId);
        if (member == null) return NotFound();

        // null guard (ikisi de nullable olabilir sende)
        if (member.GymLocationId == null || staff.GymLocationId == 0)
            return Forbid();

        // ✅ Aynı salon kuralı
        if (member.GymLocationId != staff.GymLocationId) return Forbid();

        // ✅ Tekil thread garantisi (GymLocationId ile)
        var thread = await _ctx.ChatThreads.FirstOrDefaultAsync(t =>
            t.GymLocationId == staff.GymLocationId &&
            t.MemberId == memberId &&
            t.GymStaffId == staff.Id);

        if (thread == null)
        {
            thread = new ChatThread
            {
                GymLocationId = staff.GymLocationId,
                MemberId = memberId,
                GymStaffId = staff.Id
            };
            _ctx.ChatThreads.Add(thread);
            await _ctx.SaveChangesAsync();
        }

        var messages = await _ctx.ChatMessages
            .Where(m => m.ThreadId == thread.Id)
            .OrderBy(m => m.Id)
            .Take(200)
            .ToListAsync();

        var unread = await _ctx.ChatMessages
                .Where(m => m.ThreadId == thread.Id && m.SenderType == ChatSenderType.Staff && !m.IsReadByMember)
                .ToListAsync();
            if (unread.Count > 0)
            {
                foreach (var m in unread) m.IsReadByMember = true;
                await _ctx.SaveChangesAsync();
            }

        ViewBag.ThreadId = thread.Id;
        ViewBag.Staff = staff;
        return View("Thread", messages);
    }


    // Mesaj geçmişi (sonsuz scroll için)
    [HttpGet]
    public async Task<IActionResult> Messages(int threadId, int? beforeId)
    {
        // yetki
        var allowed = await _ctx.ChatThreads.AnyAsync(t =>
            t.Id == threadId &&
           ((IsStaffUser && _ctx.GymStaffs.Any(s => s.MemberId == CurrentMemberId && s.IsActive && s.Id == t.GymStaffId))
             || (!IsStaffUser && t.MemberId == CurrentMemberId)));

        if (!allowed) return Forbid();

        var q = _ctx.ChatMessages.Where(m => m.ThreadId == threadId);
        if (beforeId.HasValue) q = q.Where(m => m.Id < beforeId.Value);

        var list = await q.OrderByDescending(m => m.Id).Take(50).ToListAsync();
        list.Reverse();
        return Json(list.Select(m => new
        {
            id = m.Id,
            body = m.Body,
            sender = m.SenderType.ToString(),
            createdAt = m.CreatedAt
        }));
    }

    [HttpGet]
    public async Task<IActionResult> StaffList()
    {
        var me = await _ctx.Members.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == CurrentMemberId);
        if (me == null) return Unauthorized();

        if (me.GymLocationId == null || me.GymLocationId == 0)
        {
            TempData["Error"] = "Önce bir salona kayıtlı olmalısın 💡";
            return View(new List<StaffListItemVM>());
        }

        // SQL tarafında: Staff + (opsiyonel) Member.Name + GymLocation.Name
        var rows = await (
            from s  in _ctx.GymStaffs.AsNoTracking()
            join m  in _ctx.Members.AsNoTracking()      on s.MemberId      equals m.Id into gj
            from m  in gj.DefaultIfEmpty()
            join gl in _ctx.GymLocations.AsNoTracking() on s.GymLocationId equals gl.Id
            where s.IsActive && s.GymLocationId == me.GymLocationId
            orderby m.Name
            select new {
                StaffId         = s.Id,
                StaffName       = m.Name,           // Member.Name varsa
                GymLocationId   = gl.Id,
                GymLocationName = gl.Name
            }
        ).ToListAsync();

        var items = rows
            .Select(x => new StaffListItemVM(
                x.StaffId,
                string.IsNullOrWhiteSpace(x.StaffName) ? $"PT #{x.StaffId}" : x.StaffName,
                x.GymLocationId,
                x.GymLocationName
            ))
            .ToList();

        return View(items); // @model List<StaffListItemVM>
    }

        
    public async Task<IActionResult> Inbox()
    {
        var meId = CurrentMemberId;

        var items = await _ctx.ChatThreads
            .AsNoTracking()
            .Where(t => t.MemberId == meId)
            .OrderByDescending(t => t.UpdatedAt)     // sıralamayı önce yap
            .Select(t => new MemberThreadListItemVM(
                t.Id,                                  // ThreadId
                t.GymStaffId,                          // StaffId
                // StaffName: GymStaff -> Member.Name
                _ctx.Members
                    .Where(m => m.Id == _ctx.GymStaffs
                        .Where(s => s.Id == t.GymStaffId)
                        .Select(s => s.MemberId)
                        .FirstOrDefault())
                    .Select(m => m.Name)
                    .FirstOrDefault() ?? ("PT #" + t.GymStaffId),
                // LastMessage
                _ctx.ChatMessages
                    .Where(m => m.ThreadId == t.Id)
                    .OrderByDescending(m => m.Id)
                    .Select(m => m.Body)
                    .FirstOrDefault(),
                t.UpdatedAt,                           // LastAt
                // Unread sadece member için (staff’tan gelen ve okunmamış)
                _ctx.ChatMessages
                    .Where(m => m.ThreadId == t.Id
                             && m.SenderType == ChatSenderType.Staff
                             && !m.IsReadByMember)
                    .Count()
            ))
            .ToListAsync();

        return View(items); // Views/Chat/Inbox.cshtml  (member)
    }
}

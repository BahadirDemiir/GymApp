using System.Security.Claims;
using GymAppFresh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GymAppFresh.Hubs;

[Authorize] // hem member hem staff
public class ChatHub : Hub
{
    private readonly DataContext _ctx;
    public ChatHub(DataContext ctx) => _ctx = ctx;

    private int CurrentMemberId => int.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsStaffUser   => Context.User.HasClaim("Policy", "Staff") 
                                  || Context.User.HasClaim("IsStaff","true"); 

    private async Task<bool> CanAccessThreadAsync(int threadId)
    {
        var t = await _ctx.ChatThreads.AsNoTracking().FirstOrDefaultAsync(x => x.Id == threadId);
        if (t == null) return false;

        if (IsStaffUser)
        {
            var staff = await _ctx.GymStaffs.FirstOrDefaultAsync(s => s.MemberId == CurrentMemberId && s.IsActive);
            return staff != null && staff.Id == t.GymStaffId;
        }
        else
        {
            return t.MemberId == CurrentMemberId;
        }
    }

    public async Task JoinThread(int threadId)
    {
        if (!await CanAccessThreadAsync(threadId)) throw new HubException("forbidden");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"thread-{threadId}");
    }

    public async Task SendMessage(int threadId, string text, string clientGuid = null)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        if (!await CanAccessThreadAsync(threadId)) throw new HubException("forbidden");

        var thread = await _ctx.ChatThreads.FirstAsync(t => t.Id == threadId);
        var msg = new ChatMessage
        {
            ThreadId = threadId,
            Body = text.Trim(),
            SenderType = IsStaffUser ? ChatSenderType.Staff : ChatSenderType.Member,
            ClientGuid = clientGuid
        };

        thread.UpdatedAt = DateTime.UtcNow;
        _ctx.ChatMessages.Add(msg);
        await _ctx.SaveChangesAsync();

        // minimal DTO
        var payload = new {
            id = msg.Id,
            body = msg.Body,
            sender = msg.SenderType.ToString(),
            createdAt = msg.CreatedAt
        };

        await Clients.Group($"thread-{threadId}").SendAsync("ReceiveMessage", payload);
    }
}

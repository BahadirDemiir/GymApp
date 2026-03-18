using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymAppFresh.Models;

public enum ChatSenderType { Member = 1, Staff = 2 }

public class ChatThread
{
    public int Id { get; set; }

    public int GymLocationId { get; set; }

    public int MemberId { get; set; }      
    public int GymStaffId { get; set; }    

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

public class ChatMessage
{
    public int Id { get; set; }
    public int ThreadId { get; set; }
    public ChatThread Thread { get; set; }

    [MaxLength(2000)]
    public string Body { get; set; }

    public ChatSenderType SenderType { get; set; }

    [MaxLength(64)]
    public string ClientGuid { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsReadByMember { get; set; }
    public bool IsReadByStaff  { get; set; }
}

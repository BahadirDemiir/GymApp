public record ThreadListItemVM(
    int ThreadId,
    int MemberId,
    string MemberName,
    string? LastMessage,
    DateTime LastAt,
    int UnreadForStaff
);

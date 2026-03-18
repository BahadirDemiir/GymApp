public record StaffListItemVM(
    int StaffId,
    string StaffName,
    int GymLocationId,
    string GymLocationName
);

public record MemberThreadListItemVM(
    int ThreadId,
    int StaffId,
    string StaffName,
    string? LastMessage,
    DateTime LastAt,
    int UnreadForMember
);
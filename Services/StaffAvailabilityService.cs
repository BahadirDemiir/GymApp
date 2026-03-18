using GymAppFresh.Models;

public static class StaffAvailabilityService
{
    public static List<(TimeSpan start, TimeSpan end)> BuildDailySlots(
        IEnumerable<StaffWorkRule> rulesForDay,
        IEnumerable<StaffTimeOff> timeOffsForDay,
        IEnumerable<StaffAppointment> apptsForDayUtc,
        DateOnly dateLocal,
        TimeSpan slotLength,
        TimeZoneInfo tz)  
    {
        var blocks = rulesForDay
            .Where(r => r.IsActive && r.End > r.Start)
            .Select(r => (r.Start, r.End))  
            .ToList();

        foreach (var off in timeOffsForDay)
        {
            var offStart = off.Start ?? TimeSpan.Zero;
            var offEnd   = off.End   ?? TimeSpan.FromHours(24);
            blocks = Subtract(blocks, (offStart, offEnd));
        }

        var dayStartLocal = dateLocal;
        foreach (var appt in apptsForDayUtc)
        {
            if (appt.Status == AppointmentStatus.Cancelled) continue;

            var apptLocalStart = TimeZoneInfo.ConvertTimeFromUtc(appt.StartUtc, tz);
            var apptLocalEnd   = TimeZoneInfo.ConvertTimeFromUtc(appt.EndUtc, tz);
            if (DateOnly.FromDateTime(apptLocalStart.Date) != dayStartLocal && DateOnly.FromDateTime(apptLocalEnd.Date) != dayStartLocal)
                continue;

            var s = apptLocalStart.TimeOfDay;
            var e = apptLocalEnd.TimeOfDay;
            blocks = Subtract(blocks, (s, e));
        }

        var slots = new List<(TimeSpan, TimeSpan)>();
        foreach (var (s, e) in blocks)
        {
            var cur = s;
            while (cur + slotLength <= e)
            {
                slots.Add((cur, cur + slotLength));
                cur += slotLength;
            }
        }
        return slots;

        static List<(TimeSpan, TimeSpan)> Subtract(List<(TimeSpan, TimeSpan)> src, (TimeSpan S, TimeSpan E) cut)
        {
            var res = new List<(TimeSpan, TimeSpan)>();
            foreach (var (a, b) in src)
            {
                if (cut.E <= a || cut.S >= b)
                {
                    res.Add((a, b)); 
                }
                else
                {
                    if (cut.S > a) res.Add((a, cut.S));
                    if (cut.E < b) res.Add((cut.E, b));
                }
            }
            return res;
        }
    }
}

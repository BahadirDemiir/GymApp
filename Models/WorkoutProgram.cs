public class WorkoutProgram {
    public int Id { get; set; }
    public string Code { get; set; }
    public string Title { get; set; }
    public int Goal { get; set; }
    public int DaysPerWeek { get; set; }
    public string Level { get; set; }
    public string Split { get; set; }
    public string Equipment { get; set; }
    public string? Description { get; set; }
    public string? PlanJson { get; set; }
}
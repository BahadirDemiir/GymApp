using GymAppFresh.Models;

namespace GymAppFresh.Models.ViewModel {
    public class ProgramSuggestionVm {
        public WorkoutProgram Program { get; set; }
        public int Score { get; set; }
        public string? Why { get; set; } ="";
    }
}

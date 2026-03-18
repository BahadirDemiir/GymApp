namespace GymAppFresh.Models {

    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<GymStaff> GymStaffs { get; set; } = new List<GymStaff>();
    }
 }

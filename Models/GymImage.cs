namespace GymAppFresh.Models {
    public class GymImage {
        public int Id { get; set; }
        public int GymLocationId { get; set; }
        public string ImagePath { get; set; }
        public GymLocation GymLocation { get; set; }
    }   

}
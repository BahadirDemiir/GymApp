// Models/Enrollment.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace GymAppFresh.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        [Required]
        public int MemberId { get; set; }

        [Required]
        public int CourseId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Opsiyonel: Pending, Approved, Canceled...
        [StringLength(20)]
        public string Status { get; set; } = "Approved";

        // Navs
        public Member Member { get; set; }
        public Course Course { get; set; }
    }
}

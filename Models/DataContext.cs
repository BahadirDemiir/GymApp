using Microsoft.EntityFrameworkCore;


namespace GymAppFresh.Models
{
    
    
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Coach> Coaches { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<GymLocation> GymLocations { get; set; }
        public DbSet<GymImage> GymImages { get; set; }
        public DbSet<GymStaff> GymStaffs { get; set; }
        public DbSet<Enrollment> Enrollment { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<WorkoutProgram> WorkoutPrograms { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<StaffWorkRule> StaffWorkRules { get; set; }
        public DbSet<StaffTimeOff> StaffTimeOffs { get; set; }
        public DbSet<StaffAppointment> StaffAppointments { get; set; }
        public DbSet<ChatThread> ChatThreads { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatThread>(b =>
            {
                b.HasIndex(t => new { t.GymLocationId, t.MemberId, t.GymStaffId }).IsUnique();
                b.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                b.Property(t => t.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<ChatMessage>(b =>
            {
                b.HasOne(m => m.Thread)
                .WithMany(t => t.Messages)
                .HasForeignKey(m => m.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(m => new { m.ThreadId, m.CreatedAt });
            });
            modelBuilder.Entity<StaffTimeOff>()
                .Property(p => p.Date)
                .HasConversion(
                    v => v.ToDateTime(TimeOnly.MinValue),
                    v => DateOnly.FromDateTime(v)
                )
                .HasColumnName("date");
            // City
            modelBuilder.Entity<City>(e =>
            {
                e.ToTable("Cities"); // tablo adı net
                e.HasKey(x => x.CityId);
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);
                // City ↔ GymLocation ilişki zaten GymLocation tarafında tanımlı
            });

            // Course -> Category
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Category)
                .WithMany(cat => cat.Courses)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Coach -> Category
            modelBuilder.Entity<Coach>()
                .HasOne(c => c.Category)
                .WithMany(cat => cat.Coaches)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course -> Coach
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Coach)
                .WithMany(co => co.Courses)
                .HasForeignKey(c => c.CoachId)
                .OnDelete(DeleteBehavior.Restrict);

            // GymImage
            modelBuilder.Entity<GymImage>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ImagePath).IsRequired().HasMaxLength(500);
                e.HasOne(x => x.GymLocation)
                    .WithMany(g => g.GymImages)
                    .HasForeignKey(x => x.GymLocationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // GymStaff
            modelBuilder.Entity<GymStaff>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.FullName).IsRequired().HasMaxLength(100);
                e.Property(x => x.RoleId).IsRequired().HasMaxLength(50);
                e.HasOne(x => x.GymLocation)
                    .WithMany(g => g.GymStaffs)
                    .HasForeignKey(x => x.GymLocationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // GymLocation -> City
            modelBuilder.Entity<GymLocation>()
                .HasOne(g => g.City)
                .WithMany(c => c.Gyms)
                .HasForeignKey(g => g.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enrollment (unique MemberId,CourseId)
            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.MemberId, e.CourseId })
                .IsUnique();

            modelBuilder.Entity<GymStaff>()
                .HasOne(g => g.Role)
                .WithMany(r => r.GymStaffs)
                .HasForeignKey(g => g.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Member)
                .WithMany()
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Member>()
                .HasOne(m => m.Membership)
                .WithMany(m => m.Members)
                .HasForeignKey(m => m.MembershipId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Member>()
                .HasOne(m => m.GymLocation)
                .WithMany(m => m.Members)
                .HasForeignKey(m => m.GymLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<GymStaff>()
            .HasOne(s => s.Member)
            .WithMany()
            .HasForeignKey(s => s.MemberId)
            .OnDelete(DeleteBehavior.SetNull);


            modelBuilder.Entity<StaffWorkRule>()
            .HasIndex(x => new { x.GymStaffId, x.DayOfWeek, x.Start, x.End })
            .IsUnique(); // aynı gün aynı aralık iki kez yazılmasın

            modelBuilder.Entity<StaffWorkRule>()
                .HasOne(x => x.GymStaff)
                .WithMany(s => s.StaffWorkRules)
                .HasForeignKey(x => x.GymStaffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StaffTimeOff>()
                .HasOne(x => x.GymStaff)
                .WithMany(s => s.StaffTimeOffs)
                .HasForeignKey(x => x.GymStaffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StaffAppointment>()
                .HasOne(a => a.GymStaff)
                .WithMany(s => s.StaffAppointments)
                .HasForeignKey(a => a.GymStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StaffAppointment>()
                .HasOne(a => a.Member)
                .WithMany()
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<WorkoutProgram>().HasData(
            new WorkoutProgram
            {
                Id = 1,
                Code = "FATLOSS_BEG_3D",
                Title = "Yağ Yakım – Başlangıç (3 gün)",
                Goal = 1,
                Level = "Beginner",
                DaysPerWeek = 3,
                Split = "FullBody",
                Equipment = "Bodyweight,Dumbbell",
                Description = "Full body + LISS (30-40dk)."
            },
            new WorkoutProgram
            {
                Id = 2,
                Code = "HYPER_UL_4D",
                Title = "Kas Geliştirme – Upper/Lower (4 gün)",
                Goal = 5,
                Level = "Beginner",
                DaysPerWeek = 4,
                Split = "UpperLower",
                Equipment = "Dumbbell,Barbell,Machines",
                Description = "Progressive overload odaklı."
            },
            new WorkoutProgram
            {
                Id = 3,
                Code = "ENDUR_FB_3D",
                Title = "Kondisyon – Full Body (3 gün)",
                Goal = 6,
                Level = "Beginner",
                DaysPerWeek = 3,
                Split = "FullBody",
                Equipment = "Bodyweight",
                Description = "Düşük-orta yoğunluk, nefes ve tempo."
            }
            );

            base.OnModelCreating(modelBuilder);
        }

    }

    }

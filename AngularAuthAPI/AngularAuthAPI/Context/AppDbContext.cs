using AngularAuthAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AngularAuthAPI.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<StudentResponse> StudentResponses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Template>().ToTable("templates");

            var stringArrayComparer = new ValueComparer<string[]>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToArray());

            modelBuilder.Entity<Template>()
                .Property(t => t.Specializations)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Metadata.SetValueComparer(stringArrayComparer);

            modelBuilder.Entity<Template>()
                .Property(t => t.Fields)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Metadata.SetValueComparer(stringArrayComparer);

            
            modelBuilder.Entity<StudentResponse>().ToTable("student_responses");
            modelBuilder.Entity<StudentResponse>()
                .Property(s => s.Responses)
                .HasColumnType("jsonb");

            modelBuilder.Entity<StudentResponse>()
                .HasOne(s => s.Template)
                .WithMany()
                .HasForeignKey(s => s.TemplateID);

            modelBuilder.Entity<StudentResponse>()
                .HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentID);
        }
    }
}

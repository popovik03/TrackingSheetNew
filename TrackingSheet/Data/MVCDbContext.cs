using Microsoft.EntityFrameworkCore;
using TrackingSheet.Models.Domain;
using TrackingSheet.Models.RO_Planer;

namespace TrackingSheet.Data
{
    public class MVCDbContext : DbContext
    {
        public MVCDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Incidents> IncidentList { get; set; }
        public DbSet<EmployeePlaner2024> EmployeePlaner2024 { get; set; }

        public DbSet<ROemployees> ROemployees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация для ROemployees
            modelBuilder.Entity<ROemployees>()
                .HasMany(r => r.PlanerEntries)
                .WithOne(e => e.ROemployees)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade); // Укажите поведение при удалении, если нужно

            // Начальные данные для сотрудников
            modelBuilder.Entity<ROemployees>().HasData(
            // Ваши начальные данные
            );
        }

    }
}

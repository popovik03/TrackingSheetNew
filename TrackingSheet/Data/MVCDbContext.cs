using Microsoft.EntityFrameworkCore;
using TrackingSheet.Models.Domain;

namespace TrackingSheet.Data
{
    public class MVCDbContext : DbContext
    {
        public MVCDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Incidents> IncidentList { get; set; }
    }
}

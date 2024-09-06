using Microsoft.EntityFrameworkCore;
using TrackingSheet.Models.Domain;
using TrackingSheet.Models.Kanban;
using TrackingSheet.Models.RO_Planer;

namespace TrackingSheet.Data
{
    public class MVCDbContext : DbContext
    {
        public MVCDbContext(DbContextOptions options) : base(options)
        {

        }

        //Основная база с инцидентами
        public DbSet<Incidents> IncidentList { get; set; }


        //Таблицы для канбан доски
        public DbSet<KanbanTask> KanbanTasks { get; set; }
        public DbSet<KanbanSubtask> KanbanSubtasks { get; set; }
        public DbSet<KanbanComment> KanbanComments { get; set; }
        public DbSet<KanbanMember> KanbanMembers { get; set; }
        public DbSet<KanbanTaskMember> KanbanTaskMembers { get; set; }


        //......................Рабочий планер пока на паузе..............................................
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



            // Настройка связи многие-ко-многим между KanbanTask и KanbanMember через KanbanTaskMember
            modelBuilder.Entity<KanbanTaskMember>()
                .HasKey(tm => new { tm.KanbanTaskId, tm.KanbanMemberId });

            modelBuilder.Entity<KanbanTaskMember>()
                .HasOne(tm => tm.KanbanTask)
                .WithMany(t => t.TaskMembers)
                .HasForeignKey(tm => tm.KanbanTaskId);

            modelBuilder.Entity<KanbanTaskMember>()
                .HasOne(tm => tm.KanbanMember)
                .WithMany(m => m.TaskMembers)
                .HasForeignKey(tm => tm.KanbanMemberId);

            // Настройка связи один-ко-многим между KanbanTask и KanbanSubtask
            modelBuilder.Entity<KanbanSubtask>()
                .HasOne(s => s.KanbanTask)
                .WithMany(t => t.Subtasks)
                .HasForeignKey(s => s.KanbanTaskId);

            // Настройка связи один-ко-многим между KanbanTask и KanbanComment
            modelBuilder.Entity<KanbanComment>()
                .HasOne(c => c.KanbanTask)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.KanbanTaskId);
       
        }
    }
}

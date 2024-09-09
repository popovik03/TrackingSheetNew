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

        // Основная база с инцидентами
        public DbSet<Incidents> IncidentList { get; set; }

        // Таблицы для канбан доски
        public DbSet<KanbanBoard> KanbanBoards { get; set; }
        public DbSet<KanbanColumn> KanbanColumns { get; set; }
        public DbSet<KanbanTask> KanbanTasks { get; set; }
        public DbSet<KanbanSubtask> KanbanSubtasks { get; set; }
        public DbSet<KanbanComment> KanbanComments { get; set; }
        public DbSet<KanbanMember> KanbanMembers { get; set; }
        public DbSet<KanbanTaskMember> KanbanTaskMembers { get; set; }

        // Рабочий планер пока на паузе
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

            // Настройка связи один-ко-многим между KanbanBoard и KanbanColumn
            modelBuilder.Entity<KanbanColumn>()
                .HasOne(c => c.KanbanBoard)
                .WithMany(b => b.Columns)
                .HasForeignKey(c => c.KanbanBoardId)
                .OnDelete(DeleteBehavior.Restrict); // Пример

            // Настройка связи один-ко-многим между KanbanColumn и KanbanTask
            modelBuilder.Entity<KanbanTask>()
                .HasOne<KanbanColumn>()
                .WithMany(c => c.Tasks)
                .HasForeignKey(t => t.KanbanColumnId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении колонки удаляются все её задачи

            // Настройка связи один-ко-многим между KanbanTask и KanbanSubtask
            modelBuilder.Entity<KanbanSubtask>()
                .HasOne(s => s.KanbanTask)
                .WithMany(t => t.Subtasks)
                .HasForeignKey(s => s.KanbanTaskId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении задачи удаляются все её подзадачи

            // Настройка связи один-ко-многим между KanbanTask и KanbanComment
            modelBuilder.Entity<KanbanComment>()
                .HasOne(c => c.KanbanTask)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.KanbanTaskId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении задачи удаляются все её комментарии

            // Настройка связи многие-ко-многим между KanbanTask и KanbanMember через KanbanTaskMember
            modelBuilder.Entity<KanbanTaskMember>()
                .HasKey(tm => new { tm.KanbanTaskId, tm.KanbanMemberId });

            modelBuilder.Entity<KanbanTaskMember>()
                .HasOne(tm => tm.KanbanTask)
                .WithMany(t => t.TaskMembers)
                .HasForeignKey(tm => tm.KanbanTaskId)
                .OnDelete(DeleteBehavior.Cascade); // Удаление задачи также удаляет связи с участниками

            modelBuilder.Entity<KanbanTaskMember>()
                .HasOne(tm => tm.KanbanMember)
                .WithMany(m => m.TaskMembers)
                .HasForeignKey(tm => tm.KanbanMemberId)
                .OnDelete(DeleteBehavior.Cascade); // Удаление участника также удаляет связи с задачами

            // Создание начального KanbanBoard
            modelBuilder.Entity<KanbanBoard>().HasData(
            new KanbanBoard
                {
                     Id = Guid.NewGuid(),
                     Board = "Главная",
                     CreatedAt = DateTime.UtcNow,
                     IsProtected = true // Указываем, что доска защищена
                }
            );
        }

        // Переопределение SaveChanges для защиты KanbanBoard
        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries<KanbanBoard>())
            {
                if (entry.State == EntityState.Deleted && entry.Entity.IsProtected)
                {
                    throw new InvalidOperationException("Невозможно удалить защищенную Kanban доску.");
                }
            }

            return base.SaveChanges();
        }

    }
}

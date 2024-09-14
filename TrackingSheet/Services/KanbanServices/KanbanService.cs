using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrackingSheet.Data;
using TrackingSheet.Models.Kanban;

namespace TrackingSheet.Services
{
    public class KanbanService : IKanbanService
    {
        private readonly MVCDbContext _context;

        public KanbanService(MVCDbContext context)
        {
            _context = context;
        }

        #region Методы для работы с досками

        public async Task<List<KanbanBoard>> GetAllBoardsAsync()
        {
            return await _context.KanbanBoards
                .Include(b => b.Columns)
                    .ThenInclude(c => c.Tasks)
                        .ThenInclude(t => t.Subtasks)
                .Include(b => b.Columns)
                    .ThenInclude(c => c.Tasks)
                        .ThenInclude(t => t.Comments)
                .OrderBy(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<KanbanBoard> GetBoardByIdAsync(Guid id)
        {
            return await _context.KanbanBoards
                .Include(b => b.Columns.OrderBy(c => c.Order)) // Сортировка колонок по Order
                    .ThenInclude(c => c.Tasks)
                        .ThenInclude(t => t.Subtasks)
                .Include(b => b.Columns)
                    .ThenInclude(c => c.Tasks)
                        .ThenInclude(t => t.Comments)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<KanbanBoard> CreateBoardAsync(KanbanBoard board)
        {
            _context.KanbanBoards.Add(board);
            await _context.SaveChangesAsync();
            return board;
        }

        public async Task UpdateBoardAsync(KanbanBoard board)
        {
            _context.KanbanBoards.Update(board);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBoardAsync(Guid id)
        {
            var board = await _context.KanbanBoards.FindAsync(id);
            if (board != null)
            {
                _context.KanbanBoards.Remove(board);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<KanbanBoard> GetBoardByColumnIdAsync(Guid columnId)
        {
            var column = await _context.KanbanColumns
                .Include(c => c.KanbanBoard)
                .ThenInclude(b => b.Columns.OrderBy(c => c.Order))  // Сортировка колонок по Order
                .FirstOrDefaultAsync(c => c.Id == columnId);

            return column?.KanbanBoard;
        }

        #endregion

        #region Методы для работы с колонками

        public async Task<KanbanColumn> GetColumnByIdAsync(Guid id)
        {
            return await _context.KanbanColumns
                .Include(c => c.Tasks)
                    .ThenInclude(t => t.Subtasks)
                .Include(c => c.Tasks)
                    .ThenInclude(t => t.Comments)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddColumnToBoardAsync(Guid boardId, KanbanColumn column)
        {
            var board = await _context.KanbanBoards
                .Include(b => b.Columns)
                .FirstOrDefaultAsync(b => b.Id == boardId);

            if (board != null)
            {
                var maxOrder = board.Columns.Any() ? board.Columns.Max(c => c.Order) : 0;
                column.Order = maxOrder + 1;

                board.Columns.Add(column);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddColumnAsync(KanbanColumn column)
        {
            // Определяем максимальный Order среди существующих колонок для данной доски
            var maxOrder = await _context.KanbanColumns
                .Where(c => c.KanbanBoardId == column.KanbanBoardId)
                .Select(c => (int?)c.Order)
                .MaxAsync() ?? 0;

            column.Order = maxOrder + 1;

            // Добавляем колонку непосредственно в контекст
            _context.KanbanColumns.Add(column);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateColumnAsync(KanbanColumn column)
        {
            _context.KanbanColumns.Update(column);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteColumnAsync(Guid id)
        {
            var column = await _context.KanbanColumns.FindAsync(id);
            if (column != null)
            {
                _context.KanbanColumns.Remove(column);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RenameColumnAsync(Guid columnId, string newName)
        {
            var column = await _context.KanbanColumns.FindAsync(columnId);
            if (column != null)
            {
                column.Column = newName;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateColumnOrderAsync(List<ColumnOrderUpdateModel> updatedOrder)
        {
            foreach (var columnUpdate in updatedOrder)
            {
                var column = await _context.KanbanColumns.FindAsync(columnUpdate.Id);
                if (column != null)
                {
                    column.Order = columnUpdate.Order;
                }
            }

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Методы для работы с задачами

        public async Task<KanbanTask> GetTaskByIdAsync(Guid id)
        {
            return await _context.KanbanTasks
                .Include(t => t.Subtasks)
                .Include(t => t.Comments)
                .Include(t => t.TaskMembers)
                    .ThenInclude(tm => tm.KanbanMember)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddTaskToColumnAsync(Guid columnId, KanbanTask task)
        {
            // Определяем максимальный Order среди задач в колонке
            var maxOrder = await _context.KanbanTasks
                .Where(t => t.KanbanColumnId == columnId)
                .Select(t => (int?)t.Order)
                .MaxAsync() ?? 0;

            task.Order = maxOrder + 1;
            task.KanbanColumnId = columnId;

            // Добавляем задачу непосредственно в контекст
            _context.KanbanTasks.Add(task);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTaskAsync(KanbanTask task)
        {
            // Проверяем, отслеживается ли сущность контекстом
            var existingTask = await _context.KanbanTasks.FindAsync(task.Id);
            if (existingTask != null)
            {
                // Обновляем свойства существующей задачи
                _context.Entry(existingTask).CurrentValues.SetValues(task);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Если задача не найдена, можно выбросить исключение или добавить новую
                throw new Exception("Задача не найдена.");
            }
        }

        public async Task EditTaskAsync(Guid taskId, string taskName, string taskDescription, string taskColor, DateTime? dueDate, string priority)
        {
            var task = await _context.KanbanTasks.FindAsync(taskId);
            if (task != null)
            {
                task.TaskName = taskName;
                task.TaskDescription = taskDescription;
                task.TaskColor = taskColor;
                task.DueDate = dueDate;
                task.Priority = priority;

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteTaskAsync(Guid taskId)
        {
            var task = await _context.KanbanTasks.FindAsync(taskId);
            if (task != null)
            {
                _context.KanbanTasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        public async Task MoveTaskAsync(Guid taskId, Guid oldColumnId, Guid newColumnId, int newIndex)
        {
            var task = await _context.KanbanTasks.FindAsync(taskId);

            if (task != null)
            {
                // Если колонка задачи изменилась
                if (task.KanbanColumnId != newColumnId)
                {
                    // Удаляем задачу из старой колонки и обновляем порядок задач в старой колонке
                    var oldColumnTasks = await _context.KanbanTasks
                        .Where(t => t.KanbanColumnId == oldColumnId)
                        .OrderBy(t => t.Order)
                        .ToListAsync();

                    oldColumnTasks.Remove(task);
                    for (int i = 0; i < oldColumnTasks.Count; i++)
                    {
                        oldColumnTasks[i].Order = i;
                    }

                    // Обновляем колонку задачи
                    task.KanbanColumnId = newColumnId;
                }

                // Обновляем порядок задач в новой колонке
                var newColumnTasks = await _context.KanbanTasks
                    .Where(t => t.KanbanColumnId == newColumnId)
                    .OrderBy(t => t.Order)
                    .ToListAsync();

                newColumnTasks.Insert(newIndex, task);

                for (int i = 0; i < newColumnTasks.Count; i++)
                {
                    newColumnTasks[i].Order = i;
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<Guid> GetBoardIdByColumnIdAsync(Guid columnId)
        {
            var column = await _context.KanbanColumns
                .FirstOrDefaultAsync(c => c.Id == columnId);

            return column?.KanbanBoardId ?? Guid.Empty;
        }

        #endregion

        // Модель для обновления порядка колонок
        public class ColumnOrderUpdateModel
        {
            public Guid Id { get; set; }
            public int Order { get; set; }
        }
    }
}

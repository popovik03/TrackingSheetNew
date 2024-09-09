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

        // Методы для работы с досками
        #region Board Methods

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
                .Include(b => b.Columns)
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
                .FirstOrDefaultAsync(c => c.Id == columnId);

            return column?.KanbanBoard;
        }

        #endregion

        // Методы для работы с колонками
        #region Column Methods

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
            var board = await GetBoardByIdAsync(boardId);
            if (board != null)
            {
                column.KanbanBoardId = boardId;
                _context.KanbanColumns.Add(column);
                await _context.SaveChangesAsync();
            }
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


        // Методы для работы с задачами
        #region Task Methods

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
            var column = await GetColumnByIdAsync(columnId);
            if (column != null)
            {
                task.KanbanColumnId = columnId;
                _context.KanbanTasks.Add(task);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateTaskAsync(KanbanTask task)
        {
            _context.KanbanTasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTaskAsync(Guid id)
        {
            var task = await _context.KanbanTasks.FindAsync(id);
            if (task != null)
            {
                _context.KanbanTasks.Remove(task);
                await _context.SaveChangesAsync();
            }
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

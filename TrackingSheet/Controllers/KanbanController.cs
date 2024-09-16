using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TrackingSheet.Services;
using TrackingSheet.Models.Kanban;
using static TrackingSheet.Services.KanbanService;

namespace TrackingSheet.Controllers
{
    public class KanbanController : Controller
    {
        private readonly IKanbanService _kanbanService;

        public KanbanController(IKanbanService kanbanService)
        {
            _kanbanService = kanbanService;
        }

        // Основной метод для отображения доски Kanban
        public async Task<IActionResult> KanbanView(Guid? selectedBoardId = null)
        {
            var boards = await _kanbanService.GetAllBoardsAsync();
            var selectedBoard = selectedBoardId.HasValue
                ? boards.FirstOrDefault(b => b.Id == selectedBoardId.Value)
                : boards.FirstOrDefault();

            ViewBag.SelectedBoardId = selectedBoard?.Id;
            ViewData["CurrentPage"] = "kanban";
            return View(boards);
        }

        #region Методы для работы с досками

        [HttpPost]
        public async Task<IActionResult> CreateBoard(string boardName)
        {
            if (string.IsNullOrWhiteSpace(boardName))
            {
                boardName = "Новая доска";
            }

            var newBoard = new KanbanBoard
            {
                Id = Guid.NewGuid(),
                Board = boardName,
                CreatedAt = DateTime.UtcNow,
            };

            await _kanbanService.CreateBoardAsync(newBoard);
            return RedirectToAction(nameof(KanbanView));
        }

        [HttpPost]
        public async Task<IActionResult> RenameBoard(Guid id, string newName)
        {
            if (!string.IsNullOrWhiteSpace(newName))
            {
                var board = await _kanbanService.GetBoardByIdAsync(id);
                if (board != null)
                {
                    board.Board = newName;
                    await _kanbanService.UpdateBoardAsync(board);
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Доска не найдена." });
                }
            }
            else
            {
                return Json(new { success = false, message = "Некорректное название доски." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBoard(Guid id)
        {
            var board = await _kanbanService.GetBoardByIdAsync(id);
            if (board != null && !board.IsProtected)
            {
                await _kanbanService.DeleteBoardAsync(id);
            }
            return RedirectToAction(nameof(KanbanView));
        }

        #endregion

        #region Методы для работы с колонками

        [HttpPost]
        public async Task<IActionResult> AddColumn(Guid boardId, string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                return RedirectToAction("KanbanView", new { selectedBoardId = boardId });
            }

            var newColumn = new KanbanColumn
            {
                Id = Guid.NewGuid(),
                KanbanBoardId = boardId,
                Column = columnName,
                ColumnColor = "#ffffff"
            };

            await _kanbanService.AddColumnAsync(newColumn);
            return RedirectToAction("KanbanView", new { selectedBoardId = boardId });
        }

        [HttpPost]
        public async Task<IActionResult> RenameColumn(Guid columnId, string newName)
        {
            if (!string.IsNullOrWhiteSpace(newName))
            {
                await _kanbanService.RenameColumnAsync(columnId, newName);
            }
            return RedirectToAction("KanbanView");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteColumn(Guid columnId)
        {
            await _kanbanService.DeleteColumnAsync(columnId);
            return RedirectToAction("KanbanView");
        }

        [HttpPost]
        public async Task<IActionResult> RenameReorderAndRecolorColumn(Guid columnId, string newName, int newOrder, string newColor)
        {
            if (!string.IsNullOrWhiteSpace(newName))
            {
                var column = await _kanbanService.GetColumnByIdAsync(columnId);
                if (column != null)
                {
                    column.Column = newName;
                    column.Order = newOrder;
                    column.ColumnColor = newColor;
                    await _kanbanService.UpdateColumnAsync(column);
                }
            }
            return RedirectToAction("KanbanView");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateColumnOrder([FromBody] List<ColumnOrderUpdateModel> updatedOrder)
        {
            await _kanbanService.UpdateColumnOrderAsync(updatedOrder);
            return Ok();
        }

        #endregion

        #region Методы для работы с задачами

        [HttpPost]
        public async Task<IActionResult> AddTask(Guid columnId, string taskName, string taskDescription, string taskColor, DateTime? dueDate, string priority)
        {
            if (string.IsNullOrWhiteSpace(taskName) || string.IsNullOrWhiteSpace(taskColor))
            {
                return RedirectToAction("KanbanView", new { selectedBoardId = await _kanbanService.GetBoardIdByColumnIdAsync(columnId) });
            }

            var newTask = new KanbanTask
            {
                TaskName = taskName,
                TaskDescription = taskDescription,
                CreatedAt = DateTime.UtcNow,
                TaskAuthor = User.Identity.Name ?? "Аноним",
                TaskColor = taskColor,
                DueDate = dueDate,
                Priority = priority,
            };

            await _kanbanService.AddTaskToColumnAsync(columnId, newTask);
            return RedirectToAction("KanbanView", new { selectedBoardId = await _kanbanService.GetBoardIdByColumnIdAsync(columnId) });
        }

        [HttpPost]
        public async Task<IActionResult> MoveTask([FromBody] TaskMoveModel model)
        {
            if (model == null || model.TaskId == Guid.Empty || model.NewColumnId == Guid.Empty)
            {
                return BadRequest("Invalid task move request.");
            }

            await _kanbanService.MoveTaskAsync(model.TaskId, model.OldColumnId, model.NewColumnId, model.NewIndex);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> EditTask([FromForm] EditTaskModel model)
        {
            if (model == null || model.TaskId == Guid.Empty)
            {
                return BadRequest("Invalid task edit request.");
            }

            await _kanbanService.EditTaskAsync(model.TaskId, model.TaskName, model.TaskDescription, model.TaskColor, model.DueDate, model.Priority);
            return RedirectToAction("KanbanView"); // Возвращаемся на KanbanView после редактирования задачи
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTask([FromForm] Guid taskId)
        {
            if (taskId == Guid.Empty)
            {
                return BadRequest("Invalid task ID.");
            }

            await _kanbanService.DeleteTaskAsync(taskId);
            return Ok();
        }

        #endregion

        // Модель для получения данных о перемещении задачи
        public class TaskMoveModel
        {
            public Guid TaskId { get; set; }
            public Guid OldColumnId { get; set; }
            public Guid NewColumnId { get; set; }
            public int NewIndex { get; set; }
        }

        // Модель для редактирования задачи
        public class EditTaskModel
        {
            public Guid TaskId { get; set; }
            public Guid ColumnId { get; set; }
            public string TaskName { get; set; }
            public string TaskDescription { get; set; }
            public string TaskColor { get; set; }
            public DateTime? DueDate { get; set; }
            public string Priority { get; set; }
        }
    }
}

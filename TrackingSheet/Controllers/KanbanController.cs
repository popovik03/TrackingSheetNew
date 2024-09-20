using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TrackingSheet.Services;
using TrackingSheet.Models.Kanban;
using static TrackingSheet.Services.KanbanService;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;


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
        public async Task<IActionResult> RenameReorderAndRecolorColumn(Guid columnId, string columnName, int order, string columnColor)
        {
            if (!string.IsNullOrWhiteSpace(columnName))
            {
                var column = await _kanbanService.GetColumnByIdAsync(columnId);
                if (column != null)
                {
                    column.Column = columnName;
                    column.Order = order;
                    column.ColumnColor = columnColor;
                    await _kanbanService.UpdateColumnAsync(column);
                }
                else
                {
                    // Можно вернуть ошибку, если колонка не найдена
                    return NotFound("Колонка не найдена.");
                }
            }
            else
            {
                // Можно вернуть ошибку, если имя колонки некорректно
                return BadRequest("Некорректное название колонки.");
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
        public async Task<IActionResult> AddTask(Guid columnId, string taskName, string taskDescription, string taskColor, DateTime? dueDate, string priority, string taskAuthor)
        {
            if (string.IsNullOrWhiteSpace(taskName) || string.IsNullOrWhiteSpace(taskColor))
            {
                return RedirectToAction("KanbanView", new { selectedBoardId = await _kanbanService.GetBoardIdByColumnIdAsync(columnId) });
            }

            var newTask = new KanbanTask
            {
                TaskName = taskName,
                TaskDescription = taskDescription,
                CreatedAt = DateTimeOffset.Now.DateTime,
                TaskAuthor = taskAuthor ?? "Аноним",
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
        public async Task<IActionResult> EditTask([FromBody] EditTaskModel model)
        {
            if (model == null || model.TaskId == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid task edit request." });
            }

            // Конвертация RowVersion из Base64 строки в byte[]
            byte[] originalRowVersion;
            try
            {
                originalRowVersion = Convert.FromBase64String(model.RowVersion);
            }
            catch (FormatException)
            {
                return BadRequest(new { message = "Invalid RowVersion format." });
            }

            // Десериализуем подзадачи из JSON
            List<KanbanSubtask> updatedSubtasks = new List<KanbanSubtask>();
            if (!string.IsNullOrEmpty(model.SubtasksJson))
            {
                try
                {
                    // Используем кастомный конвертер для обработки RowVersion как строки
                    updatedSubtasks = JsonConvert.DeserializeObject<List<KanbanSubtask>>(model.SubtasksJson, new JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter> { new ByteArrayToBase64Converter() }
                    });
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Ошибка десериализации подзадач: {ex.Message}");
                    return BadRequest(new { message = "Ошибка десериализации подзадач." });
                }
            }

            // Создаём обновлённую задачу
            var updatedTask = new KanbanTask
            {
                Id = model.TaskId,
                TaskName = model.TaskName,
                TaskDescription = model.TaskDescription,
                TaskColor = model.TaskColor,
                DueDate = model.DueDate,
                Priority = model.Priority,
                Subtasks = updatedSubtasks
            };

            try
            {
                await _kanbanService.UpdateTaskAsync(updatedTask, originalRowVersion);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "The task has been modified by another process. Please refresh and try again." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении задачи: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while updating the task." });
            }

            // После успешного обновления, возвращаем новое значение RowVersion
            var task = await _kanbanService.GetTaskByIdAsync(model.TaskId);
            var newRowVersion = Convert.ToBase64String(task.RowVersion);

            return Ok(new { message = "Task updated successfully.", RowVersion = newRowVersion });
        }




        [HttpGet]
        public async Task<IActionResult> GetTaskForEdit(Guid taskId)
        {
            if (taskId == Guid.Empty)
            {
                return BadRequest("Invalid task ID.");
            }

            var task = await _kanbanService.GetTaskByIdAsync(taskId);
            if (task == null)
            {
                return NotFound("Task not found.");
            }

            var model = new EditTaskModel
            {
                TaskId = task.Id,
                ColumnId = task.KanbanColumnId,
                TaskName = task.TaskName,
                TaskDescription = task.TaskDescription,
                TaskColor = task.TaskColor,
                DueDate = task.DueDate,
                Priority = task.Priority,
                TaskAuthor = task.TaskAuthor,
                CreatedAt = task.CreatedAt,
                SubtasksJson = JsonConvert.SerializeObject(task.Subtasks.Select(s => new
                {
                    s.Id,
                    s.SubtaskDescription,
                    s.IsCompleted,
                    RowVersion = Convert.ToBase64String(s.RowVersion)
                }).ToList()),
                RowVersion = Convert.ToBase64String(task.RowVersion)
            };

            return Json(model);
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
            public string TaskAuthor { get; set; }
            public DateTime CreatedAt { get; set; }
            public string SubtasksJson { get; set; } // Добавлено поле для получения подзадач в формате JSON

            public string RowVersion { get; set; } // Добавлено для отслеживания версий 
        }

        


    }
}

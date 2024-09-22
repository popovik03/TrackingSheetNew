using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TrackingSheet.Services;
using TrackingSheet.Models.Kanban;
using static TrackingSheet.Services.KanbanService;
using Newtonsoft.Json;
using TrackingSheet.Data;
using Microsoft.EntityFrameworkCore;


namespace TrackingSheet.Controllers
{
    public class KanbanController : Controller
    {
        private readonly IKanbanService _kanbanService;
        private readonly MVCDbContext _context;

        public KanbanController(IKanbanService kanbanService, MVCDbContext context)
        {
            _kanbanService = kanbanService;
            _context = context; 
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTask(EditTaskModel model)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.SelectMany(x => x.Value.Errors.Select(error => new
                {
                    field = x.Key,
                    errorMessage = error.ErrorMessage
                })).ToList();

                return BadRequest(new { message = "Model validation failed.", errors = errorMessages });
            }

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

            // Десериализуем подзадачи из JSON в DTO
            List<EditTaskSubtaskDto> updatedSubtasksDto = new List<EditTaskSubtaskDto>();
            if (!string.IsNullOrEmpty(model.SubtasksJson))
            {
                try
                {
                    updatedSubtasksDto = JsonConvert.DeserializeObject<List<EditTaskSubtaskDto>>(model.SubtasksJson);
                }
                catch (JsonException ex)
                {
                    return BadRequest(new { message = "Ошибка десериализации подзадач.", error = ex.Message });
                }
            }

            // Конвертируем DTO в KanbanSubtask, включая конвертацию RowVersion
            List<KanbanSubtask> updatedSubtasks = updatedSubtasksDto.Select(dto => new KanbanSubtask
            {
                Id = dto.Id,
                SubtaskDescription = dto.SubtaskDescription,
                IsCompleted = dto.IsCompleted,
                RowVersion = string.IsNullOrEmpty(dto.RowVersion) ? null : Convert.FromBase64String(dto.RowVersion)
            }).ToList();

            // Получаем существующую задачу из базы данных
            var existingTask = await _context.KanbanTasks
                .Include(t => t.Subtasks)
                .FirstOrDefaultAsync(t => t.Id == model.TaskId);

            if (existingTask == null)
            {
                return NotFound(new { message = "Задача не найдена." });
            }

            // Проверка RowVersion для оптимистичной блокировки
            if (!existingTask.RowVersion.SequenceEqual(originalRowVersion))
            {
                return Conflict(new { message = "The task has been modified by another process. Please refresh and try again." });
            }

            // Обновляем основные поля задачи
            existingTask.TaskName = model.TaskName;
            existingTask.TaskDescription = model.TaskDescription;
            existingTask.TaskColor = model.TaskColor;
            existingTask.DueDate = model.DueDate;
            existingTask.Priority = model.Priority;

            // Обновляем подзадачи
            UpdateSubtasks(existingTask, updatedSubtasks);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "The task has been modified by another process. Please refresh and try again." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the task.", error = ex.Message });
            }

            // Получаем обновлённую задачу для ответа клиенту
            var updatedTask = await _context.KanbanTasks
                .Include(t => t.Subtasks)
                .FirstOrDefaultAsync(t => t.Id == model.TaskId);

            var updatedTaskResponse = new
            {
                id = updatedTask.Id,
                taskName = updatedTask.TaskName,
                taskDescription = updatedTask.TaskDescription,
                taskColor = updatedTask.TaskColor,
                dueDate = updatedTask.DueDate?.ToString("yyyy-MM-dd"),
                priority = updatedTask.Priority,
                taskAuthor = updatedTask.TaskAuthor,
                createdAt = updatedTask.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                columnId = updatedTask.KanbanColumnId,
                rowVersion = Convert.ToBase64String(updatedTask.RowVersion),
                subtasks = updatedTask.Subtasks.Select(s => new
                {
                    id = s.Id,
                    subtaskDescription = s.SubtaskDescription,
                    isCompleted = s.IsCompleted,
                    rowVersion = Convert.ToBase64String(s.RowVersion)
                }).ToList(),
                comments = updatedTask.Comments.Select(c => new
                {
                    id = c.Id,
                    commentAuthor = c.CommentAuthor,
                    commentText = c.CommentText,
                    createdAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    rowVersion = Convert.ToBase64String(c.RowVersion)
                }).ToList()
            };

            return Ok(new { message = "Task updated successfully.", updatedTask = updatedTaskResponse });
        }


        private void UpdateSubtasks(KanbanTask existingTask, List<KanbanSubtask> updatedSubtasks)
        {
            // Удаляем подзадачи, которых нет в обновлённом списке
            var updatedSubtaskIds = updatedSubtasks.Select(s => s.Id).ToList();
            var subtasksToRemove = existingTask.Subtasks.Where(s => !updatedSubtaskIds.Contains(s.Id)).ToList();
            _context.KanbanSubtasks.RemoveRange(subtasksToRemove);

            foreach (var subtask in updatedSubtasks)
            {
                if (subtask.Id == Guid.Empty)
                {
                    // Новая подзадача
                    var newSubtask = new KanbanSubtask
                    {
                        Id = Guid.NewGuid(),
                        KanbanTaskId = existingTask.Id,
                        SubtaskDescription = subtask.SubtaskDescription,
                        IsCompleted = subtask.IsCompleted
                        // RowVersion будет автоматически установлено базой данных
                    };
                    _context.KanbanSubtasks.Add(newSubtask);
                }
                else
                {
                    // Обновляем существующую подзадачу
                    var existingSubtask = existingTask.Subtasks.FirstOrDefault(s => s.Id == subtask.Id);
                    if (existingSubtask != null)
                    {
                        existingSubtask.SubtaskDescription = subtask.SubtaskDescription;
                        existingSubtask.IsCompleted = subtask.IsCompleted;
                        // Не присваивайте RowVersion вручную
                    }
                    else
                    {
                        // Если подзадача с таким Id не найдена, создаём новую
                        var newSubtask = new KanbanSubtask
                        {
                            Id = Guid.NewGuid(),
                            KanbanTaskId = existingTask.Id,
                            SubtaskDescription = subtask.SubtaskDescription,
                            IsCompleted = subtask.IsCompleted
                            // RowVersion будет автоматически установлено базой данных
                        };
                        _context.KanbanSubtasks.Add(newSubtask);
                    }
                }
            }
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


        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] AddCommentModel model)
        {
            if (model == null || model.TaskId == Guid.Empty || string.IsNullOrWhiteSpace(model.CommentText) || string.IsNullOrWhiteSpace(model.CommentAuthor))
            {
                return BadRequest(new { message = "Некорректный запрос для добавления комментария." });
            }

            var task = await _kanbanService.GetTaskByIdAsync(model.TaskId);
            if (task == null)
            {
                return NotFound(new { message = "Задача не найдена." });
            }

            // Конвертация RowVersion задачи из Base64 строки в byte[]
            byte[] originalTaskRowVersion;
            try
            {
                originalTaskRowVersion = Convert.FromBase64String(model.RowVersion);
            }
            catch (FormatException)
            {
                return BadRequest(new { message = "Некорректный формат RowVersion." });
            }

            // Проверка RowVersion задачи для оптимистичной конкуренции
            if (!task.RowVersion.SequenceEqual(originalTaskRowVersion))
            {
                return Conflict(new { message = "The task has been modified by another process. Please refresh and try again." });
            }

            var newComment = new KanbanComment
            {
                Id = Guid.NewGuid(),
                KanbanTaskId = task.Id,
                CommentAuthor = model.CommentAuthor,
                CommentText = model.CommentText,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _kanbanService.AddCommentAsync(newComment);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "The task has been modified by another process. Please refresh and try again." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении комментария: {ex.Message}");
                return StatusCode(500, "An error occurred while adding the comment.");
            }

            // Получение добавленного комментария с обновлённым RowVersion
            var addedComment = await _kanbanService.GetCommentByIdAsync(newComment.Id);
            if (addedComment == null)
            {
                return StatusCode(500, new { message = "Unable to retrieve the added comment." });
            }

            var responseComment = new
            {
                Id = addedComment.Id,
                CommentAuthor = addedComment.CommentAuthor,
                CommentText = addedComment.CommentText,
                CreatedAt = addedComment.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                RowVersion = Convert.ToBase64String(addedComment.RowVersion)
            };

            // Получение нового RowVersion задачи
            var newRowVersion = Convert.ToBase64String(task.RowVersion);

            return Ok(new { message = "Комментарий добавлен успешно.", RowVersion = newRowVersion, Comment = responseComment });
        }



        [HttpPost]
        public async Task<IActionResult> DeleteComment([FromBody] DeleteCommentModel model)
        {
            if (model == null || model.CommentId == Guid.Empty)
            {
                return BadRequest(new { message = "Некорректный запрос для удаления комментария." });
            }

            // Конвертация RowVersion комментария из Base64 строки в byte[]
            byte[] originalCommentRowVersion;
            try
            {
                originalCommentRowVersion = Convert.FromBase64String(model.RowVersion);
            }
            catch (FormatException)
            {
                return BadRequest(new { message = "Некорректный формат RowVersion." });
            }

            var comment = await _kanbanService.GetCommentByIdAsync(model.CommentId);
            if (comment == null)
            {
                return NotFound(new { message = "Комментарий не найден." });
            }

            // Проверка RowVersion комментария для оптимистичной конкуренции
            if (!comment.RowVersion.SequenceEqual(originalCommentRowVersion))
            {
                return Conflict(new { message = "The comment has been modified by another process. Please refresh and try again." });
            }

            try
            {
                await _kanbanService.DeleteCommentAsync(comment);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "The comment has been modified by another process. Please refresh and try again." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении комментария: {ex.Message}");
                return StatusCode(500, "An error occurred while deleting the comment.");
            }

            // Получение нового RowVersion задачи (если требуется)
            var task = await _kanbanService.GetTaskByIdAsync(comment.KanbanTaskId);
            var newRowVersion = task != null ? Convert.ToBase64String(task.RowVersion) : "";

            return Ok(new { message = "Комментарий удален успешно.", RowVersion = newRowVersion });
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

        public class AddCommentModel
        {
            public Guid TaskId { get; set; }
            public string CommentAuthor { get; set; }
            public string CommentText { get; set; }
            public string RowVersion { get; set; }
        }

        public class DeleteCommentModel
        {
            public Guid CommentId { get; set; }
            public string RowVersion { get; set; }
        }

        public class EditTaskSubtaskDto
        {
            public Guid Id { get; set; }
            public string SubtaskDescription { get; set; }
            public bool IsCompleted { get; set; }
            public string RowVersion { get; set; } // RowVersion как строка (Base64)
        }


    }
}

using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TrackingSheet.Services;
using TrackingSheet.Models.Kanban;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IActionResult> KanbanView()
        {
            var boards = await _kanbanService.GetAllBoardsAsync();
            return View(boards);
        }



        //Методы для работы с досками 

        [HttpPost]
        public async Task<IActionResult> CreateBoard(string boardName)
        {
            if (string.IsNullOrWhiteSpace(boardName))
            {
                boardName = "Новая доска"; // Название по умолчанию, если ничего не указано
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
        public async Task<IActionResult> DeleteBoard(Guid id)
        {
            var board = await _kanbanService.GetBoardByIdAsync(id);
            if (board != null && !board.IsProtected)
            {
                await _kanbanService.DeleteBoardAsync(id);
            }
            return RedirectToAction(nameof(KanbanView));
        }

        [HttpPost]
        public async Task<IActionResult> RenameBoard(Guid id, string newName)
        {
            var board = await _kanbanService.GetBoardByIdAsync(id);
            if (board != null)
            {
                board.Board = newName;
                await _kanbanService.UpdateBoardAsync(board);
            }
            return RedirectToAction(nameof(KanbanView));
        }

        //Методы для работы с колонками

        [HttpPost]
        public async Task<IActionResult> AddColumn(Guid boardId, string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                return RedirectToAction("KanbanView", new { id = boardId });
            }

            var newColumn = new KanbanColumn
            {
                Id = Guid.NewGuid(),
                KanbanBoardId = boardId,
                Column = columnName,
                ColumnColor = "#ffffff", // Можно задать цвет по умолчанию
                Order = 0 // Пример, можно использовать другое значение для сортировки
            };

            await _kanbanService.AddColumnToBoardAsync(boardId, newColumn);
            return RedirectToAction("KanbanView", new { id = boardId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateColumnOrder([FromBody] List<ColumnOrderUpdateModel> updatedOrder)
        {
            await _kanbanService.UpdateColumnOrderAsync(updatedOrder);
            return Ok();
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



    }
}

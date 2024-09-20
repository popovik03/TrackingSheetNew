using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TrackingSheet.Models.Kanban
{
    public class KanbanBoard
    {
        public Guid Id { get; set; }
        public string Board { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsProtected { get; set; } // Новое поле
        public List<KanbanColumn> Columns { get; set; } = new List<KanbanColumn>();
    }


    public class KanbanColumn
    {
        public Guid Id { get; set; }
        public Guid KanbanBoardId { get; set; }
        public string Column { get; set; } // Название колонки (To-Do, In Progress, Done и т.д.)
        public string ColumnColor { get; set; } // Цвет колонки
        public int Order { get; set; } // Порядок колонки на доске
        public List<KanbanTask> Tasks { get; set; } = new List<KanbanTask>(); // Задачи в колонке
        [JsonIgnore]

        public KanbanBoard KanbanBoard { get; set; }  // Навигационное свойство для связи с доской
    }

    public class KanbanTask
    {
        public Guid Id { get; set; }
        public Guid KanbanColumnId { get; set; }
        public string? TaskAuthor { get; set; } // Создатель задачи
        public string TaskName { get; set; } // Название задачи
        public string TaskDescription { get; set; } // Описание задачи
        public List<KanbanSubtask> Subtasks { get; set; } = new List<KanbanSubtask>(); // Подзадачи
        public List<KanbanComment> Comments { get; set; } = new List<KanbanComment>(); // Комментарии
        public List<KanbanTaskMember> TaskMembers { get; set; } = new List<KanbanTaskMember>(); // Ответственные
        public string TaskColor { get; set; } // Цвет задачи
        public DateTime CreatedAt { get; set; } // Дата создания задачи
        public DateTime? DueDate { get; set; } // Дедлайн задачи
        public string? Status { get; set; } // Статус задачи (Активная, Завершённая и т.д.)
        public int Order { get; set; } // Порядок задачи в колонке
        public string? Priority { get; set; } // Приоритет задачи (Низкий, Средний, Высокий)
        public string? TaskType { get; set; } // Тип задачи (Баг, Фича и т.д.)

        [Timestamp]
        public byte[] RowVersion { get; set; } // Поле для отслеживания версий записи

    }

    public class KanbanSubtask
    {
        public Guid Id { get; set; }
        public Guid KanbanTaskId { get; set; }
        public string SubtaskDescription { get; set; } // Описание подзадачи
        public bool IsCompleted { get; set; } // Статус выполнения подзадачи
        [JsonIgnore]
        public KanbanTask KanbanTask { get; set; } // навигационное свойство для связи с задачей

        [Timestamp]
        public byte[] RowVersion { get; set; } // Поле для отслеживания версий записи
    }

    public class KanbanComment
    {
        public Guid Id { get; set; }
        public Guid KanbanTaskId { get; set; }
        public string CommentAuthor { get; set; } // Автор комментария
        public string CommentText { get; set; } // Текст комментария
        public DateTime CreatedAt { get; set; } // Дата создания комментария

        [JsonIgnore]
        public KanbanTask KanbanTask { get; set; } // навигационное свойство для связи с задачей

        [Timestamp]
        public byte[] RowVersion { get; set; } // Поле для отслеживания версий записи
    }

    public class KanbanMember
    {
        public Guid Id { get; set; }
        public string Name { get; set; } // Имя участника
        public List<KanbanTaskMember> TaskMembers { get; set; } = new List<KanbanTaskMember>(); // Связанные задачи
        [JsonIgnore] // Предотвращает циклические ссылки
        public List<KanbanTask> Tasks { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } // Поле для отслеживания версий записи
    }

    public class KanbanTaskMember
    {
        public Guid KanbanTaskId { get; set; }
        public KanbanTask KanbanTask { get; set; } // Задача

        public Guid KanbanMemberId { get; set; }
        public KanbanMember KanbanMember { get; set; } // Участник

        [Timestamp]
        public byte[] RowVersion { get; set; } // Поле для отслеживания версий записи
    }
}

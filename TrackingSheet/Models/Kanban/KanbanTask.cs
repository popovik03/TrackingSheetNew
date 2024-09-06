namespace TrackingSheet.Models.Kanban
{
    public class KanbanTask
    {
        public Guid Id { get; set; }
        public string Creator { get; set; } // Создающий задачу
        public string Board { get; set; } // Доска с задачами
        public string Status { get; set; } // Название столбца с задачами и сам столбец
        public string Task { get; set; } // Задача в столбце
        public List<KanbanSubtask> Subtasks { get; set; } = new List<KanbanSubtask>();
        public List<KanbanComment> Comments { get; set; } = new List<KanbanComment>();
        public List<KanbanTaskMember> TaskMembers { get; set; } = new List<KanbanTaskMember>();
        public string Color { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class KanbanSubtask
    {
        public Guid Id { get; set; }
        public Guid KanbanTaskId { get; set; }
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
        public KanbanTask KanbanTask { get; set; }
    }

    public class KanbanComment
    {
        public Guid Id { get; set; }
        public Guid KanbanTaskId { get; set; }
        public string Author { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public KanbanTask KanbanTask { get; set; }
    }

    public class KanbanMember
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<KanbanTaskMember> TaskMembers { get; set; } = new List<KanbanTaskMember>();
    }

    public class KanbanTaskMember
    {
        public Guid KanbanTaskId { get; set; }
        public KanbanTask KanbanTask { get; set; }

        public Guid KanbanMemberId { get; set; }
        public KanbanMember KanbanMember { get; set; }
    }
}

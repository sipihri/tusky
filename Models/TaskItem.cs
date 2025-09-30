namespace Tusky.Models;

public class TaskItem
{
    public int Id { get; set; }
    public bool IsCompleted { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }

    public override string ToString()
    {
        char mark = IsCompleted ? 'x' : ' ';
        return $"[{mark}] {Description}";
    }
}
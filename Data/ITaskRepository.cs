using Tusky.Models;

namespace Tusky.Data;

public interface ITaskRepository
{
    int Count { get; }
    
    TaskItem? GetById(int id);
    TaskItem? GetByIndex(int index);
    IReadOnlyList<TaskItem> GetAll();
    
    void Add(TaskItem item);
    void Update(TaskItem item);
    void Delete(int id);
}
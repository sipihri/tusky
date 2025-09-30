using Tusky.Data;
using Tusky.Models;

namespace Tusky.Services;

public class TaskService
{
    private readonly ITaskRepository _repository;
    
    private IReadOnlyList<TaskItem> _tasks;
    
    private FilterMode _currentFilterMode;
    private SortMode _currentSortMode;
    
    public enum FilterMode
    {
        None,
        Completed,
        Incomplete,
    }
    
    public enum SortMode
    {
        None,

        DescriptionAscending,
        StatusAscending,
        DateAscending,

        DescriptionDescending,
        StatusDescending,
        DateDescending,
    }

    public TaskService(ITaskRepository repository)
    {
        _repository = repository;
        _tasks = _repository.GetAll();
    }

    public int Count => _tasks.Count;

    public void Filter(FilterMode filterMode, bool force = false)
    {
        if (force == false && _currentFilterMode == filterMode)
            return;

        _tasks = filterMode switch
        {
            FilterMode.None => _repository.GetAll(),
            FilterMode.Completed => _repository.GetAll().Where(x => x.IsCompleted == false).ToArray(),
            FilterMode.Incomplete => _repository.GetAll().Where(x => x.IsCompleted).ToArray(),
            _ => _tasks
        };
        _currentFilterMode = filterMode;
    }

    public void Sort(SortMode sortMode, bool force = false)
    {
        if (force == false && _currentSortMode == sortMode)
            return;

        _tasks = sortMode switch
        {
            SortMode.DescriptionAscending => _tasks.OrderBy(x => x.Description).ToArray(),
            SortMode.StatusAscending => _tasks.OrderBy(x => x.IsCompleted).ToArray(),
            SortMode.DateAscending => _tasks.OrderBy(x => x.UpdatedAt).ToArray(),
            
            SortMode.DescriptionDescending => _tasks.OrderByDescending(x => x.Description).ToArray(),
            SortMode.StatusDescending => _tasks.OrderByDescending(x => x.IsCompleted).ToArray(),
            SortMode.DateDescending => _tasks.OrderByDescending(x => x.UpdatedAt).ToArray(),
            
            _ => _tasks
        };

        _currentSortMode = sortMode;
    }
    
    public TaskItem? GetTaskByIndex(int index)
    {
        if (index < 0 || index >= _tasks.Count)
            return null;
        
        // return _repository.GetByIndex(index);
        return _tasks[index];
    }

    public void AddTask(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.");
        
        _repository.Add(new TaskItem
        {
            Description = description,
            UpdatedAt = DateTime.Now
        });
        
        Filter(_currentFilterMode, true);
    }

    public void MarkTaskCompleted(int id, bool isCompleted = true)
    {
        TaskItem? task = _repository.GetById(id);
        if (task != null)
        {
            task.IsCompleted = isCompleted;
            task.UpdatedAt = DateTime.Now;
            _repository.Update(task);
            Filter(_currentFilterMode, true);
        }
        else
        {
            throw new ArgumentException($"Task item with ID {id} not found.");
        }
    }

    public void EditTaskDescription(int id, string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newDescription))
        {
            throw new ArgumentException("Description cannot be empty.");
        }
        TaskItem? task = _repository.GetById(id);
        if (task != null)
        {
            task.Description = newDescription;
            task.UpdatedAt = DateTime.Now;
            _repository.Update(task);
            Filter(_currentFilterMode, true);
        }
        else
        {
            throw new ArgumentException($"Task item with ID {id} not found.");
        }
    }

    public void DeleteTask(int id)
    {
        if (_repository.GetById(id) != null)
        {
            _repository.Delete(id);
            Filter(_currentFilterMode, true);
            return;
        }

        throw new ArgumentException($"Task item with ID {id} not found.");
    }
}
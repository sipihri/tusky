using System.Text.Json;
using Tusky.Models;

namespace Tusky.Data;

public class FileSystemTaskRepository : ITaskRepository
{
    private readonly string _filePath;
    private List<TaskItem> _tasks;
    private int _nextId;

    public FileSystemTaskRepository(string filePath)
    {
        _filePath = filePath;
        _tasks = new List<TaskItem>();
        _nextId = 1;
        LoadTasks();
    }

    private void LoadTasks()
    {
        if (File.Exists(_filePath) == false) return;
        
        try
        {
            string jsonString = File.ReadAllText(_filePath);
            
            if (string.IsNullOrWhiteSpace(jsonString))
                return;
            
            var tasks = JsonSerializer.Deserialize<List<TaskItem>>(jsonString);
            if (tasks != null)
            {
                _tasks = tasks.OrderBy(t => t.Id).ToList();
                // ensure _nextId is higher than any existing ID
                _nextId = _tasks.Any() ? _tasks.Max(t => t.Id) + 1 : 1;
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error reading {_filePath}: {ex.Message}. Starting with an empty list.");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error accessing {_filePath}: {ex.Message}. Starting with an empty list.");
        }
    }

    private void SaveTasks()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(_tasks, options);
            File.WriteAllText(_filePath, jsonString);
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error saving tasks to {_filePath}: {ex.Message}");
        }
    }

    public int Count => _tasks.Count;

    public IReadOnlyList<TaskItem> GetAll()
    {
        return _tasks;
    }

    public TaskItem? GetById(int id)
    {
        return _tasks.FirstOrDefault(t => t.Id == id);
    }

    public TaskItem? GetByIndex(int index)
    {
        if (index < 0 || index >= _tasks.Count)
            return null;
        
        return _tasks[index];
    }

    public void Add(TaskItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        item.Id = _nextId++;
        _tasks.Add(item);
        SaveTasks();
    }

    public void Update(TaskItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        TaskItem? existingItem = GetById(item.Id);
        if (existingItem != null)
        {
            existingItem.Description = item.Description;
            existingItem.IsCompleted = item.IsCompleted;
            existingItem.UpdatedAt = item.UpdatedAt;
            SaveTasks();
        }
        else
        {
            throw new KeyNotFoundException($"Task item with ID {item.Id} not found for update.");
        }
    }

    public void Delete(int id)
    {
        int initialCount = _tasks.Count;
        _tasks.RemoveAll(t => t.Id == id);
        
        if (_tasks.Count < initialCount) SaveTasks();
        else throw new KeyNotFoundException($"Task item with ID {id} not found for deletion.");
    }
}
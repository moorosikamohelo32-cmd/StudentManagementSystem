namespace StudentManagementSystem.Models;

/// <summary>Everything the application stores, saved as one JSON file.</summary>
public class AppData
{
    public List<Student> Students { get; set; } = new();
    public List<Module> Modules { get; set; } = new();
    public List<Mark> Marks { get; set; } = new();
}
